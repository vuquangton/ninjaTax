using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class PersonalIncomeTaxTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public PersonalIncomeTaxTests()
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
    public void ThueTncn_LaoDongThoiVu_Duoi2Trieu_KhongKhauTru()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        var nv = new NhanVien
        {
            LoaiHopDong = LoaiHopDongLaoDong.ThuViecThoiVu,
            CoCamKet08 = false
        };

        // Thu nhập < 2.000.000 VNĐ không phải khấu trừ
        var thue = service.TinhThueTncn(nv, 1_800_000m, 0m, 0m, 0m);
        Assert.Equal(0m, thue);
    }

    [Fact]
    public void ThueTncn_LaoDongThoiVu_Tren2Trieu_KhongCamKet08_KhauTru10PhanTram()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        var nv = new NhanVien
        {
            LoaiHopDong = LoaiHopDongLaoDong.ThuViecThoiVu,
            CoCamKet08 = false
        };

        // Thu nhập 5.000.000 VNĐ -> Khấu trừ 10% = 500.000 VNĐ
        var thue = service.TinhThueTncn(nv, 5_000_000m, 0m, 0m, 0m);
        Assert.Equal(500_000m, thue);
    }

    [Fact]
    public void ThueTncn_LaoDongThoiVu_CoCamKet08VaMst_MienKhauTru()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        var nv = new NhanVien
        {
            LoaiHopDong = LoaiHopDongLaoDong.ThuViecThoiVu,
            CoCamKet08 = true,
            MaSoThue = "8012345678"
        };

        // Có Cam kết 08 và có MST cá nhân hợp lệ -> Tạm thời không khấu trừ 10%
        var thue = service.TinhThueTncn(nv, 8_000_000m, 0m, 0m, 0m);
        Assert.Equal(0m, thue);
    }

    [Theory]
    [InlineData(4_000_000, 200_000)]       // Bậc 1: 4M * 5% = 200.000
    [InlineData(8_000_000, 550_000)]       // Bậc 2: 8M * 10% - 250k = 550.000
    [InlineData(15_000_000, 1_500_000)]    // Bậc 3: 15M * 15% - 750k = 1.500.000
    [InlineData(25_000_000, 3_350_000)]    // Bậc 4: 25M * 20% - 1.65M = 3.350.000
    [InlineData(40_000_000, 6_750_000)]    // Bậc 5: 40M * 25% - 3.25M = 6.750.000
    [InlineData(60_000_000, 12_150_000)]   // Bậc 6: 60M * 30% - 5.85M = 12.150.000
    [InlineData(100_000_000, 25_150_000)]  // Bậc 7: 100M * 35% - 9.85M = 25.150.000
    public void ThueTncn_BieuThueLuyTien7Bac_ChuanXac(decimal thuNhapTinhThue, decimal thueKyVong)
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        var nv = new NhanVien
        {
            LoaiHopDong = LoaiHopDongLaoDong.HopDongDaiHan,
            SoNguoiPhuThuoc = 0
        };

        // Giảm trừ bản thân: 11M, BH: 0
        // Tổng thu nhập = 11M + thuNhapTinhThue
        var tongThuNhap = 11_000_000m + thuNhapTinhThue;

        var thue = service.TinhThueTncn(nv, tongThuNhap, 0m, 0m, 0m);
        Assert.Equal(thueKyVong, thue);
    }

    [Fact]
    public void ThueTncn_CoNguoiPhuThuocVaBaoHiemVaOtMienThue_GiamTruDungQuyDinh()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = CreatePayrollService(context);

        var nv = new NhanVien
        {
            LoaiHopDong = LoaiHopDongLaoDong.HopDongDaiHan,
            SoNguoiPhuThuoc = 2 // 2 * 4.4M = 8.8M
        };

        // Tổng thu nhập: 40.000.000 VNĐ
        // Trong đó:
        // - OT miễn thuế: 2.000.000 VNĐ
        // - Phụ cấp ăn trưa miễn thuế: 730.000 VNĐ
        // - Bảo hiểm NLĐ đã đóng: 2.100.000 VNĐ (10.5% của 20M)
        // - Giảm trừ bản thân: 11.000.000 VNĐ
        // - Giảm trừ NPT (2 người): 8.800.000 VNĐ
        // => TNCT = 40M - 2M - 730k = 37.270.000 VNĐ
        // => TNTT = 37.270.000 - 11M - 8.8M - 2.1M = 15.370.000 VNĐ (Rơi vào Bậc 3: 10M - 18M)
        // => Thuế TNCN = 15.370.000 * 15% - 750.000 = 2.305.500 - 750.000 = 1.555.500 VNĐ
        var tongThuNhap = 40_000_000m;
        var otMienThue = 2_000_000m;
        var phuCapMienThue = 730_000m;
        var baoHiemNld = 2_100_000m;

        var thue = service.TinhThueTncn(nv, tongThuNhap, baoHiemNld, otMienThue, phuCapMienThue);
        Assert.Equal(1_555_500m, thue);
    }
}
