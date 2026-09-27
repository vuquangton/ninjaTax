using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class StockBalanceEngineTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public StockBalanceEngineTests()
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
    public async Task GetStockBalance_NoVouchers_ReturnsZero()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);

        var balance = await inventoryService.GetStockBalanceAsync(kho.Id, vatTu.Id);
        Assert.Equal(0m, balance);
    }

    [Fact]
    public async Task GetStockBalance_WithPostedInward_IncreasesStock()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // 1. Phiếu nhập 25 cái đã ghi sổ
        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-TEST-01",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            TongSoLuong = 25m,
            TongTienHang = 25_000_000m,
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 25m,
                    DonGia = 1_000_000m,
                    ThanhTien = 25_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };

        await context.PhieuNhapKhos.AddAsync(pnk);
        await context.SaveChangesAsync();

        var balance = await inventoryService.GetStockBalanceAsync(kho.Id, vatTu.Id);
        Assert.Equal(25m, balance);
    }

    [Fact]
    public async Task GetStockBalance_DraftVoucher_DoesNotAffectStock()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Phiếu nhập tạm tính (TamTinh)
        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-DRAFT-01",
            TongSoLuong = 50m,
            TongTienHang = 50_000_000m,
            TrangThai = TrangThaiPhieuKho.TamTinh,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 50m,
                    DonGia = 1_000_000m,
                    ThanhTien = 50_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };

        await context.PhieuNhapKhos.AddAsync(pnk);
        await context.SaveChangesAsync();

        var balance = await inventoryService.GetStockBalanceAsync(kho.Id, vatTu.Id);
        Assert.Equal(0m, balance);
    }

    [Fact]
    public async Task ValidateStockAvailability_InsufficientStock_ThrowsInvalidOperationException()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Nhập vào 10 cái
        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-AVAIL-01",
            TongSoLuong = 10m,
            TongTienHang = 10_000_000m,
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGia = 1_000_000m,
                    ThanhTien = 10_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await context.PhieuNhapKhos.AddAsync(pnk);
        await context.SaveChangesAsync();

        // Thử yêu cầu xuất 15 cái (Vượt tồn kho 10 cái) -> Phải ném InvalidOperationException
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            inventoryService.ValidateStockAvailabilityAsync(kho.Id, vatTu.Id, 15m, DateTime.Today));

        Assert.Contains("xuất âm kho", ex.Message);
    }

    [Fact]
    public async Task ValidateStockAvailability_SufficientStock_DoesNotThrow()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Nhập vào 10 cái
        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-AVAIL-02",
            TongSoLuong = 10m,
            TongTienHang = 10_000_000m,
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGia = 1_000_000m,
                    ThanhTien = 10_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await context.PhieuNhapKhos.AddAsync(pnk);
        await context.SaveChangesAsync();

        // Yêu cầu xuất đúng 10 cái -> Hợp lệ
        await inventoryService.ValidateStockAvailabilityAsync(kho.Id, vatTu.Id, 10m, DateTime.Today);

        // Yêu cầu xuất 5 cái -> Hợp lệ
        await inventoryService.ValidateStockAvailabilityAsync(kho.Id, vatTu.Id, 5m, DateTime.Today);
    }

    [Fact]
    public async Task SaveWarehouse_DuplicateCode_ThrowsArgumentException()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);
        var chiNhanh = await context.ChiNhanhs.FirstAsync();

        var dupKho = new Kho
        {
            ChiNhanhId = chiNhanh.Id,
            MaKho = "KHO-TONG", // Trùng mã đã seed
            TenKho = "Kho Trùng Lặp"
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => inventoryService.SaveWarehouseAsync(dupKho));
        Assert.Contains("đã tồn tại", ex.Message);
    }
}
