using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(
        IDepartmentService departmentService,
        AppDbContext context,
        ILogger<DepartmentController> logger)
    {
        _departmentService = departmentService;
        _context = context;
        _logger = logger;
    }

    // GET: /Department
    public async Task<IActionResult> Index(long? branchId = null)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        var flattened = await _departmentService.GetFlattenedHierarchyAsync(branchId);

        var vm = new DepartmentIndexViewModel
        {
            SelectedBranchId = branchId,
            BranchList = branches.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
                Selected = branchId.HasValue && branchId.Value == b.Id
            }).ToList(),
            Departments = flattened
        };

        return View(vm);
    }

    // GET: /Department/Create
    public async Task<IActionResult> Create(long? branchId = null, long? parentId = null)
    {
        var defaultBranch = branchId ?? (await _context.ChiNhanhs.Select(b => b.Id).FirstOrDefaultAsync());

        var vm = new DepartmentEditViewModel
        {
            ChiNhanhId = defaultBranch,
            PhongBanChaId = parentId,
            LoaiPhongBan = LoaiPhongBan.QuanLy,
            MaTaiKhoanChiPhi = "6422",
            DangHoatDong = true
        };

        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    // POST: /Department/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        try
        {
            var entity = new PhongBan
            {
                ChiNhanhId = vm.ChiNhanhId,
                PhongBanChaId = vm.PhongBanChaId > 0 ? vm.PhongBanChaId : null,
                MaPhongBan = (vm.MaPhongBan ?? string.Empty).Trim().ToUpper(),
                TenPhongBan = (vm.TenPhongBan ?? string.Empty).Trim(),
                TenTiengAnh = vm.TenTiengAnh?.Trim(),
                LoaiPhongBan = vm.LoaiPhongBan,
                MaTaiKhoanChiPhi = string.IsNullOrWhiteSpace(vm.MaTaiKhoanChiPhi)
                    ? vm.LoaiPhongBan.LayMaTaiKhoanChiPhiMacDinh()
                    : vm.MaTaiKhoanChiPhi.Trim(),
                TruongPhongId = vm.TruongPhongId > 0 ? vm.TruongPhongId : null,
                LaTrungTamLoiNhuan = vm.LaTrungTamLoiNhuan,
                DangHoatDong = vm.DangHoatDong,
                GhiChu = vm.GhiChu
            };

            await _departmentService.SaveDepartmentAsync(entity);
            TempData["SuccessMessage"] = $"Đã tạo phòng ban/bộ phận '{entity.TenPhongBan}' ({entity.MaPhongBan}) thành công.";
            return RedirectToAction(nameof(Index), new { branchId = entity.ChiNhanhId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo phòng ban mới: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }
    }

    // GET: /Department/Edit/{id}
    public async Task<IActionResult> Edit(long id)
    {
        var dept = await _departmentService.GetDepartmentByIdAsync(id);
        if (dept == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy phòng ban/bộ phận.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new DepartmentEditViewModel
        {
            Id = dept.Id,
            ChiNhanhId = dept.ChiNhanhId,
            PhongBanChaId = dept.PhongBanChaId,
            MaPhongBan = dept.MaPhongBan,
            TenPhongBan = dept.TenPhongBan,
            TenTiengAnh = dept.TenTiengAnh,
            LoaiPhongBan = dept.LoaiPhongBan,
            MaTaiKhoanChiPhi = dept.MaTaiKhoanChiPhi,
            TruongPhongId = dept.TruongPhongId,
            LaTrungTamLoiNhuan = dept.LaTrungTamLoiNhuan,
            DangHoatDong = dept.DangHoatDong,
            GhiChu = dept.GhiChu
        };

        await PopulateDropdownsAsync(vm, excludeDepartmentId: id);
        return View(vm);
    }

    // POST: /Department/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, DepartmentEditViewModel vm)
    {
        if (id != vm.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm, excludeDepartmentId: id);
            return View(vm);
        }

        try
        {
            var dept = await _departmentService.GetDepartmentByIdAsync(id);
            if (dept == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng ban/bộ phận cần cập nhật.";
                return RedirectToAction(nameof(Index));
            }

            dept.ChiNhanhId = vm.ChiNhanhId;
            dept.PhongBanChaId = vm.PhongBanChaId > 0 ? vm.PhongBanChaId : null;
            dept.MaPhongBan = (vm.MaPhongBan ?? string.Empty).Trim().ToUpper();
            dept.TenPhongBan = (vm.TenPhongBan ?? string.Empty).Trim();
            dept.TenTiengAnh = vm.TenTiengAnh?.Trim();
            dept.LoaiPhongBan = vm.LoaiPhongBan;
            dept.MaTaiKhoanChiPhi = string.IsNullOrWhiteSpace(vm.MaTaiKhoanChiPhi)
                ? vm.LoaiPhongBan.LayMaTaiKhoanChiPhiMacDinh()
                : vm.MaTaiKhoanChiPhi.Trim();
            dept.TruongPhongId = vm.TruongPhongId > 0 ? vm.TruongPhongId : null;
            dept.LaTrungTamLoiNhuan = vm.LaTrungTamLoiNhuan;
            dept.DangHoatDong = vm.DangHoatDong;
            dept.GhiChu = vm.GhiChu;

            await _departmentService.SaveDepartmentAsync(dept);
            TempData["SuccessMessage"] = $"Đã cập nhật phòng ban '{dept.TenPhongBan}' thành công.";
            return RedirectToAction(nameof(Index), new { branchId = dept.ChiNhanhId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật phòng ban {Id}: {Message}", id, ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownsAsync(vm, excludeDepartmentId: id);
            return View(vm);
        }
    }

    // POST: /Department/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var dept = await _departmentService.GetDepartmentByIdAsync(id);
            var branchId = dept?.ChiNhanhId;

            var ok = await _departmentService.DeleteDepartmentAsync(id);
            if (ok)
            {
                TempData["SuccessMessage"] = "Đã xóa phòng ban thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng ban cần xóa.";
            }
            return RedirectToAction(nameof(Index), new { branchId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể xóa phòng ban {Id}: {Message}", id, ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Department/GetTreeJson
    [HttpGet]
    public async Task<IActionResult> GetTreeJson(long? branchId = null)
    {
        var tree = await _departmentService.GetDepartmentTreeAsync(branchId);
        return Json(tree);
    }

    #region Helper Methods

    private async Task PopulateDropdownsAsync(DepartmentEditViewModel vm, long? excludeDepartmentId = null)
    {
        // 1. Chi nhánh
        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        vm.BranchList = branches.Select(b => new SelectListItem
        {
            Value = b.Id.ToString(),
            Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
            Selected = b.Id == vm.ChiNhanhId
        }).ToList();

        // 2. Phòng ban cha trong cùng chi nhánh (Loại trừ chính nó và các con của nó nếu đang sửa để tránh chu trình)
        var availableParents = await _departmentService.GetAvailableParentDepartmentsAsync(vm.ChiNhanhId, excludeDepartmentId);
        vm.ParentDepartmentList = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Là phòng ban gốc cấp 1 (Không có cha) --" }
        };
        vm.ParentDepartmentList.AddRange(availableParents.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.MaPhongBan} - {p.TenPhongBan}",
            Selected = vm.PhongBanChaId.HasValue && vm.PhongBanChaId.Value == p.Id
        }));

        // 3. Nhân viên trực thuộc (để chọn Trưởng phòng)
        var employees = await _context.NhanViens
            .AsNoTracking()
            .Where(n => n.DangLamViec)
            .OrderBy(n => n.HoTen)
            .ToListAsync();

        vm.EmployeeList = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Chưa chỉ định trưởng phòng --" }
        };
        vm.EmployeeList.AddRange(employees.Select(n => new SelectListItem
        {
            Value = n.Id.ToString(),
            Text = $"{n.MaNhanVien} - {n.HoTen} ({n.ChucVu ?? "Nhân viên"})",
            Selected = vm.TruongPhongId.HasValue && vm.TruongPhongId.Value == n.Id
        }));
    }

    #endregion
}

