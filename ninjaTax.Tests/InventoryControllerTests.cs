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

public class InventoryControllerTests : IDisposable
{
    private class DummyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public InventoryControllerTests()
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

    private InventoryController CreateController(AppDbContext context)
    {
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var inventoryService = new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);
        var controller = new InventoryController(inventoryService, context, NullLogger<InventoryController>.Instance)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new DummyTempDataProvider())
        };
        return controller;
    }

    [Fact]
    public async Task Index_ReturnsViewWithVouchers()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InventoryIndexViewModel>(viewResult.Model);
        Assert.NotNull(model.BranchList);
        Assert.NotNull(model.WarehouseList);
    }

    [Fact]
    public async Task CreateInward_ValidVoucher_RedirectsToIndex()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        var vm = new PhieuNhapKhoEditViewModel
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-CTRL-01",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LoaiNhapKho = LoaiNhapKho.MuaNgoai,
            GhiSoNgay = true,
            ChiTiets = new List<ChiTietNhapKhoEditViewModel>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGia = 1_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };

        var result = await controller.CreateInward(vm);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InventoryController.Index), redirectResult.ActionName);

        var created = await context.PhieuNhapKhos.FirstOrDefaultAsync(p => p.SoPhieu == "PNK-CTRL-01");
        Assert.NotNull(created);
        Assert.Equal(TrangThaiPhieuKho.DaGhiSo, created.TrangThai);
    }

    [Fact]
    public async Task CreateOutward_InsufficientStock_ReturnsViewWithModelError()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");

        // Yêu cầu xuất 9999 cái (trong khi kho đang trống)
        var vm = new PhieuXuatKhoEditViewModel
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PXK-CTRL-ERR",
            NgayXuat = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LoaiXuatKho = LoaiXuatKho.BanHang,
            GhiSoNgay = true,
            ChiTiets = new List<ChiTietXuatKhoEditViewModel>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 9999m,
                    DonGiaVon = 1_000_000m,
                    TaiKhoanNoId = tk632.Id,
                    TaiKhoanCoId = tk156.Id
                }
            }
        };

        var result = await controller.CreateOutward(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task GetStockBalanceJson_ReturnsExpectedBalance()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);

        var result = await controller.GetStockBalanceJson(kho.Id, vatTu.Id);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }
}
