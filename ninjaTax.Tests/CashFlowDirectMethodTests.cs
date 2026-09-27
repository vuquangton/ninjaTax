using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class CashFlowDirectMethodTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public CashFlowDirectMethodTests()
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
    public async Task LapBaoCaoB03_KhopSoDuTienCuoiKy_VoiSoCai111Va112()
    {
        using var context = new AppDbContext(_dbOptions);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk112 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1121");
        var tk411 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "4111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");
        var tk334 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "334");

        // 1. Nhận vốn góp: Nợ 1121 / Có 4111: 400,000,000 đ (HĐTC)
        var btVon = new ButToan
        {
            SoChungTu = "PKT-CF-01",
            SoChungTuGoc = "VON",
            NgayChungTu = new DateTime(2026, 1, 15),
            NgayChungTuGoc = new DateTime(2026, 1, 15),
            NgayHachToan = new DateTime(2026, 1, 15),
            DienGiai = "Góp vốn vào tài khoản ngân hàng",
            TongTien = 400_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btVon.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk112.Id, TaiKhoanCoId = tk411.Id, SoTien = 400_000_000m });
        context.ButToans.Add(btVon);

        // 2. Thu tiền bán hàng: Nợ 1111 / Có 5111: 100,000,000 đ (HĐKD - Mã 01)
        var btBan = new ButToan
        {
            SoChungTu = "PKT-CF-02",
            SoChungTuGoc = "THU",
            NgayChungTu = new DateTime(2026, 2, 10),
            NgayChungTuGoc = new DateTime(2026, 2, 10),
            NgayHachToan = new DateTime(2026, 2, 10),
            DienGiai = "Thu tiền mặt bán hàng",
            TongTien = 100_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btBan.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk511.Id, SoTien = 100_000_000m });
        context.ButToans.Add(btBan);

        // 3. Chi trả tiền nhà cung cấp: Nợ 331 / Có 1111: 40,000,000 đ (HĐKD - Mã 02)
        var btTraNcc = new ButToan
        {
            SoChungTu = "PKT-CF-03",
            SoChungTuGoc = "CHI-NCC",
            NgayChungTu = new DateTime(2026, 2, 20),
            NgayChungTuGoc = new DateTime(2026, 2, 20),
            NgayHachToan = new DateTime(2026, 2, 20),
            DienGiai = "Chi tiền trả nhà cung cấp",
            TongTien = 40_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btTraNcc.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk331.Id, TaiKhoanCoId = tk111.Id, SoTien = 40_000_000m });
        context.ButToans.Add(btTraNcc);

        // 4. Chi trả lương nhân viên: Nợ 334 / Có 1121: 25,000,000 đ (HĐKD - Mã 03)
        var btTraLuong = new ButToan
        {
            SoChungTu = "PKT-CF-04",
            SoChungTuGoc = "CHI-LUONG",
            NgayChungTu = new DateTime(2026, 3, 5),
            NgayChungTuGoc = new DateTime(2026, 3, 5),
            NgayHachToan = new DateTime(2026, 3, 5),
            DienGiai = "Chuyển khoản thanh toán lương",
            TongTien = 25_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btTraLuong.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk334.Id, TaiKhoanCoId = tk112.Id, SoTien = 25_000_000m });
        context.ButToans.Add(btTraLuong);

        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(btVon.Id);
        await butToanService.GhiSoAsync(btBan.Id);
        await butToanService.GhiSoAsync(btTraNcc.Id);
        await butToanService.GhiSoAsync(btTraLuong.Id);

        var b03 = await reportService.LapBaoCaoB03Async(2026);

        // Kiểm tra tính cân đối và khớp sổ cái
        Assert.True(b03.IsKhopSoCai);
        Assert.Equal(b03.TienCuoiKy, b03.DuNoTk111Va112);
        // Lưu chuyển thuần trong kỳ: (100M - 40M - 25M) [HĐKD 35M] + [HĐTC 400M] = 435M
        Assert.Equal(435_000_000m, b03.LuuChuyenThuanTrongKy);
        Assert.Equal(435_000_000m, b03.TienCuoiKy);
    }
}
