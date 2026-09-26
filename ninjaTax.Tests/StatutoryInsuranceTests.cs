using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class StatutoryInsuranceTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public StatutoryInsuranceTests()
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

    private PayrollService CreatePayrollService(AppDbContext context)
    {
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var timesheetService = new TimesheetService(context, NullLogger<TimesheetService>.Instance);
        return new PayrollService(context, butToanService, timesheetService, NullLogger<PayrollService>.Instance);
    }

    [Fact]
    public void LuongDongBaoHiem_DuoiMucToiThieuVung_NémNgoaiLe()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        // Vùng 1 tối thiểu là 4.960.000 VNĐ theo NĐ 74/2024
        // Mức khai báo 4.000.000 VNĐ -> bắt buộc phải ném ngoại lệ
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            service.TinhBaoHiemNguoiLaoDong(4_000_000m, 1);
        });

        Assert.Contains("thấp hơn mức lương tối thiểu vùng", ex.Message);
    }

    [Fact]
    public void TinhBaoHiem_MucLuongBinhThuong30Trieu_TinhDungTyLe()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        // Mức lương 30.000.000 VNĐ (dưới trần 46.8M và trần 99.2M)
        var luong = 30_000_000m;

        // DN gánh: 17.5% BHXH, 3% BHYT, 1% BHTN, 2% KPCĐ
        var (bhxhDn, bhytDn, bhtnDn, kpcdDn) = service.TinhBaoHiemDoanhNghiep(luong, 1);
        Assert.Equal(5_250_000m, bhxhDn); // 30M * 17.5%
        Assert.Equal(900_000m, bhytDn);    // 30M * 3%
        Assert.Equal(300_000m, bhtnDn);    // 30M * 1%
        Assert.Equal(600_000m, kpcdDn);    // 30M * 2%

        // NLĐ gánh: 8% BHXH, 1.5% BHYT, 1% BHTN
        var (bhxhNld, bhytNld, bhtnNld) = service.TinhBaoHiemNguoiLaoDong(luong, 1);
        Assert.Equal(2_400_000m, bhxhNld); // 30M * 8%
        Assert.Equal(450_000m, bhytNld);   // 30M * 1.5%
        Assert.Equal(300_000m, bhtnNld);   // 30M * 1%
    }

    [Fact]
    public void TinhBaoHiem_VuotTran46_8M_ChanTranBhxhVaBhyt()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        // Lương 60.000.000 VNĐ > Trần 46.800.000 VNĐ (20 * 2.34M theo NĐ 73/2024)
        // BHXH & BHYT chỉ được tính trên 46.800.000 VNĐ
        // BHTN vẫn được tính trên 60.000.000 VNĐ vì trần BHTN là 99.200.000 VNĐ
        var luong = 60_000_000m;

        var (bhxhDn, bhytDn, bhtnDn, kpcdDn) = service.TinhBaoHiemDoanhNghiep(luong, 1);
        Assert.Equal(Math.Round(46_800_000m * 0.175m, 0), bhxhDn); // 8.190.000 VNĐ
        Assert.Equal(Math.Round(46_800_000m * 0.030m, 0), bhytDn); // 1.404.000 VNĐ
        Assert.Equal(Math.Round(60_000_000m * 0.010m, 0), bhtnDn); // 600.000 VNĐ (tính trên 60M)
        Assert.Equal(Math.Round(46_800_000m * 0.020m, 0), kpcdDn); // 936.000 VNĐ

        var (bhxhNld, bhytNld, bhtnNld) = service.TinhBaoHiemNguoiLaoDong(luong, 1);
        Assert.Equal(Math.Round(46_800_000m * 0.080m, 0), bhxhNld); // 3.744.000 VNĐ
        Assert.Equal(Math.Round(46_800_000m * 0.015m, 0), bhytNld); // 702.000 VNĐ
        Assert.Equal(Math.Round(60_000_000m * 0.010m, 0), bhtnNld); // 600.000 VNĐ
    }

    [Fact]
    public void TinhBaoHiem_VuotTranBhtn99_2M_ChanTranBhtn()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        // Lương 120.000.000 VNĐ > Trần BHTN 99.200.000 VNĐ (20 * 4.960.000)
        var luong = 120_000_000m;

        var (bhxhDn, bhytDn, bhtnDn, kpcdDn) = service.TinhBaoHiemDoanhNghiep(luong, 1);
        Assert.Equal(Math.Round(46_800_000m * 0.175m, 0), bhxhDn);
        Assert.Equal(Math.Round(99_200_000m * 0.010m, 0), bhtnDn); // Chặn tại trần 99.2M: 992.000 VNĐ

        var (bhxhNld, bhytNld, bhtnNld) = service.TinhBaoHiemNguoiLaoDong(luong, 1);
        Assert.Equal(Math.Round(99_200_000m * 0.010m, 0), bhtnNld); // Chặn tại trần 99.2M: 992.000 VNĐ
    }
}
