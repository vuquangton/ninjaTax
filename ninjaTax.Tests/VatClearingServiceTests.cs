using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class VatClearingServiceTests
{
    private async Task<AppDbContext> CreateDatabaseContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:memdb_vat_{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public async Task KhauTruThueGtgtAsync_InputLessThanOutput_ClearsExactInputAmount()
    {
        // Arrange
        using var context = await CreateDatabaseContextAsync();
        var butToanService = new ButToanService(context, new NullLogger<ButToanService>());
        var closingService = new PeriodClosingService(context, butToanService, new NullLogger<PeriodClosingService>());

        var tk1331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1331");
        var tk33311 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "33311");
        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");

        // Đầu vào phát sinh Nợ 1331 = 50,000,000
        var btVao = new ButToan
        {
            SoChungTu = "HD-MUA-01",
            NgayHachToan = new DateTime(2026, 3, 10),
            TongTien = 50000000,
            TongNo = 50000000,
            TongCo = 50000000,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new ChiTietButToan { TaiKhoanNoId = tk1331.Id, TaiKhoanCoId = tk111.Id, SoTien = 50000000 }
            }
        };

        // Đầu ra phát sinh Có 33311 = 70,000,000
        var btRa = new ButToan
        {
            SoChungTu = "HD-BAN-01",
            NgayHachToan = new DateTime(2026, 3, 20),
            TongTien = 70000000,
            TongNo = 70000000,
            TongCo = 70000000,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new ChiTietButToan { TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk33311.Id, SoTien = 70000000 }
            }
        };

        context.ButToans.AddRange(btVao, btRa);
        await context.SaveChangesAsync();

        // Act: Khấu trừ thuế tháng 3/2026
        var result = await closingService.KhauTruThueGtgtAsync(2026, 3);

        // Assert
        Assert.True(result.ThanhCong);
        // Khấu trừ = Min(50M, 70M) = 50,000,000
        Assert.Equal(50000000m, result.SoTienKhauTru);
        Assert.NotNull(result.ButToanId);

        var voucher = await context.ButToans.Include(b => b.ChiTietButToans).FirstOrDefaultAsync(b => b.Id == result.ButToanId.Value);
        Assert.NotNull(voucher);
        Assert.Equal("PKT-KT-THUE-202603", voucher.SoChungTu);
        Assert.Equal(50000000m, voucher.TongTien);
        Assert.Single(voucher.ChiTietButToans);
        Assert.Equal(tk33311.Id, voucher.ChiTietButToans.First().TaiKhoanNoId);
        Assert.Equal(tk1331.Id, voucher.ChiTietButToans.First().TaiKhoanCoId);
    }
}
