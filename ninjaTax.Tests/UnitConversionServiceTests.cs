using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class UnitConversionServiceTests
{
    private async Task<AppDbContext> CreateInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=file:mem_unit_conversion_test?mode=memory&cache=shared")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public void QuyDoiVeDonViCoBan_PhepNhan_CalculatesAccurately()
    {
        // 1 Thùng = 24 Lon -> 5 Thùng = 120 Lon
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=:memory:").Options);
        var service = new UnitConversionService(context);

        var result = service.QuyDoiVeDonViCoBan(5m, 24m, PhepTinhQuyDoi.Nhan);
        Assert.Equal(120m, result);
    }

    [Fact]
    public void QuyDoiTuDonViCoBan_PhepNhan_CalculatesAccurately()
    {
        // 48 Lon -> 48 / 24 = 2 Thùng
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=:memory:").Options);
        var service = new UnitConversionService(context);

        var result = service.QuyDoiTuDonViCoBan(48m, 24m, PhepTinhQuyDoi.Nhan);
        Assert.Equal(2m, result);
    }

    [Fact]
    public async Task SaveConversionUnitAsync_RejectsDuplicateOrInvalidUnits()
    {
        using var context = await CreateInMemoryDbContextAsync();
        var service = new UnitConversionService(context);

        var vatTu = new VatTuHangHoa
        {
            MaVatTu = "BIA-SG",
            TenVatTu = "Bia Sài Gòn Special",
            DonViTinh = "Lon",
            LoaiVatTu = LoaiVatTuHangHoa.HangHoa,
            DangHoatDong = true
        };
        context.VatTuHangHoas.Add(vatTu);
        await context.SaveChangesAsync();

        // 1. Tên trùng với đơn vị tính cơ bản ("Lon") -> Bị từ chối
        var unitSameAsBase = new DonViTinhQuyDoi
        {
            VatTuHangHoaId = vatTu.Id,
            TenDonViTinh = "Lon",
            TyLeQuyDoi = 1m
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveConversionUnitAsync(unitSameAsBase));

        // 2. Thêm đơn vị Thùng hợp lệ
        var unitThung = new DonViTinhQuyDoi
        {
            VatTuHangHoaId = vatTu.Id,
            TenDonViTinh = "Thùng",
            TyLeQuyDoi = 24m,
            PhepTinh = PhepTinhQuyDoi.Nhan
        };
        var saved = await service.SaveConversionUnitAsync(unitThung);
        Assert.True(saved.Id > 0);

        // 3. Thêm trùng đơn vị "Thùng" lần nữa -> Bị từ chối
        var unitDuplicate = new DonViTinhQuyDoi
        {
            VatTuHangHoaId = vatTu.Id,
            TenDonViTinh = "Thùng",
            TyLeQuyDoi = 24m
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveConversionUnitAsync(unitDuplicate));
    }
}
