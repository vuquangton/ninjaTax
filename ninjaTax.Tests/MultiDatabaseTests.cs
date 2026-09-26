using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

/// <summary>
/// Bộ kiểm thử xác thực kiến trúc Multi-Database và các quy tắc kế toán TT99:
/// - AC1: Chuyển đổi và tự động fallback Provider
/// - AC2: Đảm bảo độ chính xác tuyệt đối của kiểu số thực tiền tệ (Decimal 19, 4), không bị sai số trôi nổi (floating drift)
/// - AC3: Kiểm tra định danh bigint (long) tự tăng cho khóa chính và khóa ngoại
/// - AC5: Kiểm tra xác thực chuỗi kết nối và báo lỗi rõ ràng
/// - TT99: Ràng buộc cân đối kép TongNo == TongCo, bắt buộc chứng từ gốc, cấm TK 911
/// </summary>
public class MultiDatabaseTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public MultiDatabaseTests()
    {
        // Mở kết nối Sqlite in-memory cho các ca kiểm thử
        _sqliteConnection = new SqliteConnection("Data Source=:memory:");
        _sqliteConnection.Open();

        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        using var context = new AppDbContext(_dbOptions);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _sqliteConnection.Dispose();
    }

    [Fact]
    public async Task AC2_DecimalPrecision_StoreAndRetrieve_ExactMatchWithoutDrift()
    {
        // Giả lập giá trị tiền lẻ 4 chữ số thập phân nhạy cảm với sai số trôi nổi
        const decimal testAmount = 1000000.005m;

        using (var context = new AppDbContext(_dbOptions))
        {
            var tkNo = new TaiKhoan
            {
                MaTaiKhoan = "1111",
                TenTaiKhoan = "Tiền Việt Nam",
                BacTaiKhoan = 2,
                LoaiTaiKhoan = LoaiTaiKhoan.TaiSan,
                TinhChat = TinhChatTaiKhoan.DuNo,
                DangHoatDong = true
            };
            var tkCo = new TaiKhoan
            {
                MaTaiKhoan = "1121",
                TenTaiKhoan = "Tiền gửi ngân hàng",
                BacTaiKhoan = 2,
                LoaiTaiKhoan = LoaiTaiKhoan.TaiSan,
                TinhChat = TinhChatTaiKhoan.DuNo,
                DangHoatDong = true
            };
            context.TaiKhoans.AddRange(tkNo, tkCo);
            await context.SaveChangesAsync();

            var butToan = new ButToan
            {
                SoChungTu = "PKT-TEST-001",
                NgayHachToan = DateTime.Today,
                NgayChungTu = DateTime.Today,
                SoChungTuGoc = "UNC-12345",
                NgayChungTuGoc = DateTime.Today,
                DienGiai = "Kiểm tra độ chính xác tiền tệ 19, 4",
                TongTien = testAmount,
                TongNo = testAmount,
                TongCo = testAmount,
                ChiTietButToans = new List<ChiTietButToan>
                {
                    new()
                    {
                        DongSo = 1,
                        TaiKhoanNoId = tkNo.Id,
                        TaiKhoanCoId = tkCo.Id,
                        SoTien = testAmount,
                        DienGiai = "Dòng kiểm tra decimal"
                    }
                }
            };

            context.ButToans.Add(butToan);
            await context.SaveChangesAsync();
        }

        // Đọc lại từ CSDL trong context mới và xác nhận giá trị chính xác tuyệt đối
        using (var newContext = new AppDbContext(_dbOptions))
        {
            var retrieved = await newContext.ButToans
                .Include(b => b.ChiTietButToans)
                .FirstOrDefaultAsync(b => b.SoChungTu == "PKT-TEST-001");

            Assert.NotNull(retrieved);
            Assert.Equal(testAmount, retrieved.TongTien);
            Assert.Equal(testAmount, retrieved.TongNo);
            Assert.Equal(testAmount, retrieved.TongCo);

            var line = retrieved.ChiTietButToans.First();
            Assert.Equal(testAmount, line.SoTien);

            // Kiểm tra số học không có độ trễ/sai số lũy kế
            decimal multiplied = line.SoTien * 3m;
            Assert.Equal(3000000.015m, multiplied);
        }
    }

    [Fact]
    public async Task AC3_BigIntIdStrategy_GeneratesLongIdsSequentially()
    {
        using var context = new AppDbContext(_dbOptions);

        var doiTuong1 = new DoiTuong
        {
            MaDoiTuong = "DT001",
            TenDoiTuong = "Đối tượng thử nghiệm 1",
            Loai = LoaiDoiTuong.KhachHang
        };
        var doiTuong2 = new DoiTuong
        {
            MaDoiTuong = "DT002",
            TenDoiTuong = "Đối tượng thử nghiệm 2",
            Loai = LoaiDoiTuong.NhaCungCap
        };

        context.DoiTuongs.AddRange(doiTuong1, doiTuong2);
        await context.SaveChangesAsync();

        Assert.True(doiTuong1.Id > 0, "ID phải là số nguyên dương kiểu long");
        Assert.True(doiTuong2.Id > doiTuong1.Id, "ID tự tăng theo thứ tự");
        Assert.IsType<long>(doiTuong1.Id);
    }

    [Fact]
    public void AC1_ProviderSwitching_FallbackToSqliteWhenMissingOrInvalid()
    {
        var services = new ServiceCollection();
        var inMemoryConfig = new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "OracleUnknown",
            ["ConnectionStrings:Sqlite"] = "Data Source=:memory:"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var logger = NullLogger.Instance;

        var selectedProvider = services.AddMultiDatabaseContext(configuration, logger);

        Assert.Equal("Sqlite", selectedProvider);
    }

    [Fact]
    public void AC5_ConnectionStrings_ThrowsWhenSpecifiedProviderHasNoConnectionString()
    {
        var services = new ServiceCollection();
        var inMemoryConfig = new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "SqlServer",
            // Cố tình bỏ qua connection string SqlServer
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var logger = NullLogger.Instance;

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddMultiDatabaseContext(configuration, logger);
        });

        Assert.Contains("SqlServer", ex.Message);
    }

    [Fact]
    public async Task TT99_DoubleEntry_EnforcesTongNoEqualsTongCo()
    {
        using var context = new AppDbContext(_dbOptions);
        var logger = NullLogger<ButToanService>.Instance;
        var service = new ButToanService(context, logger);

        var tkNo = new TaiKhoan { MaTaiKhoan = "1111", TenTaiKhoan = "Tiền mặt", DangHoatDong = true };
        var tkCo = new TaiKhoan { MaTaiKhoan = "5111", TenTaiKhoan = "Doanh thu", DangHoatDong = true };
        context.TaiKhoans.AddRange(tkNo, tkCo);
        await context.SaveChangesAsync();

        var butToanKhongCan = new ButToan
        {
            SoChungTu = "PKT-LE-001",
            SoChungTuGoc = "HD01",
            NgayChungTuGoc = DateTime.Today,
            DienGiai = "Kiểm tra bất đối xứng",
            TongNo = 500000m,
            TongCo = 400000m, // Không cân
            ChiTietButToans = new List<ChiTietButToan>()
        };

        var (hopLe, loi) = service.KiemTraHopLe(butToanKhongCan);

        Assert.False(hopLe);
        Assert.NotNull(loi);
    }

    [Fact]
    public async Task TT99_StrictRules_RejectsAccount911()
    {
        using var context = new AppDbContext(_dbOptions);
        var logger = NullLogger<ButToanService>.Instance;
        var service = new ButToanService(context, logger);

        var tk911 = new TaiKhoan { MaTaiKhoan = "911", TenTaiKhoan = "Xác định KQKD", DangHoatDong = true };
        var tk421 = new TaiKhoan { MaTaiKhoan = "421", TenTaiKhoan = "Lợi nhuận chưa phân phối", DangHoatDong = true };
        context.TaiKhoans.AddRange(tk911, tk421);
        await context.SaveChangesAsync();

        var butToanViPham = new ButToan
        {
            SoChungTu = "PKT-911",
            SoChungTuGoc = "CHUNG_TU_01",
            NgayChungTuGoc = DateTime.Today,
            DienGiai = "Cố tình hạch toán 911",
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DongSo = 1,
                    TaiKhoanNoId = tk911.Id,
                    TaiKhoanCoId = tk421.Id,
                    SoTien = 1000000m
                }
            }
        };

        var (thanhCong, thongBao, _) = await service.TaoMoiAsync(butToanViPham);

        Assert.False(thanhCong);
        Assert.Contains("911", thongBao);
    }

    [Fact]
    public async Task Check_MariaDb_Connection()
    {
        var connStr = "Server=localhost;Port=3306;User ID=dev;Password=123456;";
        await using var connection = new MySqlConnector.MySqlConnection(connStr);
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT VERSION();";
        var version = await cmd.ExecuteScalarAsync();

        cmd.CommandText = "CREATE DATABASE IF NOT EXISTS ninjataxdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        await cmd.ExecuteNonQueryAsync();

        cmd.CommandText = "SHOW DATABASES;";
        await using var reader = await cmd.ExecuteReaderAsync();
        var dbs = new List<string>();
        while (await reader.ReadAsync())
        {
            dbs.Add(reader.GetString(0));
        }

        Assert.NotNull(version);
        Assert.Contains("ninjataxdb", dbs);
    }
}
