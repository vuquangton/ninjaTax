using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using ninjaTax.Controllers;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class CompanyControllerTests : IDisposable
{
    private class DummyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public CompanyControllerTests()
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

    private CompanyController CreateController(AppDbContext context)
    {
        var companyService = new CompanyService(context, NullLogger<CompanyService>.Instance);
        var controller = new CompanyController(companyService, NullLogger<CompanyController>.Instance)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new DummyTempDataProvider())
        };
        return controller;
    }

    [Fact]
    public async Task Index_ReturnsViewWithDashboardModel()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CompanyDashboardViewModel>(viewResult.Model);
        Assert.NotNull(model.DoanhNghiep);
        Assert.Equal("0109998883", model.DoanhNghiep.MaSoThue);
        Assert.NotNull(model.CauHinhKeToan);
        Assert.NotEmpty(model.ChiNhanhs);
    }

    [Fact]
    public async Task Edit_Get_ReturnsCompanyEditViewModel()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var result = await controller.Edit();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CompanyEditViewModel>(viewResult.Model);
        Assert.Equal("0109998883", model.MaSoThue);
    }

    [Fact]
    public async Task Edit_Post_ValidTaxId_UpdatesSuccessfully()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var editVm = new CompanyEditViewModel
        {
            MaDoanhNghiep = "DN01",
            TenDoanhNghiep = "CÔNG TY TNHH MỚI CẬP NHẬT",
            MaSoThue = "0100109106", // MST Vinatex hợp lệ Modulo 11
            DiaChiTruSo = "Số 1 Tràng Tiền, Hoàn Kiếm, Hà Nội",
            VonDieuLe = 50_000_000_000m,
            NgayThanhLap = new DateTime(2015, 5, 20)
        };

        var result = await controller.Edit(editVm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var company = await context.ThongTinDoanhNghieps.FirstAsync();
        Assert.Equal("0100109106", company.MaSoThue);
        Assert.Equal("CÔNG TY TNHH MỚI CẬP NHẬT", company.TenDoanhNghiep);
    }

    [Fact]
    public async Task Edit_Post_InvalidTaxId_FailsValidation()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var editVm = new CompanyEditViewModel
        {
            MaDoanhNghiep = "DN01",
            TenDoanhNghiep = "CÔNG TY BỊ SAI MST",
            MaSoThue = "0109998889", // MST sai Modulo 11
            DiaChiTruSo = "Số 1 Tràng Tiền, Hoàn Kiếm, Hà Nội",
            VonDieuLe = 10_000_000_000m,
            NgayThanhLap = DateTime.Today
        };

        var result = await controller.Edit(editVm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task AccountingConfig_GetAndPost_UpdatesConfiguration()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var getResult = await controller.AccountingConfig();
        var viewResult = Assert.IsType<ViewResult>(getResult);
        var vm = Assert.IsType<AccountingConfigEditViewModel>(viewResult.Model);

        vm.DonViTienTe = "VND";
        vm.ThangBatDauNienDo = 4; // Niên độ từ 01/04
        vm.CanhBaoChiVuotQuy = false;

        var postResult = await controller.AccountingConfig(vm);
        var redirect = Assert.IsType<RedirectToActionResult>(postResult);
        Assert.Equal("AccountingConfig", redirect.ActionName);

        var cfg = await context.CauHinhKeToans.FirstAsync();
        Assert.Equal(4, cfg.ThangBatDauNienDo);
        Assert.False(cfg.CanhBaoChiVuotQuy);
    }

    [Fact]
    public async Task LockBook_And_UnlockBook_TogglesLockDateSuccessfully()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var lockDate = new DateTime(2025, 12, 31);
        var lockResult = await controller.LockBook(lockDate);
        var lockRedirect = Assert.IsType<RedirectToActionResult>(lockResult);
        Assert.Equal("AccountingConfig", lockRedirect.ActionName);

        var cfgLocked = await context.CauHinhKeToans.FirstAsync();
        Assert.Equal(lockDate, cfgLocked.NgayKhoaSo);

        // Mở khóa sổ
        var unlockResult = await controller.UnlockBook();
        var unlockRedirect = Assert.IsType<RedirectToActionResult>(unlockResult);
        Assert.Equal("AccountingConfig", unlockRedirect.ActionName);

        var cfgUnlocked = await context.CauHinhKeToans.FirstAsync();
        Assert.Null(cfgUnlocked.NgayKhoaSo);
    }

    [Fact]
    public async Task CreateBranch_ValidBranch_Succeeds()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var branchVm = new BranchEditViewModel
        {
            MaChiNhanh = "CN-HCM-01",
            TenChiNhanh = "Chi nhánh TP. Hồ Chí Minh",
            MaSoThueChiNhanh = "0109998883-001", // 13 số hợp lệ
            LoaiChiNhanh = LoaiChiNhanh.PhuThuocKhacTinh,
            KeKhaiThueGtgtRieng = true,
            KeKhaiThueTncnRieng = true,
            DiaChi = "123 Nguyễn Huệ, Quận 1, TP. HCM",
            TinhThanhPho = "TP. Hồ Chí Minh",
            DangHoatDong = true
        };

        var result = await controller.CreateBranch(branchVm);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Branches", redirect.ActionName);

        var branch = await context.ChiNhanhs.FirstOrDefaultAsync(b => b.MaChiNhanh == "CN-HCM-01");
        Assert.NotNull(branch);
        Assert.Equal("0109998883-001", branch.MaSoThueChiNhanh);
        Assert.True(branch.KeKhaiThueGtgtRieng);
    }

    [Fact]
    public async Task CreateBranch_Invalid13DigitTaxCode_FailsValidation()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var branchVm = new BranchEditViewModel
        {
            MaChiNhanh = "CN-SAI-MST",
            TenChiNhanh = "Chi nhánh Sai MST",
            MaSoThueChiNhanh = "0109998883-12", // Chỉ có 2 số đuôi (phải là 3 số)
            LoaiChiNhanh = LoaiChiNhanh.PhuThuocCungTinh,
            DangHoatDong = true
        };

        var result = await controller.CreateBranch(branchVm);
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task CreateBranch_DuplicateHeadOffice_FailsValidation()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var branchVm = new BranchEditViewModel
        {
            MaChiNhanh = "HO-02",
            TenChiNhanh = "Trụ sở chính thứ 2",
            LoaiChiNhanh = LoaiChiNhanh.TruSoChinh, // Đã tồn tại HO-01 là Trụ sở chính
            DangHoatDong = true
        };

        var result = await controller.CreateBranch(branchVm);
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteBranch_HeadOffice_ReturnsErrorMessageInTempData()
    {
        using var context = new AppDbContext(_dbOptions);
        var controller = CreateController(context);

        var hoBranch = await context.ChiNhanhs.FirstAsync(b => b.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh);

        var result = await controller.DeleteBranch(hoBranch.Id);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Branches", redirect.ActionName);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task DeleteBranch_DependentBranch_Succeeds()
    {
        using var context = new AppDbContext(_dbOptions);
        var company = await context.ThongTinDoanhNghieps.FirstAsync();

        var subBranch = new ChiNhanh
        {
            DoanhNghiepId = company.Id,
            MaChiNhanh = "CN-CAN-THO",
            TenChiNhanh = "Chi nhánh Cần Thơ",
            LoaiChiNhanh = LoaiChiNhanh.PhuThuocCungTinh,
            DangHoatDong = true
        };
        await context.ChiNhanhs.AddAsync(subBranch);
        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var result = await controller.DeleteBranch(subBranch.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Branches", redirect.ActionName);
        Assert.Null(await context.ChiNhanhs.FindAsync(subBranch.Id));
    }
}

