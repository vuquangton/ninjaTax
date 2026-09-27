using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Controllers;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class DepartmentControllerTests : IDisposable
{
    private class DummyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public DepartmentControllerTests()
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

    private DepartmentController CreateController(AppDbContext context)
    {
        var deptService = new DepartmentService(context, NullLogger<DepartmentService>.Instance);
        var controller = new DepartmentController(deptService, context, NullLogger<DepartmentController>.Instance)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new DummyTempDataProvider())
        };
        return controller;
    }

    [Fact]
    public async Task Index_ReturnsViewWithFlattenedHierarchy()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DepartmentIndexViewModel>(viewResult.Model);
        Assert.NotNull(model.Departments);
        Assert.True(model.Departments.Count >= 5); // Default seeded departments
    }

    [Fact]
    public async Task Create_ValidDepartment_RedirectsToIndex()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();

        var vm = new DepartmentEditViewModel
        {
            ChiNhanhId = chiNhanh.Id,
            MaPhongBan = "PB-MARKETING",
            TenPhongBan = "Phòng Marketing",
            LoaiPhongBan = LoaiPhongBan.BanHang,
            MaTaiKhoanChiPhi = "6421",
            DangHoatDong = true
        };

        var result = await controller.Create(vm);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DepartmentController.Index), redirectResult.ActionName);

        var created = await context.PhongBans.FirstOrDefaultAsync(p => p.MaPhongBan == "PB-MARKETING");
        Assert.NotNull(created);
        Assert.Equal("Phòng Marketing", created.TenPhongBan);
        Assert.Equal("6421", created.MaTaiKhoanChiPhi);
    }

    [Fact]
    public async Task Create_DuplicateCode_ReturnsViewWithModelError()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();

        var vm = new DepartmentEditViewModel
        {
            ChiNhanhId = chiNhanh.Id,
            MaPhongBan = "BOD", // Already seeded
            TenPhongBan = "Trùng Ban Giám Đốc",
            LoaiPhongBan = LoaiPhongBan.QuanLy
        };

        var result = await controller.Create(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Delete_DepartmentWithChildren_ThrowsOrShowsErrorMessage()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        // BOD has child PB-KTTC in seeds
        var bod = await context.PhongBans.FirstOrDefaultAsync(p => p.MaPhongBan == "BOD");
        Assert.NotNull(bod);

        var result = await controller.Delete(bod.Id);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DepartmentController.Index), redirectResult.ActionName);
        Assert.NotNull(controller.TempData["ErrorMessage"]);

        // BOD should still exist
        var exists = await context.PhongBans.AnyAsync(p => p.Id == bod.Id);
        Assert.True(exists);
    }

    [Fact]
    public async Task GetTreeJson_ReturnsJsonWithRootDepartments()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var result = await controller.GetTreeJson();

        var jsonResult = Assert.IsType<JsonResult>(result);
        var roots = Assert.IsAssignableFrom<List<PhongBan>>(jsonResult.Value);
        Assert.NotEmpty(roots);
    }
}

