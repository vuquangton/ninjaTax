using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class TaiKhoanNganHangServiceTests
{
    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:memdb_tknh_{Guid.NewGuid()}?mode=memory&cache=shared")
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
    public async Task TaoMoiAsync_ThongTinHopLe_ThanhCong()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var service = new TaiKhoanNganHangService(context);

        var tk1121 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1121");

        var model = new TaiKhoanNganHangCreateEditViewModel
        {
            SoTaiKhoan = " 0011001234567 ",
            TenNganHang = "Vietcombank",
            ChiNhanh = "Sở Giao Dịch Hà Nội",
            ChuTaiKhoan = "CÔNG TY CP CÔNG NGHỆ NINJATAX",
            SoDuBanDau = 150000000,
            TaiKhoanKeToanId = tk1121.Id,
            DangHoatDong = true
        };

        // Act
        var result = await service.TaoMoiAsync(model);

        // Assert
        Assert.True(result.ThanhCong, result.ThongBao);
        Assert.NotNull(result.BankAccountId);

        var created = await context.TaiKhoanNganHangs.FindAsync(result.BankAccountId);
        Assert.NotNull(created);
        Assert.Equal("0011001234567", created.SoTaiKhoan);
        Assert.Equal("Vietcombank", created.TenNganHang);
    }

    [Fact]
    public async Task TaoMoiAsync_TrungSoTaiKhoanTrongNganHang_BaoLoi()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        context.TaiKhoanNganHangs.Add(new TaiKhoanNganHang
        {
            SoTaiKhoan = "19033445566",
            TenNganHang = "Techcombank",
            DangHoatDong = true
        });
        await context.SaveChangesAsync();

        var service = new TaiKhoanNganHangService(context);
        var model = new TaiKhoanNganHangCreateEditViewModel
        {
            SoTaiKhoan = "19033445566",
            TenNganHang = "Techcombank"
        };

        // Act
        var result = await service.TaoMoiAsync(model);

        // Assert
        Assert.False(result.ThanhCong);
        Assert.Contains("đã tồn tại", result.ThongBao);
    }

    [Fact]
    public async Task XoaAsync_ChuaCoPhatSinh_XoaThanhCong()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var tknh = new TaiKhoanNganHang
        {
            SoTaiKhoan = "999888777",
            TenNganHang = "MBBank",
            DangHoatDong = true
        };
        context.TaiKhoanNganHangs.Add(tknh);
        await context.SaveChangesAsync();

        var service = new TaiKhoanNganHangService(context);

        // Act
        var result = await service.XoaAsync(tknh.Id);

        // Assert
        Assert.True(result.ThanhCong);
        Assert.Null(await context.TaiKhoanNganHangs.FindAsync(tknh.Id));
    }

    [Fact]
    public async Task CapNhatAsync_ThongTinHopLe_CapNhatThanhCong()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var tknh = new TaiKhoanNganHang
        {
            SoTaiKhoan = "123456789",
            TenNganHang = "BIDV",
            ChiNhanh = "Cũ",
            DangHoatDong = true
        };
        context.TaiKhoanNganHangs.Add(tknh);
        await context.SaveChangesAsync();

        var service = new TaiKhoanNganHangService(context);
        var editModel = new TaiKhoanNganHangCreateEditViewModel
        {
            Id = tknh.Id,
            SoTaiKhoan = "123456789",
            TenNganHang = "BIDV",
            ChiNhanh = "Chi nhánh Quang Trung",
            DangHoatDong = true
        };

        // Act
        var result = await service.CapNhatAsync(tknh.Id, editModel);

        // Assert
        Assert.True(result.ThanhCong);
        var updated = await context.TaiKhoanNganHangs.FindAsync(tknh.Id);
        Assert.NotNull(updated);
        Assert.Equal("Chi nhánh Quang Trung", updated.ChiNhanh);
    }
}
