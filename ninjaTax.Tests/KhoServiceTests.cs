using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class KhoServiceTests
{
    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:memdb_kho_{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;
    }

    private async Task<AppDbContext> GetDatabaseContextAsync()
    {
        var options = CreateNewContextOptions();
        var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public async Task SaveWarehouseAsync_KhoHopLe_ThanhCongVaVietHoaMa()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var inventoryService = new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);

        var branch = await context.ChiNhanhs.FirstAsync();
        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");

        var kho = new Kho
        {
            ChiNhanhId = branch.Id,
            MaKho = " kho_da_nang ",
            TenKho = "Kho Hàng Đà Nẵng",
            DiaChi = "123 Nguyễn Văn Linh, Đà Nẵng",
            TaiKhoanKhoMacDinhId = tk1561.Id,
            DangHoatDong = true
        };

        // Act
        var result = await inventoryService.SaveWarehouseAsync(kho);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("KHO_DA_NANG", result.MaKho);
        Assert.Equal("Kho Hàng Đà Nẵng", result.TenKho);

        var saved = await context.Khos.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal("KHO_DA_NANG", saved.MaKho);
    }

    [Fact]
    public async Task SaveWarehouseAsync_TrungMaKhoTrongChiNhanh_BaoLoi()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var inventoryService = new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);

        var existingKho = await context.Khos.FirstAsync();

        var duplicateKho = new Kho
        {
            ChiNhanhId = existingKho.ChiNhanhId,
            MaKho = existingKho.MaKho,
            TenKho = "Kho Trùng Lặp"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => inventoryService.SaveWarehouseAsync(duplicateKho));
    }

    [Fact]
    public async Task DeleteWarehouseAsync_ChuaCoPhatSinh_XoaThanhCong()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var inventoryService = new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);

        var branch = await context.ChiNhanhs.FirstAsync();
        var kho = new Kho
        {
            ChiNhanhId = branch.Id,
            MaKho = "KHO_TEMP",
            TenKho = "Kho Tạm",
            DangHoatDong = true
        };
        context.Khos.Add(kho);
        await context.SaveChangesAsync();

        // Act
        var result = await inventoryService.DeleteWarehouseAsync(kho.Id);

        // Assert
        Assert.True(result.Success, result.Message);
        Assert.Null(await context.Khos.FindAsync(kho.Id));
    }

    [Fact]
    public async Task DeleteWarehouseAsync_DaCoPhieuNhapKho_TuChoiVaBaoToanDuLieu()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var inventoryService = new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);

        var existingKho = await context.Khos.FirstAsync();
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);

        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = existingKho.ChiNhanhId,
            KhoId = existingKho.Id,
            NhaCungCapId = ncc.Id,
            SoPhieu = "PNK-TEST-KHO",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today
        };
        context.PhieuNhapKhos.Add(pnk);
        await context.SaveChangesAsync();

        // Act
        var result = await inventoryService.DeleteWarehouseAsync(existingKho.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("đã phát sinh phiếu nhập/xuất kho", result.Message);
        Assert.NotNull(await context.Khos.FindAsync(existingKho.Id));
    }
}
