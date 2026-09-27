using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Controllers;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class ButToanControllerTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public ButToanControllerTests()
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
    public async Task ListJson_ReturnsPagedResults()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var controller = new ButToanController(butToanService, NullLogger<ButToanController>.Instance);

        // Seed 3 ButToan records
        context.ButToans.AddRange(
            new ButToan { SoChungTu = "BT001", SoChungTuGoc = "GOC1", NgayChungTuGoc = DateTime.Today, DienGiai = "Bút toán 1", NgayHachToan = DateTime.Today, NgayChungTu = DateTime.Today, TongTien = 1000000, TrangThai = TrangThaiButToan.ChuaGhiSo },
            new ButToan { SoChungTu = "BT002", SoChungTuGoc = "GOC2", NgayChungTuGoc = DateTime.Today, DienGiai = "Bút toán 2", NgayHachToan = DateTime.Today.AddDays(-1), NgayChungTu = DateTime.Today.AddDays(-1), TongTien = 2000000, TrangThai = TrangThaiButToan.DaGhiSo },
            new ButToan { SoChungTu = "BT003", SoChungTuGoc = "GOC3", NgayChungTuGoc = DateTime.Today, DienGiai = "Bút toán 3", NgayHachToan = DateTime.Today.AddDays(-2), NgayChungTu = DateTime.Today.AddDays(-2), TongTien = 3000000, TrangThai = TrangThaiButToan.ChuaGhiSo }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.ListJson(page: 1, pageSize: 2);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);

        var jsonObject = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(jsonObject);
        var root = doc.RootElement;

        Assert.Equal(3, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, root.GetProperty("rows").GetArrayLength());
    }
}
