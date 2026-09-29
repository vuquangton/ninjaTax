using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller quản lý Danh mục Kho hàng (Warehouses) phân bổ theo chi nhánh.
/// </summary>
public class KhoController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly AppDbContext _context;
    private readonly ILogger<KhoController> _logger;

    public KhoController(
        IInventoryService inventoryService,
        AppDbContext context,
        ILogger<KhoController> logger)
    {
        _inventoryService = inventoryService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(long? branchId)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        var warehouses = await _inventoryService.GetAllWarehousesAsync(branchId);

        var activeNhapKhoIds = await _context.PhieuNhapKhos.Select(p => p.KhoId).Distinct().ToHashSetAsync();
        var activeXuatKhoIds = await _context.PhieuXuatKhos.Select(p => p.KhoId).Distinct().ToHashSetAsync();

        var list = warehouses.Select(k => new KhoItemViewModel
        {
            Id = k.Id,
            MaKho = k.MaKho,
            TenKho = k.TenKho,
            DiaChi = k.DiaChi,
            TenChiNhanh = k.ChiNhanh?.TenChiNhanh ?? "-",
            TenThuKho = k.ThuKho?.HoTen,
            TaiKhoanKhoMa = k.TaiKhoanKhoMacDinh?.MaTaiKhoan,
            DangHoatDong = k.DangHoatDong,
            DaPhatSinhPhieuKho = activeNhapKhoIds.Contains(k.Id) || activeXuatKhoIds.Contains(k.Id)
        }).ToList();

        var vm = new KhoIndexViewModel
        {
            BranchId = branchId,
            Warehouses = list,
            BranchList = branches.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
                Selected = branchId.HasValue && branchId.Value == b.Id
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(long? branchId)
    {
        var defaultBranch = branchId ?? (await _context.ChiNhanhs.Select(b => b.Id).FirstOrDefaultAsync());

        var vm = new KhoCreateEditViewModel
        {
            ChiNhanhId = defaultBranch,
            DangHoatDong = true
        };

        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KhoCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        try
        {
            var kho = new Kho
            {
                ChiNhanhId = vm.ChiNhanhId,
                MaKho = vm.MaKho,
                TenKho = vm.TenKho,
                DiaChi = vm.DiaChi,
                ThuKhoId = vm.ThuKhoId,
                TaiKhoanKhoMacDinhId = vm.TaiKhoanKhoMacDinhId,
                DangHoatDong = vm.DangHoatDong,
                GhiChu = vm.GhiChu
            };

            await _inventoryService.SaveWarehouseAsync(kho);
            TempData["SuccessMessage"] = $"Đã tạo mới kho hàng '{kho.MaKho}' thành công!";
            return RedirectToAction(nameof(Index), new { branchId = vm.ChiNhanhId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo kho hàng");
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var kho = await _inventoryService.GetWarehouseByIdAsync(id);
        if (kho == null)
        {
            return NotFound();
        }

        var canDelete = await _inventoryService.CanDeleteWarehouseAsync(id);

        var vm = new KhoCreateEditViewModel
        {
            Id = kho.Id,
            ChiNhanhId = kho.ChiNhanhId,
            MaKho = kho.MaKho,
            TenKho = kho.TenKho,
            DiaChi = kho.DiaChi,
            ThuKhoId = kho.ThuKhoId,
            TaiKhoanKhoMacDinhId = kho.TaiKhoanKhoMacDinhId,
            DangHoatDong = kho.DangHoatDong,
            GhiChu = kho.GhiChu,
            DaPhatSinhPhieuKho = !canDelete
        };

        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, KhoCreateEditViewModel vm)
    {
        if (id != vm.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        try
        {
            var kho = new Kho
            {
                Id = vm.Id,
                ChiNhanhId = vm.ChiNhanhId,
                MaKho = vm.MaKho,
                TenKho = vm.TenKho,
                DiaChi = vm.DiaChi,
                ThuKhoId = vm.ThuKhoId,
                TaiKhoanKhoMacDinhId = vm.TaiKhoanKhoMacDinhId,
                DangHoatDong = vm.DangHoatDong,
                GhiChu = vm.GhiChu
            };

            await _inventoryService.SaveWarehouseAsync(kho);
            TempData["SuccessMessage"] = $"Đã cập nhật kho hàng '{kho.MaKho}' thành công!";
            return RedirectToAction(nameof(Index), new { branchId = vm.ChiNhanhId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật kho hàng");
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var (thanhCong, thongBao) = await _inventoryService.DeleteWarehouseAsync(id);
        if (thanhCong)
        {
            TempData["SuccessMessage"] = "Đã xóa kho hàng thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = thongBao ?? "Không thể xóa kho hàng.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(KhoCreateEditViewModel vm)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        vm.BranchList = branches.Select(b => new SelectListItem
        {
            Value = b.Id.ToString(),
            Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
            Selected = b.Id == vm.ChiNhanhId
        }).ToList();

        var employees = await _context.NhanViens.AsNoTracking()
            .Where(n => n.DangLamViec)
            .OrderBy(n => n.HoTen)
            .ToListAsync();
        vm.EmployeeList = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Chọn thủ kho --" }
        };
        vm.EmployeeList.AddRange(employees.Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = $"{e.MaNhanVien} - {e.HoTen} ({e.ChucVu ?? "Thủ kho"})",
            Selected = vm.ThuKhoId.HasValue && vm.ThuKhoId.Value == e.Id
        }));

        var accounts = await _context.TaiKhoans.AsNoTracking()
            .Where(t => t.DangHoatDong && !t.MaTaiKhoan.StartsWith("911") && (t.MaTaiKhoan.StartsWith("152") || t.MaTaiKhoan.StartsWith("153") || t.MaTaiKhoan.StartsWith("155") || t.MaTaiKhoan.StartsWith("156")))
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();
        vm.AccountList = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Chọn TK kho ngầm định --" }
        };
        vm.AccountList.AddRange(accounts.Select(a => new SelectListItem
        {
            Value = a.Id.ToString(),
            Text = $"{a.MaTaiKhoan} - {a.TenTaiKhoan}",
            Selected = vm.TaiKhoanKhoMacDinhId.HasValue && vm.TaiKhoanKhoMacDinhId.Value == a.Id
        }));
    }
}
