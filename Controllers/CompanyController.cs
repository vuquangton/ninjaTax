using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    // GET: /Company
    public async Task<IActionResult> Index()
    {
        var company = await _companyService.LayThongTinDoanhNghiepAsync();
        var config = await _companyService.LayCauHinhKeToanAsync();
        var branches = await _companyService.LayDanhSachChiNhanhAsync();

        var vm = new CompanyDashboardViewModel
        {
            DoanhNghiep = company,
            CauHinhKeToan = config,
            ChiNhanhs = branches
        };

        return View(vm);
    }

    // GET: /Company/Edit
    public async Task<IActionResult> Edit()
    {
        var c = await _companyService.LayThongTinDoanhNghiepAsync();
        var vm = new CompanyEditViewModel
        {
            Id = c.Id,
            MaDoanhNghiep = c.MaDoanhNghiep,
            TenDoanhNghiep = c.TenDoanhNghiep,
            TenGiaoDich = c.TenGiaoDich,
            TenTiengAnh = c.TenTiengAnh,
            MaSoThue = c.MaSoThue,
            DiaChiTruSo = c.DiaChiTruSo,
            TinhThanhPho = c.TinhThanhPho,
            QuanHuyen = c.QuanHuyen,
            MaCoQuanThueQuanLy = c.MaCoQuanThueQuanLy,
            TenCoQuanThueQuanLy = c.TenCoQuanThueQuanLy,
            NguoiDaiDienPhapLuat = c.NguoiDaiDienPhapLuat,
            ChucDanhNguoiDaiDien = c.ChucDanhNguoiDaiDien,
            GiamDoc = c.GiamDoc,
            KeToanTruong = c.KeToanTruong,
            NguoiLapBieu = c.NguoiLapBieu,
            ThuQuy = c.ThuQuy,
            SoDienThoai = c.SoDienThoai,
            Email = c.Email,
            Website = c.Website,
            VonDieuLe = c.VonDieuLe,
            NgayThanhLap = c.NgayThanhLap
        };

        return View(vm);
    }

    // POST: /Company/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var entity = new ThongTinDoanhNghiep
            {
                Id = vm.Id,
                MaDoanhNghiep = vm.MaDoanhNghiep,
                TenDoanhNghiep = vm.TenDoanhNghiep,
                TenGiaoDich = vm.TenGiaoDich,
                TenTiengAnh = vm.TenTiengAnh,
                MaSoThue = vm.MaSoThue.Trim(),
                DiaChiTruSo = vm.DiaChiTruSo,
                TinhThanhPho = vm.TinhThanhPho,
                QuanHuyen = vm.QuanHuyen,
                MaCoQuanThueQuanLy = vm.MaCoQuanThueQuanLy,
                TenCoQuanThueQuanLy = vm.TenCoQuanThueQuanLy,
                NguoiDaiDienPhapLuat = vm.NguoiDaiDienPhapLuat,
                ChucDanhNguoiDaiDien = vm.ChucDanhNguoiDaiDien,
                GiamDoc = vm.GiamDoc,
                KeToanTruong = vm.KeToanTruong,
                NguoiLapBieu = vm.NguoiLapBieu,
                ThuQuy = vm.ThuQuy,
                SoDienThoai = vm.SoDienThoai,
                Email = vm.Email,
                Website = vm.Website,
                VonDieuLe = vm.VonDieuLe,
                NgayThanhLap = vm.NgayThanhLap
            };

            await _companyService.CapNhatThongTinDoanhNghiepAsync(entity);
            TempData["SuccessMessage"] = "Cập nhật hồ sơ doanh nghiệp thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ doanh nghiệp");
            ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
            return View(vm);
        }
    }

    // GET: /Company/AccountingConfig
    public async Task<IActionResult> AccountingConfig()
    {
        var config = await _companyService.LayCauHinhKeToanAsync();
        var vm = new AccountingConfigEditViewModel
        {
            DoanhNghiepId = config.DoanhNghiepId,
            CheDoKeToan = config.CheDoKeToan,
            DonViTienTe = config.DonViTienTe,
            NgayBatDauNienDo = config.NgayBatDauNienDo,
            ThangBatDauNienDo = config.ThangBatDauNienDo,
            PhuongPhapThueGtgt = config.PhuongPhapThueGtgt,
            PhuongPhapXuatKho = config.PhuongPhapXuatKho,
            PhuongPhapKhauHaoTscd = config.PhuongPhapKhauHaoTscd,
            NgayKhoaSo = config.NgayKhoaSo,
            CanhBaoChiVuotQuy = config.CanhBaoChiVuotQuy,
            CanhBaoXuatAmKho = config.CanhBaoXuatAmKho,
            CanhBaoHoaDonTren20TrTienMat = config.CanhBaoHoaDonTren20TrTienMat
        };

        return View(vm);
    }

    // POST: /Company/AccountingConfig
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AccountingConfig(AccountingConfigEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var config = new CauHinhKeToan
            {
                DoanhNghiepId = vm.DoanhNghiepId,
                CheDoKeToan = vm.CheDoKeToan,
                DonViTienTe = vm.DonViTienTe,
                NgayBatDauNienDo = vm.NgayBatDauNienDo,
                ThangBatDauNienDo = vm.ThangBatDauNienDo,
                PhuongPhapThueGtgt = vm.PhuongPhapThueGtgt,
                PhuongPhapXuatKho = vm.PhuongPhapXuatKho,
                PhuongPhapKhauHaoTscd = vm.PhuongPhapKhauHaoTscd,
                NgayKhoaSo = vm.NgayKhoaSo,
                CanhBaoChiVuotQuy = vm.CanhBaoChiVuotQuy,
                CanhBaoXuatAmKho = vm.CanhBaoXuatAmKho,
                CanhBaoHoaDonTren20TrTienMat = vm.CanhBaoHoaDonTren20TrTienMat
            };

            await _companyService.CapNhatCauHinhKeToanAsync(config);
            TempData["SuccessMessage"] = "Cập nhật thiết lập kế toán thành công!";
            return RedirectToAction(nameof(AccountingConfig));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật cấu hình kế toán");
            ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
            return View(vm);
        }
    }

    // POST: /Company/LockBook
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockBook(DateTime ngayKhoaSo)
    {
        try
        {
            await _companyService.KhoaSoKeToanAsync(ngayKhoaSo);
            TempData["SuccessMessage"] = $"Đã khóa sổ kế toán đến hết ngày {ngayKhoaSo:dd/MM/yyyy}. Mọi thao tác thêm/sửa/xóa chứng từ trước và trong ngày này đều bị chặn!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khóa sổ");
            TempData["ErrorMessage"] = $"Lỗi khóa sổ: {ex.Message}";
        }

        return RedirectToAction(nameof(AccountingConfig));
    }

    // POST: /Company/UnlockBook
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockBook()
    {
        try
        {
            await _companyService.MoKhoaSoKeToanAsync();
            TempData["SuccessMessage"] = "Đã mở khóa sổ kế toán toàn bộ!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi mở khóa sổ");
            TempData["ErrorMessage"] = $"Lỗi mở khóa sổ: {ex.Message}";
        }

        return RedirectToAction(nameof(AccountingConfig));
    }

    // GET: /Company/Branches
    public async Task<IActionResult> Branches()
    {
        var branches = await _companyService.LayDanhSachChiNhanhAsync();
        return View(branches);
    }

    // GET: /Company/CreateBranch
    public IActionResult CreateBranch()
    {
        return View(new BranchEditViewModel());
    }

    // POST: /Company/CreateBranch
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBranch(BranchEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var branch = new ChiNhanh
            {
                MaChiNhanh = vm.MaChiNhanh.Trim(),
                TenChiNhanh = vm.TenChiNhanh.Trim(),
                MaSoThueChiNhanh = string.IsNullOrWhiteSpace(vm.MaSoThueChiNhanh) ? null : vm.MaSoThueChiNhanh.Trim(),
                LoaiChiNhanh = vm.LoaiChiNhanh,
                KeKhaiThueGtgtRieng = vm.KeKhaiThueGtgtRieng,
                KeKhaiThueTncnRieng = vm.KeKhaiThueTncnRieng,
                DiaChi = vm.DiaChi,
                TinhThanhPho = vm.TinhThanhPho,
                MaCoQuanThueQuanLyRieng = vm.MaCoQuanThueQuanLyRieng,
                TenCoQuanThueQuanLyRieng = vm.TenCoQuanThueQuanLyRieng,
                NguoiDungDau = vm.NguoiDungDau,
                SoDienThoai = vm.SoDienThoai,
                DangHoatDong = vm.DangHoatDong
            };

            await _companyService.ThemChiNhanhAsync(branch);
            TempData["SuccessMessage"] = $"Thêm chi nhánh '{branch.TenChiNhanh}' thành công!";
            return RedirectToAction(nameof(Branches));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi thêm chi nhánh");
            ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
            return View(vm);
        }
    }

    // GET: /Company/EditBranch/5
    public async Task<IActionResult> EditBranch(long id)
    {
        var branches = await _companyService.LayDanhSachChiNhanhAsync();
        var branch = branches.FirstOrDefault(b => b.Id == id);
        if (branch == null)
        {
            return NotFound();
        }

        var vm = new BranchEditViewModel
        {
            Id = branch.Id,
            DoanhNghiepId = branch.DoanhNghiepId,
            MaChiNhanh = branch.MaChiNhanh,
            TenChiNhanh = branch.TenChiNhanh,
            MaSoThueChiNhanh = branch.MaSoThueChiNhanh,
            LoaiChiNhanh = branch.LoaiChiNhanh,
            KeKhaiThueGtgtRieng = branch.KeKhaiThueGtgtRieng,
            KeKhaiThueTncnRieng = branch.KeKhaiThueTncnRieng,
            DiaChi = branch.DiaChi,
            TinhThanhPho = branch.TinhThanhPho,
            MaCoQuanThueQuanLyRieng = branch.MaCoQuanThueQuanLyRieng,
            TenCoQuanThueQuanLyRieng = branch.TenCoQuanThueQuanLyRieng,
            NguoiDungDau = branch.NguoiDungDau,
            SoDienThoai = branch.SoDienThoai,
            DangHoatDong = branch.DangHoatDong
        };

        return View(vm);
    }

    // POST: /Company/EditBranch/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBranch(long id, BranchEditViewModel vm)
    {
        if (id != vm.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var branch = new ChiNhanh
            {
                Id = vm.Id,
                DoanhNghiepId = vm.DoanhNghiepId,
                MaChiNhanh = vm.MaChiNhanh.Trim(),
                TenChiNhanh = vm.TenChiNhanh.Trim(),
                MaSoThueChiNhanh = string.IsNullOrWhiteSpace(vm.MaSoThueChiNhanh) ? null : vm.MaSoThueChiNhanh.Trim(),
                LoaiChiNhanh = vm.LoaiChiNhanh,
                KeKhaiThueGtgtRieng = vm.KeKhaiThueGtgtRieng,
                KeKhaiThueTncnRieng = vm.KeKhaiThueTncnRieng,
                DiaChi = vm.DiaChi,
                TinhThanhPho = vm.TinhThanhPho,
                MaCoQuanThueQuanLyRieng = vm.MaCoQuanThueQuanLyRieng,
                TenCoQuanThueQuanLyRieng = vm.TenCoQuanThueQuanLyRieng,
                NguoiDungDau = vm.NguoiDungDau,
                SoDienThoai = vm.SoDienThoai,
                DangHoatDong = vm.DangHoatDong
            };

            await _companyService.CapNhatChiNhanhAsync(branch);
            TempData["SuccessMessage"] = $"Cập nhật chi nhánh '{branch.TenChiNhanh}' thành công!";
            return RedirectToAction(nameof(Branches));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật chi nhánh");
            ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
            return View(vm);
        }
    }

    // POST: /Company/DeleteBranch/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBranch(long id)
    {
        try
        {
            await _companyService.XoaChiNhanhAsync(id);
            TempData["SuccessMessage"] = "Đã xóa chi nhánh thành công!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xóa chi nhánh");
            TempData["ErrorMessage"] = $"Lỗi xóa chi nhánh: {ex.Message}";
        }

        return RedirectToAction(nameof(Branches));
    }
}
