using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class TaxAuditRiskShieldTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public TaxAuditRiskShieldTests()
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
    public async Task QuetBayThue_PhatHienAmQuyTienMat_BaoDongDo()
    {
        using var context = new AppDbContext(_dbOptions);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);
        var taxService = new TaxFinalizationService(context, reportService, NullLogger<TaxFinalizationService>.Instance);
        var shieldService = new TaxAuditShieldService(context, taxService, reportService, NullLogger<TaxAuditShieldService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Chi tiền mặt khi quỹ đang bằng 0: Nợ 331 / Có 1111: 15,000,000 đ
        var btChiAm = new ButToan
        {
            SoChungTu = "PC-AMQUY",
            SoChungTuGoc = "GOC-AM",
            NgayChungTu = new DateTime(2026, 7, 10),
            NgayChungTuGoc = new DateTime(2026, 7, 10),
            NgayHachToan = new DateTime(2026, 7, 10),
            DienGiai = "Chi tiền mặt quá số tồn",
            TongTien = 15_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btChiAm.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk331.Id, TaiKhoanCoId = tk111.Id, SoTien = 15_000_000m });
        context.ButToans.Add(btChiAm);
        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(btChiAm.Id);

        var report = await shieldService.QuetToanBoBayThueAsync(2026);

        Assert.True(report.SoCanhBaoDoNghiemTrong > 0);
        var bay2 = report.PhatHiens.FirstOrDefault(p => p.LoaiBay == LoaiBayThue.Bay2_AmQuyTienMatThoiDiem);
        Assert.NotNull(bay2);
        Assert.Equal(MucDoRuiRo.Cao_CanhBaoDo, bay2.MucDo);
        Assert.Equal(15_000_000m, bay2.SoTienViPham);
    }

    [Fact]
    public async Task QuetBayThue_PhatHienHoaDonTren20TrTienMat_BaoDongDo()
    {
        using var context = new AppDbContext(_dbOptions);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);
        var taxService = new TaxFinalizationService(context, reportService, NullLogger<TaxFinalizationService>.Instance);
        var shieldService = new TaxAuditShieldService(context, taxService, reportService, NullLogger<TaxAuditShieldService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");

        // Chi mua hàng 35 triệu bằng tiền mặt
        var bt = new ButToan
        {
            SoChungTu = "PC-35M",
            SoChungTuGoc = "HD-35M",
            NgayChungTu = new DateTime(2026, 8, 1),
            NgayChungTuGoc = new DateTime(2026, 8, 1),
            NgayHachToan = new DateTime(2026, 8, 1),
            DienGiai = "Mua hàng hóa thanh toán tiền mặt 35M",
            TongTien = 35_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        bt.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk156.Id, TaiKhoanCoId = tk111.Id, SoTien = 35_000_000m });
        context.ButToans.Add(bt);
        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(bt.Id);

        var report = await shieldService.QuetToanBoBayThueAsync(2026);

        var bay1 = report.PhatHiens.FirstOrDefault(p => p.LoaiBay == LoaiBayThue.Bay1_HoaDonTren20TrTienMat);
        Assert.NotNull(bay1);
        Assert.Equal(MucDoRuiRo.Cao_CanhBaoDo, bay1.MucDo);
        Assert.Equal(35_000_000m, bay1.SoTienViPham);
        Assert.Equal(7_000_000m, bay1.SoTienThueRuiRo); // 20% của 35M
    }
}
