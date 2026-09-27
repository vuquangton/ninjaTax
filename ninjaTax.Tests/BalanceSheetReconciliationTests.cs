using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class BalanceSheetReconciliationTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public BalanceSheetReconciliationTests()
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
    public async Task LapBaoCaoB01_CanDoiTuyetDoi_TongTaiSanBangTongNguonVon()
    {
        using var context = new AppDbContext(_dbOptions);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk411 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "4111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");

        // 1. Góp vốn: Nợ 1111 / Có 4111: 500,000,000 đ
        var btVon = new ButToan
        {
            SoChungTu = "PKT-001",
            SoChungTuGoc = "VON-001",
            NgayChungTu = new DateTime(2026, 1, 1),
            NgayChungTuGoc = new DateTime(2026, 1, 1),
            NgayHachToan = new DateTime(2026, 1, 1),
            DienGiai = "Góp vốn điều lệ",
            TongTien = 500_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btVon.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk411.Id, SoTien = 500_000_000m });
        context.ButToans.Add(btVon);
        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(btVon.Id);

        // 2. Mua hàng nhập kho bằng tiền mặt: Nợ 1561 / Có 1111: 200,000,000 đ
        var btMua = new ButToan
        {
            SoChungTu = "PKT-002",
            SoChungTuGoc = "MUA-001",
            NgayChungTu = new DateTime(2026, 2, 1),
            NgayChungTuGoc = new DateTime(2026, 2, 1),
            NgayHachToan = new DateTime(2026, 2, 1),
            DienGiai = "Mua hàng hóa nhập kho",
            TongTien = 200_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btMua.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk156.Id, TaiKhoanCoId = tk111.Id, SoTien = 200_000_000m });
        context.ButToans.Add(btMua);
        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(btMua.Id);

        // 3. Bán hàng thu tiền ngay: Nợ 1111 / Có 5111: 300,000,000 đ; Giá vốn: Nợ 632 / Có 1561: 150,000,000 đ
        var btBan = new ButToan
        {
            SoChungTu = "PKT-003",
            SoChungTuGoc = "BAN-001",
            NgayChungTu = new DateTime(2026, 3, 1),
            NgayChungTuGoc = new DateTime(2026, 3, 1),
            NgayHachToan = new DateTime(2026, 3, 1),
            DienGiai = "Bán hàng thu tiền mặt và ghi nhận giá vốn",
            TongTien = 450_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btBan.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk511.Id, SoTien = 300_000_000m });
        btBan.ChiTietButToans.Add(new ChiTietButToan { DongSo = 2, TaiKhoanNoId = tk632.Id, TaiKhoanCoId = tk156.Id, SoTien = 150_000_000m });
        context.ButToans.Add(btBan);
        await context.SaveChangesAsync();
        await butToanService.GhiSoAsync(btBan.Id);

        // Kiểm tra B01
        var b01 = await reportService.LapBaoCaoB01Async(2026);

        Assert.True(b01.IsCanDoi);
        Assert.Equal(b01.TongTaiSanCuoiNam, b01.TongNguonVonCuoiNam);
        Assert.True(b01.TongTaiSanCuoiNam > 0);
    }

    [Fact]
    public async Task TaiKhoanLuongTinh_131_BocTachHaiBen_KhongBuTruCheo()
    {
        using var context = new AppDbContext(_dbOptions);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");

        var dtA = new DoiTuong { MaDoiTuong = "KH_A", TenDoiTuong = "Khách Hàng A", Loai = LoaiDoiTuong.KhachHang };
        var dtB = new DoiTuong { MaDoiTuong = "KH_B", TenDoiTuong = "Khách Hàng B", Loai = LoaiDoiTuong.KhachHang };
        context.DoiTuongs.AddRange(dtA, dtB);
        await context.SaveChangesAsync();

        // KH A nợ tiền mua hàng: Nợ 131 (KH A) / Có 5111: 80,000,000 đ
        var btA = new ButToan
        {
            SoChungTu = "PKT-131-A",
            SoChungTuGoc = "GOC-A",
            NgayChungTu = new DateTime(2026, 4, 1),
            NgayChungTuGoc = new DateTime(2026, 4, 1),
            NgayHachToan = new DateTime(2026, 4, 1),
            DienGiai = "Bán chịu cho KH A",
            TongTien = 80_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btA.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk131.Id, TaiKhoanCoId = tk511.Id, DoiTuongId = dtA.Id, SoTien = 80_000_000m });
        context.ButToans.Add(btA);

        // KH B ứng trước tiền hàng: Nợ 1111 / Có 131 (KH B): 30,000,000 đ
        var btB = new ButToan
        {
            SoChungTu = "PKT-131-B",
            SoChungTuGoc = "GOC-B",
            NgayChungTu = new DateTime(2026, 4, 2),
            NgayChungTuGoc = new DateTime(2026, 4, 2),
            NgayHachToan = new DateTime(2026, 4, 2),
            DienGiai = "KH B ứng trước tiền",
            TongTien = 30_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        btB.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk131.Id, DoiTuongId = dtB.Id, SoTien = 30_000_000m });
        context.ButToans.Add(btB);
        await context.SaveChangesAsync();

        await butToanService.GhiSoAsync(btA.Id);
        await butToanService.GhiSoAsync(btB.Id);

        var b01 = await reportService.LapBaoCaoB01Async(2026);

        // Phải thu KH (Mã 131) phải là 80M, KHÔNG PHẢI 50M (nếu bị cấn trừ sai)
        var ma131 = b01.ChiTiets.First(c => c.MaSo == "131").SoCuoiNam;
        // Người mua trả tiền trước (Mã 312) phải là 30M
        var ma312 = b01.ChiTiets.First(c => c.MaSo == "312").SoCuoiNam;

        Assert.Equal(80_000_000m, ma131);
        Assert.Equal(30_000_000m, ma312);
        Assert.True(b01.IsCanDoi);
    }
}
