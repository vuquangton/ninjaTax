using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class CommercialAdjustmentServiceTests
{
    private async Task<AppDbContext> CreateInMemoryDbContextAsync()
    {
        var dbName = $"mem_commercial_adj_test_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:{dbName}?mode=memory&cache=shared")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public async Task HangBanTraLai_CreatesDoubleEntryAndRestoresInventory()
    {
        using var context = await CreateInMemoryDbContextAsync();
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var adjustmentService = new CommercialAdjustmentService(context, butToanService, NullLogger<CommercialAdjustmentService>.Instance);

        var branch = await context.ChiNhanhs.FirstAsync();
        var customer = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var warehouse = await context.Khos.FirstAsync(k => k.ChiNhanhId == branch.Id);

        var product = new VatTuHangHoa
        {
            MaVatTu = "PROD-A",
            TenVatTu = "Sản phẩm A",
            DonViTinh = "Cái",
            LoaiVatTu = LoaiVatTuHangHoa.HangHoa,
            DangHoatDong = true,
            DangTheoDoiTonKho = true
        };
        context.VatTuHangHoas.Add(product);
        await context.SaveChangesAsync();

        var chungTu = new ChungTuDieuChinhThuongMai
        {
            ChiNhanhId = branch.Id,
            LoaiDieuChinh = LoaiDieuChinhThuongMai.HangBanTraLai,
            SoChungTu = "HBTL-2026-001",
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            DoiTuongId = customer.Id,
            KhoId = warehouse.Id,
            HinhThucXuLy = HinhThucXuLyDieuChinh.GiamTruCongNo,
            LyDo = "Khách trả hàng do lỗi kỹ thuật",
            ChiTietDieuChinhs = new List<ChiTietDieuChinhThuongMai>
            {
                new ChiTietDieuChinhThuongMai
                {
                    VatTuHangHoaId = product.Id,
                    DonViTinh = "Cái",
                    SoLuong = 2,
                    DonGia = 15_000_000,
                    ThueSuatVat = 10,
                    DonGiaVonNhapLai = 10_000_000
                }
            }
        };

        var result = await adjustmentService.TaoChungTuAsync(chungTu);

        Assert.NotNull(result);
        Assert.Equal(TrangThaiDieuChinhThuongMai.DaGhiSo, result.TrangThai);
        Assert.Equal(30_000_000, result.TongTienHang);
        Assert.Equal(3_000_000, result.TongTienThueVat);
        Assert.Equal(33_000_000, result.TongThanhToan);
        Assert.Equal(20_000_000, result.TongGiaTriNhapLaiKho);

        // Bút toán giảm trừ doanh thu: Nợ 5212 = 30M, Nợ 33311 = 3M / Có 131 = 33M
        Assert.NotNull(result.ButToanDoanhThuCongNoId);
        var btDT = await context.ButToans.Include(b => b.ChiTietButToans).FirstOrDefaultAsync(b => b.Id == result.ButToanDoanhThuCongNoId);
        Assert.NotNull(btDT);
        Assert.Equal(33_000_000, btDT.TongNo);
        Assert.Equal(33_000_000, btDT.TongCo);

        // Bút toán nhập lại kho: Nợ 1561 = 20M / Có 632 = 20M
        Assert.NotNull(result.ButToanGiaVonKhoId);
        var btGV = await context.ButToans.Include(b => b.ChiTietButToans).FirstOrDefaultAsync(b => b.Id == result.ButToanGiaVonKhoId);
        Assert.NotNull(btGV);
        Assert.Equal(20_000_000, btGV.TongNo);
        Assert.Equal(20_000_000, btGV.TongCo);

        // Tự động sinh phiếu nhập kho tương ứng
        var pnk = await context.PhieuNhapKhos.FirstOrDefaultAsync(p => p.SoPhieu == "PNK-HBTL-2026-001");
        Assert.NotNull(pnk);
        Assert.Equal(2, pnk.TongSoLuong);
        Assert.Equal(20_000_000, pnk.TongTienHang);
    }

    [Fact]
    public async Task ChietKhauThuongMaiBan_PostsTo5211AndClearsTo511AtPeriodEnd()
    {
        using var context = await CreateInMemoryDbContextAsync();
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var adjustmentService = new CommercialAdjustmentService(context, butToanService, NullLogger<CommercialAdjustmentService>.Instance);
        var closingService = new PeriodClosingService(context, butToanService, NullLogger<PeriodClosingService>.Instance);

        var branch = await context.ChiNhanhs.FirstAsync();
        var customer = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk5111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");

        // 1. Giả lập phát sinh doanh thu bán hàng 100M: Nợ 131 / Có 5111
        var btDoanhThu = new ButToan
        {
            SoChungTu = "HDBH-TEST-001",
            SoChungTuGoc = "HDBH-TEST-001",
            NgayChungTuGoc = DateTime.Today,
            NgayHachToan = DateTime.Today,
            NgayChungTu = DateTime.Today,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new ChiTietButToan
                {
                    DongSo = 1,
                    TaiKhoanNoId = tk131.Id,
                    TaiKhoanCoId = tk5111.Id,
                    SoTien = 100_000_000,
                    DienGiai = "Doanh thu bán hàng"
                }
            }
        };
        var (btOk, btMsg, _) = await butToanService.TaoMoiAsync(btDoanhThu);
        Assert.True(btOk, $"Tao btDoanhThu failed: {btMsg}");

        // 2. Lập chiết khấu thương mại 10M (VAT 10% = 1M): Nợ 5211 = 10M, Nợ 33311 = 1M / Có 131 = 11M
        var cktm = new ChungTuDieuChinhThuongMai
        {
            ChiNhanhId = branch.Id,
            LoaiDieuChinh = LoaiDieuChinhThuongMai.ChietKhauThuongMaiBan,
            SoChungTu = "CKTM-2026-001",
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            DoiTuongId = customer.Id,
            HinhThucXuLy = HinhThucXuLyDieuChinh.GiamTruCongNo,
            LyDo = "Chiết khấu doanh số quý 1",
            ChiTietDieuChinhs = new List<ChiTietDieuChinhThuongMai>
            {
                new ChiTietDieuChinhThuongMai
                {
                    SoLuong = 1,
                    DonGia = 10_000_000,
                    ThueSuatVat = 10
                }
            }
        };

        var result = await adjustmentService.TaoChungTuAsync(cktm);
        Assert.NotNull(result);
        Assert.NotNull(result.ButToanDoanhThuCongNoId);

        // 3. Chạy kết chuyển cuối kỳ:
        // Bước 0: Kết chuyển Nợ 5111 / Có 5211: 10M
        // Bước 1: Doanh thu thuần 90M kết chuyển Nợ 5111 / Có 911: 90M
        var closingResult = await closingService.TaoButToanKetChuyenAsync(DateTime.Today.Year, DateTime.Today.Month);

        Assert.True(closingResult.ThanhCong);
        Assert.NotNull(closingResult.ButToanKetChuyenId);

        var btClosing = await context.ButToans.Include(b => b.ChiTietButToans).FirstOrDefaultAsync(b => b.Id == closingResult.ButToanKetChuyenId);
        Assert.NotNull(btClosing);

        // Kiểm tra dòng kết chuyển giảm trừ sang 511
        var lineGiamTru = btClosing.ChiTietButToans.FirstOrDefault(c => c.TaiKhoanNoId == tk5111.Id && c.SoTien == 10_000_000);
        Assert.NotNull(lineGiamTru);

        // Kiểm tra dòng doanh thu thuần sang 911 (100M - 10M = 90M)
        var tk911 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "911");
        var lineDtThuan = btClosing.ChiTietButToans.FirstOrDefault(c => c.TaiKhoanNoId == tk5111.Id && c.TaiKhoanCoId == tk911.Id);
        Assert.NotNull(lineDtThuan);
        Assert.Equal(90_000_000, lineDtThuan.SoTien);
    }
}
