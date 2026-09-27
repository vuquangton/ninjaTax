using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class DashboardServiceTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public DashboardServiceTests()
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
    public async Task GetKpisAsync_ReturnsRevenueExpenseReceivablePayable()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new DashboardService(context);

        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan.StartsWith("511"));
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan.StartsWith("642"));
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");
        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan.StartsWith("111"));

        var dt = await context.DoiTuongs.FirstAsync();

        // Seed but toan
        var bt = new ButToan
        {
            SoChungTu = "BT_DASH",
            SoChungTuGoc = "GOC_DASH",
            NgayChungTuGoc = DateTime.Today,
            NgayHachToan = DateTime.Today,
            NgayChungTu = DateTime.Today,
            DienGiai = "Test Dashboard",
            TongTien = 15000000,
            TongNo = 15000000,
            TongCo = 15000000,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                // Revenue: Co 511
                new ChiTietButToan { TaiKhoanNoId = tk131.Id, TaiKhoanCoId = tk511.Id, SoTien = 10000000, DoiTuongId = dt.Id },
                // Expense: No 642
                new ChiTietButToan { TaiKhoanNoId = tk642.Id, TaiKhoanCoId = tk111.Id, SoTien = 5000000 },
                // Payable: Co 331
                new ChiTietButToan { TaiKhoanNoId = tk642.Id, TaiKhoanCoId = tk331.Id, SoTien = 2000000, DoiTuongId = dt.Id }
            }
        };

        context.ButToans.Add(bt);
        await context.SaveChangesAsync();

        // Act
        var kpis = await service.GetKpisAsync();

        // Assert
        Assert.True(kpis.DoanhThu >= 10000000);
        Assert.True(kpis.ChiPhi >= 5000000);
        Assert.True(kpis.PhaiThu >= 10000000);
        Assert.True(kpis.PhaiTra >= 2000000);
    }
}
