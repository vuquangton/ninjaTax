using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class BadDebtProvisionServiceTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public BadDebtProvisionServiceTests()
    {
        _sqliteConnection = new SqliteConnection($"Data Source=mem_baddebt_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
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
    public async Task LayDanhSachNoQuaHan_AppliesCorrectTt48Percentages()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new BadDebtProvisionService(context, NullLogger<BadDebtProvisionService>.Instance);

        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var asOf = new DateTime(2026, 12, 31);

        // 1. Quá hạn 200 ngày (180 - 365) => 30%
        var hd30 = new HoaDonBanHang
        {
            SoChungTu = "HD-30P",
            KhachHangId = kh.Id,
            NgayHoaDon = asOf.AddDays(-230),
            HanThanhToan = asOf.AddDays(-200),
            TongThanhToan = 10000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };

        // 2. Quá hạn 400 ngày (365 - 730) => 50%
        var hd50 = new HoaDonBanHang
        {
            SoChungTu = "HD-50P",
            KhachHangId = kh.Id,
            NgayHoaDon = asOf.AddDays(-430),
            HanThanhToan = asOf.AddDays(-400),
            TongThanhToan = 20000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };

        // 3. Quá hạn 800 ngày (730 - 1095) => 70%
        var hd70 = new HoaDonBanHang
        {
            SoChungTu = "HD-70P",
            KhachHangId = kh.Id,
            NgayHoaDon = asOf.AddDays(-830),
            HanThanhToan = asOf.AddDays(-800),
            TongThanhToan = 30000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };

        // 4. Quá hạn 1200 ngày (>= 1095) => 100%
        var hd100 = new HoaDonBanHang
        {
            SoChungTu = "HD-100P",
            KhachHangId = kh.Id,
            NgayHoaDon = asOf.AddDays(-1230),
            HanThanhToan = asOf.AddDays(-1200),
            TongThanhToan = 40000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };

        // 5. Quá hạn 100 ngày (< 180) => 0% (Không trích lập theo TT48)
        var hd0 = new HoaDonBanHang
        {
            SoChungTu = "HD-0P",
            KhachHangId = kh.Id,
            NgayHoaDon = asOf.AddDays(-130),
            HanThanhToan = asOf.AddDays(-100),
            TongThanhToan = 5000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };

        context.HoaDonBanHangs.AddRange(hd30, hd50, hd70, hd100, hd0);
        await context.SaveChangesAsync();

        var list = await service.LayDanhSachNoQuaHanTt48Async(asOf);

        Assert.Equal(4, list.Count); // hd0 bị bỏ qua vì < 180 ngày

        var r30 = list.First(x => x.SoHoaDon == "HD-30P");
        Assert.Equal(30m, r30.TyLeTrichLap);
        Assert.Equal(3000000m, r30.SoTienDuPhong); // 10tr * 30%

        var r50 = list.First(x => x.SoHoaDon == "HD-50P");
        Assert.Equal(50m, r50.TyLeTrichLap);
        Assert.Equal(10000000m, r50.SoTienDuPhong); // 20tr * 50%

        var r70 = list.First(x => x.SoHoaDon == "HD-70P");
        Assert.Equal(70m, r70.TyLeTrichLap);
        Assert.Equal(21000000m, r70.SoTienDuPhong); // 30tr * 70%

        var r100 = list.First(x => x.SoHoaDon == "HD-100P");
        Assert.Equal(100m, r100.TyLeTrichLap);
        Assert.Equal(40000000m, r100.SoTienDuPhong); // 40tr * 100%
    }

    [Fact]
    public async Task TaoVaGhiSoBangTrichLap_AdditionalProvision_Posts6426To2293()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new BadDebtProvisionService(context, NullLogger<BadDebtProvisionService>.Instance);

        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var asOf = new DateTime(2026, 12, 31);

        // Hóa đơn quá hạn 400 ngày: nợ 20tr => trích 50% = 10tr
        var hd = new HoaDonBanHang
        {
            SoChungTu = "HD-PROV-01",
            KhachHangId = kh.Id,
            NgayHoaDon = asOf.AddDays(-430),
            HanThanhToan = asOf.AddDays(-400),
            TongThanhToan = 20000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };
        context.HoaDonBanHangs.Add(hd);
        await context.SaveChangesAsync();

        var bang = await service.TaoBangTrichLapDuPhongAsync(asOf);

        Assert.Equal(10000000m, bang.TongSoDuPhongPhaiTrich);
        Assert.Equal(0m, bang.SoDuDuPhongHienTai2293);
        Assert.Equal(10000000m, bang.SoTienTrichThem);
        Assert.Equal(0m, bang.SoTienHoanNhap);

        var (ok, msg) = await service.GhiSoBangTrichLapAsync(bang.Id);
        Assert.True(ok, msg);

        var bangReload = await context.BangTrichLapDuPhongNoPhaiThus.FindAsync(bang.Id);
        Assert.NotNull(bangReload);
        Assert.Equal(TrangThaiBangTrichLap.DaGhiSo, bangReload.TrangThai);
        Assert.NotNull(bangReload.ButToanId);

        var butToan = await context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstOrDefaultAsync(b => b.Id == bangReload.ButToanId);

        Assert.NotNull(butToan);
        Assert.Equal(10000000m, butToan.TongTien);

        var tk6426 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "6426");
        var tk2293 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "2293");

        var ct = butToan.ChiTietButToans.First();
        Assert.Equal(tk6426.Id, ct.TaiKhoanNoId);
        Assert.Equal(tk2293.Id, ct.TaiKhoanCoId);
        Assert.Equal(10000000m, ct.SoTien);
    }

    [Fact]
    public async Task TaoVaGhiSoBangTrichLap_Reversal_Posts2293To6426()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new BadDebtProvisionService(context, NullLogger<BadDebtProvisionService>.Instance);

        var asOf = new DateTime(2026, 12, 31);
        var tk6426 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "6426");
        var tk2293 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "2293");

        // Giả lập số dư Có hiện tại của 2293 là 15.000.000 đ
        var btPrevious = new ButToan
        {
            SoChungTu = "PKT-PREV-2293",
            NgayHachToan = asOf.AddDays(-1),
            TongTien = 15000000m,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DongSo = 1,
                    TaiKhoanNoId = tk6426.Id,
                    TaiKhoanCoId = tk2293.Id,
                    SoTien = 15000000m
                }
            }
        };
        context.ButToans.Add(btPrevious);

        // Hóa đơn quá hạn chỉ cần trích 6.000.000 đ (nợ 20tr * 30%)
        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var hd = new HoaDonBanHang
        {
            SoChungTu = "HD-PROV-02",
            KhachHangId = kh.Id,
            NgayHoaDon = asOf.AddDays(-230),
            HanThanhToan = asOf.AddDays(-200),
            TongThanhToan = 20000000m,
            DaThuTien = 0m,
            TrangThai = TrangThaiHddt.CoQuanThueCapMa
        };
        context.HoaDonBanHangs.Add(hd);
        await context.SaveChangesAsync();

        var bang = await service.TaoBangTrichLapDuPhongAsync(asOf);

        Assert.Equal(6000000m, bang.TongSoDuPhongPhaiTrich);
        Assert.Equal(15000000m, bang.SoDuDuPhongHienTai2293);
        Assert.Equal(0m, bang.SoTienTrichThem);
        Assert.Equal(9000000m, bang.SoTienHoanNhap); // Hoàn nhập 15tr - 6tr = 9tr

        var (ok, msg) = await service.GhiSoBangTrichLapAsync(bang.Id);
        Assert.True(ok, msg);

        var bangReload = await context.BangTrichLapDuPhongNoPhaiThus.FindAsync(bang.Id);
        Assert.NotNull(bangReload);
        var butToan = await context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstOrDefaultAsync(b => b.Id == bangReload.ButToanId);

        Assert.NotNull(butToan);
        Assert.Equal(9000000m, butToan.TongTien);

        var ct = butToan.ChiTietButToans.First();
        Assert.Equal(tk2293.Id, ct.TaiKhoanNoId); // Hoàn nhập: Nợ 2293
        Assert.Equal(tk6426.Id, ct.TaiKhoanCoId); // Có 6426
        Assert.Equal(9000000m, ct.SoTien);
    }
}
