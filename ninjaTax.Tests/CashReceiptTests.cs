using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class CashReceiptTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public CashReceiptTests()
    {
        _sqliteConnection = new SqliteConnection("Data Source=:memory:");
        _sqliteConnection.Open();

        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        using var context = new AppDbContext(_dbOptions);
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
    }

    public void Dispose()
    {
        _sqliteConnection.Dispose();
    }

    [Fact]
    public async Task TaoVaGhiSoPhieuThu_IncreasesCashBalance_AndGeneratesBalancedGlVoucher()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var congNoService = new CongNoService(context, NullLogger<CongNoService>.Instance);
        var thuChiService = new ThuChiService(context, butToanService, congNoService, NullLogger<ThuChiService>.Instance);

        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);

        var model = new ThuChiCreateViewModel
        {
            LoaiChungTu = LoaiChungTuThuChi.ThuTienMat,
            NgayChungTu = new DateTime(2026, 9, 10),
            NgayHachToan = new DateTime(2026, 9, 10),
            DoiTuongId = kh.Id,
            NguoiGiaoNopNhan = "Nguyễn Văn A",
            LyDo = "Thu tiền mặt khách hàng trả nợ",
            ChiTiets = new List<ChiTietThuChiItemViewModel>
            {
                new()
                {
                    DienGiai = "Thu tiền nợ theo hóa đơn",
                    TaiKhoanNoId = tk1111.Id,
                    TaiKhoanCoId = tk131.Id,
                    SoTien = 15000000m,
                    DoiTuongId = kh.Id
                }
            }
        };

        // 1. Tạo phiếu thu
        var chungTu = await thuChiService.TaoChungTuThuChiAsync(model);
        Assert.NotNull(chungTu);
        Assert.StartsWith("PT-2026-", chungTu.SoChungTu);
        Assert.Equal(15000000m, chungTu.TongTien);
        Assert.Equal(TrangThaiThuChi.ChoGhiSo, chungTu.TrangThai);

        // 2. Ghi sổ phiếu thu
        var ghiSo = await thuChiService.GhiSoChungTuAsync(chungTu.Id);
        Assert.Equal(TrangThaiThuChi.DaGhiSo, ghiSo.TrangThai);
        Assert.NotNull(ghiSo.ButToanId);

        // 3. Kiểm tra Bút toán Sổ Cái GL tương ứng
        var bt = await context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstOrDefaultAsync(b => b.Id == ghiSo.ButToanId);

        Assert.NotNull(bt);
        Assert.Equal(TrangThaiButToan.DaGhiSo, bt.TrangThai);
        Assert.Equal(15000000m, bt.TongNo);
        Assert.Equal(15000000m, bt.TongCo);
        Assert.Equal(bt.TongNo, bt.TongCo); // Invariant TT99

        // Tuyệt đối không dùng TK 911 cho phiếu thu thông thường
        Assert.DoesNotContain(bt.ChiTietButToans, c => c.TaiKhoanNo?.MaTaiKhoan == "911" || c.TaiKhoanCo?.MaTaiKhoan == "911");

        // 4. Kiểm tra tồn quỹ tiền mặt đã tăng lên 15.000.000 VNĐ
        var tonQuy = await thuChiService.TinhTonQuyKhaDungAsync(new DateTime(2026, 9, 10));
        Assert.Equal(15000000m, tonQuy);
    }

    [Fact]
    public async Task TaoBaoCoNganHang_AndSettlesSalesInvoice_UpdatesArSuccessfully()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var congNoService = new CongNoService(context, NullLogger<CongNoService>.Instance);
        var thuChiService = new ThuChiService(context, butToanService, congNoService, NullLogger<ThuChiService>.Instance);

        var tk1121 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1121");
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);

        // Tạo tài khoản ngân hàng doanh nghiệp
        var tkNh = await thuChiService.TaoTaiKhoanNganHangAsync(new TaiKhoanNganHang
        {
            SoTaiKhoan = "001100223344",
            TenNganHang = "Vietcombank Sở Giao Dịch",
            SoDuBanDau = 50000000m,
            DangHoatDong = true
        });

        // Tạo Hóa đơn bán hàng Phase 2
        var hd = new HoaDonBanHang
        {
            SoChungTu = "BH-001",
            SoHoaDon = "0000001",
            KhachHangId = kh.Id,
            NgayHoaDon = new DateTime(2026, 9, 1),
            TongTienHang = 20000000m,
            TongTienThueVat = 2000000m,
            TongThanhToan = 22000000m,
            DaThuTien = 0m
        };
        context.HoaDonBanHangs.Add(hd);
        await context.SaveChangesAsync();

        var model = new ThuChiCreateViewModel
        {
            LoaiChungTu = LoaiChungTuThuChi.BaoCoNganHang,
            NgayChungTu = new DateTime(2026, 9, 15),
            NgayHachToan = new DateTime(2026, 9, 15),
            TaiKhoanNganHangId = tkNh.Id,
            DoiTuongId = kh.Id,
            HoaDonBanHangId = hd.Id,
            LyDo = "Khách hàng chuyển khoản thanh toán HĐ 0000001",
            ChiTiets = new List<ChiTietThuChiItemViewModel>
            {
                new()
                {
                    DienGiai = "Báo Có ngân hàng tiền về",
                    TaiKhoanNoId = tk1121.Id,
                    TaiKhoanCoId = tk131.Id,
                    SoTien = 22000000m,
                    DoiTuongId = kh.Id
                }
            }
        };

        var chungTu = await thuChiService.TaoChungTuThuChiAsync(model);
        Assert.StartsWith("BC-2026-", chungTu.SoChungTu);

        // Ghi sổ
        await thuChiService.GhiSoChungTuAsync(chungTu.Id);

        // Kiểm tra số dư tài khoản ngân hàng: 50M ban đầu + 22M thu = 72M
        var soDuNh = await thuChiService.TinhTonQuyKhaDungAsync(new DateTime(2026, 9, 15), tkNh.Id);
        Assert.Equal(72000000m, soDuNh);

        // Kiểm tra Hóa đơn bán hàng đã được tất toán công nợ
        var hdSauThu = await context.HoaDonBanHangs.FindAsync(hd.Id);
        Assert.NotNull(hdSauThu);
        Assert.Equal(22000000m, hdSauThu.DaThuTien);
        Assert.Equal(0m, hdSauThu.ConPhaiThu);
    }

    [Fact]
    public async Task BaoCaoSoQuy_CalculatesBalancesAndMovementsAccurately()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var congNoService = new CongNoService(context, NullLogger<CongNoService>.Instance);
        var thuChiService = new ThuChiService(context, butToanService, congNoService, NullLogger<ThuChiService>.Instance);

        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "642");

        // Giao dịch 1: Thu 10.000.000 đ vào ngày 2026-09-05
        var pt1 = await thuChiService.TaoChungTuThuChiAsync(new ThuChiCreateViewModel
        {
            LoaiChungTu = LoaiChungTuThuChi.ThuTienMat,
            NgayChungTu = new DateTime(2026, 9, 5),
            NgayHachToan = new DateTime(2026, 9, 5),
            LyDo = "Thu tiền bán lẻ",
            ChiTiets = new() { new() { TaiKhoanNoId = tk1111.Id, TaiKhoanCoId = tk131.Id, SoTien = 10000000m } }
        });
        await thuChiService.GhiSoChungTuAsync(pt1.Id);

        // Giao dịch 2: Chi 3.000.000 đ vào ngày 2026-09-08
        var pc1 = await thuChiService.TaoChungTuThuChiAsync(new ThuChiCreateViewModel
        {
            LoaiChungTu = LoaiChungTuThuChi.ChiTienMat,
            NgayChungTu = new DateTime(2026, 9, 8),
            NgayHachToan = new DateTime(2026, 9, 8),
            LyDo = "Chi tiếp khách",
            ChiTiets = new() { new() { TaiKhoanNoId = tk642.Id, TaiKhoanCoId = tk1111.Id, SoTien = 3000000m } }
        });
        await thuChiService.GhiSoChungTuAsync(pc1.Id);

        // Lấy Sổ Quỹ từ 2026-09-01 đến 2026-09-30
        var soQuy = await thuChiService.LayBaoCaoSoQuyAsync("1111", new DateTime(2026, 9, 1), new DateTime(2026, 9, 30));

        Assert.Equal(0m, soQuy.SoDuDauKy);
        Assert.Equal(10000000m, soQuy.TongThuTrongKy);
        Assert.Equal(3000000m, soQuy.TongChiTrongKy);
        Assert.Equal(7000000m, soQuy.SoDuCuoiKy);
        Assert.Equal(2, soQuy.DongSoQuys.Count);
    }
}
