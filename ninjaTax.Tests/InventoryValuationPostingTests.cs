using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class InventoryValuationPostingTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public InventoryValuationPostingTests()
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

    private InventoryService CreateInventoryService(AppDbContext context)
    {
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        return new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);
    }

    [Fact]
    public async Task GhiSoPhieuNhapKho_AutoCreatesDoubleEntryGL_UpdatesLatestPurchasePrice()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-VAL-01",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LoaiNhapKho = LoaiNhapKho.MuaNgoai,
            NhaCungCapId = ncc.Id,
            TongSoLuong = 10m,
            TongTienHang = 15_000_000m,
            TrangThai = TrangThaiPhieuKho.TamTinh,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGia = 1_500_000m,
                    ThanhTien = 15_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };

        var saved = await inventoryService.SavePhieuNhapKhoAsync(pnk);
        var posted = await inventoryService.GhiSoPhieuNhapKhoAsync(saved.Id);

        Assert.Equal(TrangThaiPhieuKho.DaGhiSo, posted.TrangThai);
        Assert.NotNull(posted.ButToanId);

        // Kiểm tra Bút toán sinh ra
        var bt = await context.ButToans
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanNo)
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanCo)
            .FirstAsync(b => b.Id == posted.ButToanId.Value);

        Assert.Equal(TrangThaiButToan.DaGhiSo, bt.TrangThai);
        Assert.Equal(15_000_000m, bt.TongTien);
        Assert.Equal(bt.TongNo, bt.TongCo);

        // Bất biến: Tuyệt đối không dùng TK 911
        Assert.DoesNotContain(bt.ChiTietButToans, c => c.TaiKhoanNo?.MaTaiKhoan == "911" || c.TaiKhoanCo?.MaTaiKhoan == "911");

        // Kiểm tra cập nhật giá mua gần nhất
        var vtRefreshed = await context.VatTuHangHoas.FindAsync(vatTu.Id);
        Assert.NotNull(vtRefreshed);
        Assert.Equal(1_500_000m, vtRefreshed.DonGiaMuaGanNhat);
    }

    [Fact]
    public async Task CalculateWeightedAverageCost_MultipleBatches_CalculatesCorrectPeriodicWeightedAverage()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Lô 1: 10 cái x 100.000 = 1.000.000đ
        var pnk1 = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-BATCH-01",
            TongSoLuong = 10m,
            TongTienHang = 1_000_000m,
            TrangThai = TrangThaiPhieuKho.TamTinh,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGia = 100_000m,
                    ThanhTien = 1_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await inventoryService.SavePhieuNhapKhoAsync(pnk1);
        await inventoryService.GhiSoPhieuNhapKhoAsync(pnk1.Id);

        // Lô 2: 20 cái x 130.000 = 2.600.000đ
        var pnk2 = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-BATCH-02",
            TongSoLuong = 20m,
            TongTienHang = 2_600_000m,
            TrangThai = TrangThaiPhieuKho.TamTinh,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 20m,
                    DonGia = 130_000m,
                    ThanhTien = 2_600_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await inventoryService.SavePhieuNhapKhoAsync(pnk2);
        await inventoryService.GhiSoPhieuNhapKhoAsync(pnk2.Id);

        // Đơn giá BQGQ = (1.000.000 + 2.600.000) / (10 + 20) = 3.600.000 / 30 = 120.000đ
        var avgCost = await inventoryService.CalculateWeightedAverageCostAsync(kho.Id, vatTu.Id, DateTime.Today);
        Assert.Equal(120_000m, avgCost);
    }

    [Fact]
    public async Task GhiSoPhieuXuatKho_AutoCalculatesCOGS_CreatesDoubleEntryGL_AttachesDepartment()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var pbKd = await context.PhongBans.FirstAsync(p => p.LoaiPhongBan == LoaiPhongBan.BanHang);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");

        // 1. Nhập 30 cái x 120.000 = 3.600.000đ
        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-XK-TEST",
            TongSoLuong = 30m,
            TongTienHang = 3_600_000m,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 30m,
                    DonGia = 120_000m,
                    ThanhTien = 3_600_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await inventoryService.SavePhieuNhapKhoAsync(pnk);
        await inventoryService.GhiSoPhieuNhapKhoAsync(pnk.Id);

        // 2. Lập phiếu xuất 10 cái (đơn vị chưa nhập đơn giá vốn -> DonGiaVon = 0)
        var pxk = new PhieuXuatKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PXK-VAL-01",
            LoaiXuatKho = LoaiXuatKho.BanHang,
            KhachHangId = kh.Id,
            PhongBanId = pbKd.Id, // Gắn phân bổ chi phí phòng kinh doanh
            TongSoLuong = 10m,
            TongTienGiaVon = 0m,
            ChiTietXuatKhos = new List<ChiTietXuatKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGiaVon = 0m, // Sẽ tự động tính BQGQ
                    TienGiaVon = 0m,
                    TaiKhoanNoId = tk632.Id,
                    TaiKhoanCoId = tk156.Id
                }
            }
        };

        var savedPxk = await inventoryService.SavePhieuXuatKhoAsync(pxk);
        var postedPxk = await inventoryService.GhiSoPhieuXuatKhoAsync(savedPxk.Id);

        Assert.Equal(TrangThaiPhieuKho.DaGhiSo, postedPxk.TrangThai);
        Assert.Single(postedPxk.ChiTietXuatKhos);

        var line = postedPxk.ChiTietXuatKhos.First();
        Assert.Equal(120_000m, line.DonGiaVon);
        Assert.Equal(1_200_000m, line.TienGiaVon); // 10 x 120.000 = 1.200.000đ
        Assert.Equal(1_200_000m, postedPxk.TongTienGiaVon);

        // Kiểm tra Bút toán Sổ Cái
        Assert.NotNull(postedPxk.ButToanId);
        var bt = await context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstAsync(b => b.Id == postedPxk.ButToanId.Value);

        Assert.Equal(1_200_000m, bt.TongTien);
        Assert.Equal(pbKd.Id, bt.ChiTietButToans.First().PhongBanId);

        // Kiểm tra tồn kho sau khi xuất: 30 - 10 = 20
        var remaining = await inventoryService.GetStockBalanceAsync(kho.Id, vatTu.Id);
        Assert.Equal(20m, remaining);
    }

    [Fact]
    public async Task HuyGhiSoPhieuNhapKho_FailsIfCausesNegativeStock()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");

        // 1. Nhập kho 10 cái
        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-CAN-TEST",
            TongSoLuong = 10m,
            TongTienHang = 1_000_000m,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGia = 100_000m,
                    ThanhTien = 1_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await inventoryService.SavePhieuNhapKhoAsync(pnk);
        await inventoryService.GhiSoPhieuNhapKhoAsync(pnk.Id);

        // 2. Xuất kho 8 cái (Còn lại 2 cái)
        var pxk = new PhieuXuatKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PXK-CAN-TEST",
            TongSoLuong = 8m,
            TongTienGiaVon = 800_000m,
            ChiTietXuatKhos = new List<ChiTietXuatKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 8m,
                    DonGiaVon = 100_000m,
                    TienGiaVon = 800_000m,
                    TaiKhoanNoId = tk632.Id,
                    TaiKhoanCoId = tk156.Id
                }
            }
        };
        await inventoryService.SavePhieuXuatKhoAsync(pxk);
        await inventoryService.GhiSoPhieuXuatKhoAsync(pxk.Id);

        // 3. Cố gắng hủy phiếu nhập 10 cái -> Tồn 2 mà giảm 10 sẽ âm 8 -> Phải ném ngoại lệ chặn!
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            inventoryService.HuyGhiSoPhieuNhapKhoAsync(pnk.Id));

        Assert.Contains("âm kho", ex.Message);
    }
}
