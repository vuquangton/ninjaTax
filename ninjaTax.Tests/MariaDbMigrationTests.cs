using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using Xunit;

namespace ninjaTax.Tests;

public class MariaDbMigrationTests
{
    private const string MariaDbConnectionString = "Server=localhost;Port=3306;Database=ninjataxdb;User ID=dev;Password=123456;TreatTinyAsBoolean=true";

    [Fact]
    public async Task CanConnectAndInitializeMariaDbSchema()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySQL(MariaDbConnectionString);

        using var context = new AppDbContext(optionsBuilder.Options);

        // Tạo cấu trúc database
        var created = await context.Database.EnsureCreatedAsync();

        // Kiểm tra kết nối
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect);

        // Chạy seed data
        await DbInitializer.SeedDataAsync(context);

        // Kiểm tra các bảng cốt lõi
        var accountCount = await context.TaiKhoans.CountAsync();
        Assert.True(accountCount > 0, "Hệ thống tài khoản TT99 phải được nạp vào MariaDB");

        // Kiểm tra TK 911 có tồn tại theo chuẩn TT99 (Xác định kết quả kinh doanh)
        var has911 = await context.TaiKhoans.AnyAsync(t => t.MaTaiKhoan == "911" || t.MaTaiKhoan.StartsWith("911"));
        Assert.True(has911, "TK 911 phải có trong hệ thống tài khoản theo chuẩn TT99/2025/TT-BTC");

        var warehouseCount = await context.Khos.CountAsync();
        Assert.True(warehouseCount > 0, "Kho hàng mặc định phải được khởi tạo");
    }
}
