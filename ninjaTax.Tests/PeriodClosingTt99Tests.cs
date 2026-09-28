using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class PeriodClosingTt99Tests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public PeriodClosingTt99Tests()
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
    public async Task TaoButToanKetChuyenAsync_StrictlyNoAccount911_AndTransfersDirectlyTo4212()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var periodClosingService = new PeriodClosingService(context, butToanService, NullLogger<PeriodClosingService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "6421");

        // 1. Ghi nhận doanh thu bán hàng: Nợ 1111 (100M) / Có 5111 (100M)
        var bt1 = new ButToan
        {
            SoChungTu = "PKT-BANHANG-01",
            NgayHachToan = new DateTime(2026, 1, 15),
            NgayChungTu = new DateTime(2026, 1, 15),
            DienGiai = "Doanh thu bán lẻ",
            TongTien = 100_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt1.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk511.Id, SoTien = 100_000_000m });
        context.ButToans.Add(bt1);

        // 2. Ghi nhận giá vốn: Nợ 632 (60M) / Có 1561 (60M)
        var bt2 = new ButToan
        {
            SoChungTu = "PKT-GIAVON-01",
            NgayHachToan = new DateTime(2026, 1, 15),
            NgayChungTu = new DateTime(2026, 1, 15),
            DienGiai = "Giá vốn hàng bán",
            TongTien = 60_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt2.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk632.Id, TaiKhoanCoId = tk156.Id, SoTien = 60_000_000m });
        context.ButToans.Add(bt2);

        // 3. Ghi nhận chi phí quản lý: Nợ 6421 (10M) / Có 1111 (10M)
        var bt3 = new ButToan
        {
            SoChungTu = "PKT-CHIPHI-01",
            NgayHachToan = new DateTime(2026, 1, 20),
            NgayChungTu = new DateTime(2026, 1, 20),
            DienGiai = "Chi phí văn phòng phẩm",
            TongTien = 10_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt3.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk642.Id, TaiKhoanCoId = tk111.Id, SoTien = 10_000_000m });
        context.ButToans.Add(bt3);

        await context.SaveChangesAsync();

        // Chạy kết chuyển Tháng 1/2026
        var ketQua = await periodClosingService.TaoButToanKetChuyenAsync(2026, 1);

        Assert.True(ketQua.ThanhCong);
        Assert.NotNull(ketQua.ButToanKetChuyenId);
        Assert.Equal(100_000_000m, ketQua.TongDoanhThuKetChuyen);
        Assert.Equal(70_000_000m, ketQua.TongChiPhiKetChuyen);
        Assert.Equal(30_000_000m, ketQua.LoiNhuanSauThueKetChuyen);

        // Kiểm tra chứng từ sinh ra trong DB
        var butToanKc = await context.ButToans
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanNo)
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanCo)
            .FirstOrDefaultAsync(b => b.Id == ketQua.ButToanKetChuyenId);

        Assert.NotNull(butToanKc);
        Assert.Equal(TrangThaiButToan.DaGhiSo, butToanKc.TrangThai);

        // CHUẨN THÔNG TƯ 99/2025/TT-BTC: BẮT BUỘC HẠCH TOÁN QUA TÀI KHOẢN 911
        var allAccountCodes = butToanKc.ChiTietButToans
            .SelectMany(c => new[] { c.TaiKhoanNo!.MaTaiKhoan, c.TaiKhoanCo!.MaTaiKhoan })
            .ToList();

        Assert.Contains(allAccountCodes, a => a.StartsWith("911"));
        Assert.Contains(allAccountCodes, a => a.StartsWith("4212"));

        // Kiểm tra TK 911 cân đối tuyệt đối giữa Nợ và Có (Số dư cuối kỳ = 0)
        var psNo911 = butToanKc.ChiTietButToans
            .Where(c => c.TaiKhoanNo!.MaTaiKhoan.StartsWith("911"))
            .Sum(c => c.SoTien);

        var psCo911 = butToanKc.ChiTietButToans
            .Where(c => c.TaiKhoanCo!.MaTaiKhoan.StartsWith("911"))
            .Sum(c => c.SoTien);

        Assert.Equal(psNo911, psCo911);
        Assert.Equal(100_000_000m, psNo911); // Tổng kết chuyển DT = 100M
    }

    [Fact]
    public async Task TaoButToanKetChuyenAsync_KhiDaKhoaSo_NemLoiKhongChoKetChuyen()
    {
        using var context = new AppDbContext(_dbOptions);
        var companyService = new CompanyService(context, NullLogger<CompanyService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var periodClosingService = new PeriodClosingService(context, butToanService, NullLogger<PeriodClosingService>.Instance);

        // Khóa sổ đến hết ngày 31/01/2026
        await companyService.LockBookToDateAsync(new DateTime(2026, 1, 31));

        // Thử kết chuyển tháng 1/2026
        var ketQua = await periodClosingService.TaoButToanKetChuyenAsync(2026, 1);

        Assert.False(ketQua.ThanhCong);
        Assert.Contains("khóa sổ", ketQua.ThongBao);
    }
}
