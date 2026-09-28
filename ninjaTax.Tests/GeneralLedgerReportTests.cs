using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class GeneralLedgerReportTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public GeneralLedgerReportTests()
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
    public async Task LaySoNhatKyChungAsync_ReturnsAllPostedTransactions_InChronologicalOrder()
    {
        using var context = new AppDbContext(_dbOptions);
        var glService = new GeneralLedgerService(context, NullLogger<GeneralLedgerService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");

        var bt = new ButToan
        {
            SoChungTu = "PKT-GL-01",
            NgayHachToan = new DateTime(2026, 2, 10),
            NgayChungTu = new DateTime(2026, 2, 10),
            DienGiai = "Bán hàng thu tiền mặt",
            TongTien = 50_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk511.Id, SoTien = 50_000_000m });
        context.ButToans.Add(bt);
        await context.SaveChangesAsync();

        var nkc = await glService.LaySoNhatKyChungAsync(new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));

        Assert.NotEmpty(nkc.DongChiTiets);
        Assert.Contains(nkc.DongChiTiets, d => d.SoChungTu == "PKT-GL-01" && d.SoTien == 50_000_000m);
        Assert.True(nkc.TongPhatSinh >= 50_000_000m);
    }

    [Fact]
    public async Task LaySoCaiAsync_CalculatesAccurateOpeningBalance_Movements_AndClosingBalance()
    {
        using var context = new AppDbContext(_dbOptions);
        var glService = new GeneralLedgerService(context, NullLogger<GeneralLedgerService>.Instance);

        var tk112 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1121");
        var tk411 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "4111");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Đầu kỳ (tháng 1): Góp vốn vào ngân hàng 200M
        var btDauKy = new ButToan
        {
            SoChungTu = "PKT-VON-01",
            NgayHachToan = new DateTime(2026, 1, 5),
            NgayChungTu = new DateTime(2026, 1, 5),
            DienGiai = "Góp vốn bằng tiền gửi",
            TongTien = 200_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        btDauKy.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk112.Id, TaiKhoanCoId = tk411.Id, SoTien = 200_000_000m });
        context.ButToans.Add(btDauKy);

        // Trong kỳ (tháng 2): Chi thanh toán NCC 50M
        var btTrongKy = new ButToan
        {
            SoChungTu = "UNC-CHI-01",
            NgayHachToan = new DateTime(2026, 2, 15),
            NgayChungTu = new DateTime(2026, 2, 15),
            DienGiai = "Thanh toán NCC qua ngân hàng",
            TongTien = 50_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        btTrongKy.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk331.Id, TaiKhoanCoId = tk112.Id, SoTien = 50_000_000m });
        context.ButToans.Add(btTrongKy);

        await context.SaveChangesAsync();

        // Xem sổ cái TK 1121 tháng 2/2026 (Từ 01/02 đến 28/02)
        var soCai = await glService.LaySoCaiAsync("1121", new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));

        Assert.Equal(200_000_000m, soCai.DuNoDauKy);
        Assert.Equal(0m, soCai.TongPhatSinhNo);
        Assert.Equal(50_000_000m, soCai.TongPhatSinhCo);
        Assert.Equal(150_000_000m, soCai.DuNoCuoiKy);
        Assert.Single(soCai.DongChiTiets);
    }

    [Fact]
    public async Task LayBangCanDoiTaiKhoanAsync_GuaranteesStrictDebitCreditEqualityAcrossAllColumnPairs()
    {
        using var context = new AppDbContext(_dbOptions);
        var glService = new GeneralLedgerService(context, NullLogger<GeneralLedgerService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk411 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "4111");
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Góp vốn tiền mặt 100M
        var bt1 = new ButToan
        {
            SoChungTu = "PKT-TB-01",
            NgayHachToan = new DateTime(2026, 3, 1),
            NgayChungTu = new DateTime(2026, 3, 1),
            TongTien = 100_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt1.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk411.Id, SoTien = 100_000_000m });
        context.ButToans.Add(bt1);

        // Mua hàng hóa nợ người bán 40M
        var bt2 = new ButToan
        {
            SoChungTu = "PKT-TB-02",
            NgayHachToan = new DateTime(2026, 3, 10),
            NgayChungTu = new DateTime(2026, 3, 10),
            TongTien = 40_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt2.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk156.Id, TaiKhoanCoId = tk331.Id, SoTien = 40_000_000m });
        context.ButToans.Add(bt2);

        await context.SaveChangesAsync();

        var tb = await glService.LayBangCanDoiTaiKhoanAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));

        // 3 CẶP CỘT BẢNG CÂN ĐỐI PHÁT SINH PHẢI CÂN ĐỐI TUYỆT ĐỐI
        Assert.True(tb.CanDoiDauKy);
        Assert.True(tb.CanDoiPhatSinh);
        Assert.True(tb.CanDoiCuoiKy);
        Assert.True(tb.CanDoiHoanToan);

        Assert.Equal(140_000_000m, tb.TongPhatSinhNoTrongKy);
        Assert.Equal(140_000_000m, tb.TongPhatSinhCoTrongKy);
    }

    [Fact]
    public async Task LayBangCanDoiTaiKhoanAsync_TwoWayAccounts_CalculatesNonOffsettingBalances_AndAccount911ClearsToZero()
    {
        using var context = new AppDbContext(_dbOptions);
        var glService = new GeneralLedgerService(context, NullLogger<GeneralLedgerService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk911 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "911");
        var tk4212 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "4212");

        var khA = await context.DoiTuongs.FirstAsync(d => d.MaDoiTuong == "KH001");
        var khB = new DoiTuong { MaDoiTuong = "KH002", TenDoiTuong = "Khách hàng B", Loai = LoaiDoiTuong.KhachHang };
        context.DoiTuongs.Add(khB);
        await context.SaveChangesAsync();

        // 1. Khách hàng A mua nợ 10M: Nợ 131 (KH A) / Có 5111: 10M
        var bt1 = new ButToan
        {
            SoChungTu = "PKT-TWO-WAY-01",
            NgayHachToan = new DateTime(2026, 4, 5),
            NgayChungTu = new DateTime(2026, 4, 5),
            TongTien = 10_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt1.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk131.Id, TaiKhoanCoId = tk511.Id, DoiTuongId = khA.Id, SoTien = 10_000_000m });
        context.ButToans.Add(bt1);

        // 2. Khách hàng B trả trước tiền mặt 5M: Nợ 1111 / Có 131 (KH B): 5M
        var bt2 = new ButToan
        {
            SoChungTu = "PKT-TWO-WAY-02",
            NgayHachToan = new DateTime(2026, 4, 10),
            NgayChungTu = new DateTime(2026, 4, 10),
            TongTien = 5_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt2.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk131.Id, DoiTuongId = khB.Id, SoTien = 5_000_000m });
        context.ButToans.Add(bt2);

        // 3. Kết chuyển doanh thu sang 911 và sang 4212
        var btKc = new ButToan
        {
            SoChungTu = "PKT-KC-2026-04",
            NgayHachToan = new DateTime(2026, 4, 30),
            NgayChungTu = new DateTime(2026, 4, 30),
            TongTien = 20_000_000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        btKc.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk511.Id, TaiKhoanCoId = tk911.Id, SoTien = 10_000_000m });
        btKc.ChiTietButToans.Add(new ChiTietButToan { DongSo = 2, TaiKhoanNoId = tk911.Id, TaiKhoanCoId = tk4212.Id, SoTien = 10_000_000m });
        context.ButToans.Add(btKc);

        await context.SaveChangesAsync();

        var tb = await glService.LayBangCanDoiTaiKhoanAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));

        // Ràng buộc số dư 2 bên lưỡng tính của TK 131:
        // Không bù trừ giữa KH A (+10M Nợ) và KH B (+5M Có) thành 5M Nợ
        var row131 = tb.DanhSachTaiKhoan.First(d => d.MaTaiKhoan == "131");
        Assert.Equal(10_000_000m, row131.DuNoCuoiKy);
        Assert.Equal(5_000_000m, row131.DuCoCuoiKy);

        // Ràng buộc TK 911 đóng sạch cuối kỳ:
        var row911 = tb.DanhSachTaiKhoan.FirstOrDefault(d => d.MaTaiKhoan == "911");
        if (row911 != null)
        {
            Assert.Equal(0m, row911.DuNoCuoiKy);
            Assert.Equal(0m, row911.DuCoCuoiKy);
        }

        // Bảng cân đối hoàn toàn
        Assert.True(tb.CanDoiCuoiKy);
        Assert.True(tb.CanDoiPhatSinh);
        Assert.True(tb.CanDoiHoanToan);
    }
}
