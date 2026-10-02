using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class ArApSettlementTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public ArApSettlementTests()
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
    public async Task DoiTruHoaDonMua_PartialAndFullSettlement_TracksBalancesCorrectly()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new CongNoService(context, NullLogger<CongNoService>.Instance);

        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);

        var hd = new HoaDonMuaHang
        {
            SoChungTu = "MH-AP-01",
            SoHoaDon = "00000001",
            NhaCungCapId = ncc.Id,
            NgayHoaDon = DateTime.Today,
            TongTienHang = 10000000m,
            TongTienThueVat = 1000000m,
            TongThanhToan = 11000000m,
            DaThanhToan = 0m
        };
        context.HoaDonMuaHangs.Add(hd);
        await context.SaveChangesAsync();

        // 1. Đối trừ 5.000.000 đ
        var (ok1, msg1) = await service.DoiTruHoaDonMuaAsync(hd.Id, 5000000m, null, "Thanh toán đợt 1");
        Assert.True(ok1, msg1);

        var hdCheck1 = await context.HoaDonMuaHangs.FindAsync(hd.Id);
        Assert.NotNull(hdCheck1);
        Assert.Equal(5000000m, hdCheck1.DaThanhToan);
        Assert.Equal(6000000m, hdCheck1.ConPhaiTra);

        // 2. Đối trừ nốt 6.000.000 đ
        var (ok2, msg2) = await service.DoiTruHoaDonMuaAsync(hd.Id, 6000000m, null, "Thanh toán đợt 2");
        Assert.True(ok2, msg2);

        var hdCheck2 = await context.HoaDonMuaHangs.FindAsync(hd.Id);
        Assert.NotNull(hdCheck2);
        Assert.Equal(11000000m, hdCheck2.DaThanhToan);
        Assert.Equal(0m, hdCheck2.ConPhaiTra);

        // Kiểm tra lịch sử đối trừ
        var lichSu = await context.DoiTruCongNos.Where(d => d.HoaDonMuaHangId == hd.Id).ToListAsync();
        Assert.Equal(2, lichSu.Count);
    }

    [Fact]
    public async Task DoiTruFifoKhachHang_SettlesOldestInvoicesFirst()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new CongNoService(context, NullLogger<CongNoService>.Instance);

        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);

        // Tạo 3 hóa đơn cách ngày nhau
        var hdOld = new HoaDonBanHang
        {
            SoChungTu = "BH-001",
            KhachHangId = kh.Id,
            NgayHoaDon = DateTime.Today.AddDays(-60),
            HanThanhToan = DateTime.Today.AddDays(-30),
            TongThanhToan = 10000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };
        var hdMid = new HoaDonBanHang
        {
            SoChungTu = "BH-002",
            KhachHangId = kh.Id,
            NgayHoaDon = DateTime.Today.AddDays(-30),
            HanThanhToan = DateTime.Today.AddDays(-15),
            TongThanhToan = 20000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };
        var hdNew = new HoaDonBanHang
        {
            SoChungTu = "BH-003",
            KhachHangId = kh.Id,
            NgayHoaDon = DateTime.Today,
            HanThanhToan = DateTime.Today.AddDays(15),
            TongThanhToan = 30000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };

        context.HoaDonBanHangs.AddRange(hdOld, hdMid, hdNew);
        await context.SaveChangesAsync();

        // Thu 15.000.000 đ => Phải xóa sạch hdOld (10tr) và trả một phần hdMid (5tr)
        var (ok, msg, daDoiTru) = await service.DoiTruFifoKhachHangAsync(kh.Id, 15000000m);

        Assert.True(ok, msg);
        Assert.Equal(15000000m, daDoiTru);

        var hdOldCheck = await context.HoaDonBanHangs.FindAsync(hdOld.Id);
        var hdMidCheck = await context.HoaDonBanHangs.FindAsync(hdMid.Id);
        var hdNewCheck = await context.HoaDonBanHangs.FindAsync(hdNew.Id);

        Assert.NotNull(hdOldCheck);
        Assert.NotNull(hdMidCheck);
        Assert.NotNull(hdNewCheck);

        Assert.Equal(0m, hdOldCheck.ConPhaiThu);     // Đã thanh toán xong
        Assert.Equal(15000000m, hdMidCheck.ConPhaiThu); // Còn nợ 15tr (20tr - 5tr)
        Assert.Equal(30000000m, hdNewCheck.ConPhaiThu); // Chưa đụng đến (nguyên 30tr)
    }

    [Fact]
    public async Task BaoCaoTuoiNo_CorrectlyDistributesBuckets()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new CongNoService(context, NullLogger<CongNoService>.Instance);

        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);

        var now = DateTime.Today;

        // 1. Trong hạn
        var hdTrongHan = new HoaDonBanHang
        {
            SoChungTu = "BH-AGE-01",
            KhachHangId = kh.Id,
            NgayHoaDon = now,
            HanThanhToan = now.AddDays(5),
            TongThanhToan = 5000000m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };
        // 2. Quá hạn 20 ngày (Thuộc 1-30 ngày)
        var hd1Den30 = new HoaDonBanHang
        {
            SoChungTu = "BH-AGE-02",
            KhachHangId = kh.Id,
            NgayHoaDon = now.AddDays(-30),
            HanThanhToan = now.AddDays(-20),
            TongThanhToan = 7000000m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };
        // 3. Quá hạn 100 ngày (Thuộc >90 ngày)
        var hdTren90 = new HoaDonBanHang
        {
            SoChungTu = "BH-AGE-03",
            KhachHangId = kh.Id,
            NgayHoaDon = now.AddDays(-120),
            HanThanhToan = now.AddDays(-100),
            TongThanhToan = 12000000m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };

        context.HoaDonBanHangs.AddRange(hdTrongHan, hd1Den30, hdTren90);
        await context.SaveChangesAsync();

        var aging = await service.BaoCaoTuoiNoPhaiThuAsync(now);

        var item = aging.FirstOrDefault(x => x.DoiTuongId == kh.Id);
        Assert.NotNull(item);
        Assert.Equal(5000000m, item.TrongHan);
        Assert.Equal(7000000m, item.Tu1Den30Ngay);
        Assert.Equal(0m, item.Tu31Den60Ngay);
        Assert.Equal(0m, item.Tu61Den90Ngay);
        Assert.Equal(12000000m, item.Tu91Den180Ngay); // 100 days falls in 91-180 days bucket
        Assert.Equal(24000000m, item.TongNo);
    }

    [Fact]
    public async Task BuTruCongNoHaiChieu_Succeeds_PostsJournalEntry331To131()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new CongNoService(context, NullLogger<CongNoService>.Instance);

        // Tạo đối tượng vừa là khách hàng vừa là nhà cung cấp
        var doiTuong = new DoiTuong
        {
            MaDoiTuong = "DT-DUAL-01",
            TenDoiTuong = "Công ty TNHH Song Hành",
            Loai = LoaiDoiTuong.KhachHang,
            MaSoThue = "0301234567"
        };
        context.DoiTuongs.Add(doiTuong);
        await context.SaveChangesAsync();

        // 1. Tạo HĐ bán hàng (Phải thu 131: 15.000.000)
        var hdBan = new HoaDonBanHang
        {
            SoChungTu = "BH-OFFSET-01",
            KhachHangId = doiTuong.Id,
            NgayHoaDon = DateTime.Today.AddDays(-10),
            HanThanhToan = DateTime.Today,
            TongThanhToan = 15000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };
        // 2. Tạo HĐ mua hàng (Phải trả 331: 20.000.000)
        var hdMua = new HoaDonMuaHang
        {
            SoChungTu = "MH-OFFSET-01",
            NhaCungCapId = doiTuong.Id,
            NgayHoaDon = DateTime.Today.AddDays(-10),
            HanThanhToan = DateTime.Today,
            TongThanhToan = 20000000m,
            DaThanhToan = 0m
        };

        context.HoaDonBanHangs.Add(hdBan);
        context.HoaDonMuaHangs.Add(hdMua);
        await context.SaveChangesAsync();

        // Bù trừ 10.000.000 đ
        var (ok, msg, butToan) = await service.BuTruCongNoHaiChieuAsync(doiTuong.Id, 10000000m, DateTime.Today, "Bù trừ đối tác");

        Assert.True(ok, msg);
        Assert.NotNull(butToan);
        Assert.Equal(10000000m, butToan.TongTien);
        Assert.Single(butToan.ChiTietButToans);

        var ct = butToan.ChiTietButToans.First();
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        Assert.Equal(tk331.Id, ct.TaiKhoanNoId);
        Assert.Equal(tk131.Id, ct.TaiKhoanCoId);
        Assert.Equal(10000000m, ct.SoTien);

        // Kiểm tra số dư công nợ sau bù trừ
        var hdBanReload = await context.HoaDonBanHangs.FindAsync(hdBan.Id);
        var hdMuaReload = await context.HoaDonMuaHangs.FindAsync(hdMua.Id);
        Assert.NotNull(hdBanReload);
        Assert.NotNull(hdMuaReload);
        Assert.Equal(5000000m, hdBanReload.ConPhaiThu); // 15tr - 10tr = 5tr
        Assert.Equal(10000000m, hdMuaReload.ConPhaiTra); // 20tr - 10tr = 10tr
    }
}
