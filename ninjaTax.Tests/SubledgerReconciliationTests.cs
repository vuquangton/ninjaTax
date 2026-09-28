using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class SubledgerReconciliationTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public SubledgerReconciliationTests()
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
    public async Task KiemTraDoiSoatToanHeThongAsync_WhenAllSubledgersMatchGl_ReturnsZeroDiscrepancy()
    {
        using var context = new AppDbContext(_dbOptions);
        var subledgerService = new SubledgerReconciliationService(context, NullLogger<SubledgerReconciliationService>.Instance);

        var report = await subledgerService.KiemTraDoiSoatToanHeThongAsync(DateTime.Today);

        Assert.NotNull(report);
        Assert.NotEmpty(report.DanhSachDoiSoat);
        Assert.True(report.ToanBoKhopSoLieu);
        Assert.Equal(0, report.SoMucLech);
    }

    [Fact]
    public async Task KiemTraDoiSoatToanHeThongAsync_WhenArDiscrepancyExists_FlagsDiscrepancyAccurately()
    {
        using var context = new AppDbContext(_dbOptions);
        var subledgerService = new SubledgerReconciliationService(context, NullLogger<SubledgerReconciliationService>.Instance);

        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");

        // Tạo bút toán vào Sổ cái TK 131 nhưng không tạo hóa đơn bán hàng trong subledger (gây lệch sổ)
        var bt = new ButToan
        {
            SoChungTu = "PKT-LECH-AR",
            NgayHachToan = DateTime.Today,
            NgayChungTu = DateTime.Today,
            DienGiai = "Bút toán trực tiếp không qua hóa đơn",
            TongTien = 15_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk131.Id, TaiKhoanCoId = tk511.Id, SoTien = 15_000_000m });
        context.ButToans.Add(bt);
        await context.SaveChangesAsync();

        var report = await subledgerService.KiemTraDoiSoatToanHeThongAsync(DateTime.Today);

        var arCheck = report.DanhSachDoiSoat.FirstOrDefault(d => d.MaTaiKhoan == "131");
        Assert.NotNull(arCheck);
        Assert.False(arCheck.KhopSoLieu);
        Assert.Equal(15_000_000m, arCheck.ChenhLech);
        Assert.False(report.ToanBoKhopSoLieu);
    }
}
