using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class CorporateIncomeTax80PercentTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public CorporateIncomeTax80PercentTests()
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
    public async Task QuyetToanTndn_ViPham80PhanTram_Phat003PhanTramMoiNgay()
    {
        using var context = new AppDbContext(_dbOptions);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);
        var taxService = new TaxFinalizationService(context, reportService, NullLogger<TaxFinalizationService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk3334 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "3334");

        // 1. Kinh doanh tạo LNTT = 1,000,000,000 đ -> Thuế TNDN 20% = 200,000,000 đ
        var btKd = new ButToan
        {
            SoChungTu = "PKT-TNDN-01",
            SoChungTuGoc = "KD",
            NgayChungTu = new DateTime(2026, 6, 1),
            NgayChungTuGoc = new DateTime(2026, 6, 1),
            NgayHachToan = new DateTime(2026, 6, 1),
            DienGiai = "Doanh thu và giá vốn",
            TongTien = 1_500_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btKd.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk511.Id, SoTien = 1_500_000_000m });
        btKd.ChiTietButToans.Add(new ChiTietButToan { DongSo = 2, TaiKhoanNoId = tk632.Id, TaiKhoanCoId = tk156.Id, SoTien = 500_000_000m });
        context.ButToans.Add(btKd);

        // 2. Doanh nghiệp chỉ tạm nộp 100,000,000 đ (50%) < Ngưỡng 80% (160,000,000 đ)
        // Nộp thuế TNDN tạm tính: Nợ 3334 / Có 1111: 100,000,000 đ
        var btTamNop = new ButToan
        {
            SoChungTu = "PKT-TNDN-02",
            SoChungTuGoc = "TAMNOP",
            NgayChungTu = new DateTime(2026, 10, 20),
            NgayChungTuGoc = new DateTime(2026, 10, 20),
            NgayHachToan = new DateTime(2026, 10, 20),
            DienGiai = "Tạm nộp thuế TNDN quý 3",
            TongTien = 100_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btTamNop.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk3334.Id, TaiKhoanCoId = tk111.Id, SoTien = 100_000_000m });
        context.ButToans.Add(btTamNop);

        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(btKd.Id);
        await butToanService.GhiSoAsync(btTamNop.Id);

        var qtt = await taxService.LapQuyetToanTndnAsync(2026);

        Assert.Equal(200_000_000m, qtt.ChiTieuC7_ThueTndnPhaiNop);
        Assert.Equal(160_000_000m, qtt.Nguong80PhanTram);
        Assert.Equal(100_000_000m, qtt.TongTamNop4Quy);
        Assert.True(qtt.ViPham80PhanTram);
        Assert.Equal(60_000_000m, qtt.SoTienNopThieu);
        Assert.True(qtt.TienPhatChamNopDuKien > 0);
    }

    [Fact]
    public async Task QuyetToanTndn_BocTachChiPhiB4_HoaDonTren20TrTienMat()
    {
        using var context = new AppDbContext(_dbOptions);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);
        var taxService = new TaxFinalizationService(context, reportService, NullLogger<TaxFinalizationService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "6422");

        // Chi phí quản lý mua bằng tiền mặt 50,000,000 đ (>= 20 triệu không hợp lệ)
        var btChi = new ButToan
        {
            SoChungTu = "PKT-B4-01",
            SoChungTuGoc = "HD01",
            NgayChungTu = new DateTime(2026, 5, 10),
            NgayChungTuGoc = new DateTime(2026, 5, 10),
            NgayHachToan = new DateTime(2026, 5, 10),
            DienGiai = "Mua thiết bị văn phòng tiền mặt 50 triệu",
            TongTien = 50_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btChi.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk642.Id, TaiKhoanCoId = tk111.Id, SoTien = 50_000_000m });
        context.ButToans.Add(btChi);

        // Doanh thu 100,000,000 đ
        var btDt = new ButToan
        {
            SoChungTu = "PKT-B4-02",
            SoChungTuGoc = "DT01",
            NgayChungTu = new DateTime(2026, 5, 15),
            NgayChungTuGoc = new DateTime(2026, 5, 15),
            NgayHachToan = new DateTime(2026, 5, 15),
            DienGiai = "Doanh thu bán hàng",
            TongTien = 100_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btDt.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk511.Id, SoTien = 100_000_000m });
        context.ButToans.Add(btDt);

        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(btChi.Id);
        await butToanService.GhiSoAsync(btDt.Id);

        var qtt = await taxService.LapQuyetToanTndnAsync(2026);

        // Lợi nhuận kế toán A1 = 100M - 50M = 50M
        Assert.Equal(50_000_000m, qtt.ChiTieuA1_LoiNhuanKeToan);
        // B4 tự động bóc tách khoản 50M
        Assert.Equal(50_000_000m, qtt.ChiTieuB4_ChiPhiKhongDuocTru);
        // Thu nhập chịu thuế B14 = A1 + B4 = 100M
        Assert.Equal(100_000_000m, qtt.ChiTieuB14_ThuNhapChiuThue);
        // Thuế TNDN C7 = 100M * 20% = 20M
        Assert.Equal(20_000_000m, qtt.ChiTieuC7_ThueTndnPhaiNop);
    }
}
