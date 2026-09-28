using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller Quản trị Hệ thống Tài khoản kế toán (Chart of Accounts - COA) theo Thông tư 99/2025/TT-BTC.
/// Tuân thủ quy chuẩn Controller mỏng (Thin Controller):
/// Chỉ nhận HTTP request, gọi ITaiKhoanService xử lý và điều hướng Razor Views.
/// </summary>
public class TaiKhoanController : Controller
{
    private readonly ITaiKhoanService _taiKhoanService;
    private readonly ILogger<TaiKhoanController> _logger;

    public TaiKhoanController(ITaiKhoanService taiKhoanService, ILogger<TaiKhoanController> logger)
    {
        _taiKhoanService = taiKhoanService;
        _logger = logger;
    }

    /// <summary>
    /// Danh mục Hệ thống Tài khoản (Tree Table / Grid View)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? timKiem, LoaiTaiKhoan? loaiTaiKhoan)
    {
        ViewBag.TimKiem = timKiem;
        ViewBag.LoaiTaiKhoan = loaiTaiKhoan;

        var danhSach = await _taiKhoanService.LayDanhSachAsync(timKiem, loaiTaiKhoan);
        return View(danhSach);
    }

    /// <summary>
    /// Màn hình thêm mới tài khoản (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(long? parentId)
    {
        var model = new TaiKhoanCreateEditViewModel
        {
            TaiKhoanMeId = parentId,
            DangHoatDong = true
        };

        if (parentId.HasValue)
        {
            var parent = await _taiKhoanService.LayTheoIdAsync(parentId.Value);
            if (parent != null)
            {
                model.BacTaiKhoan = parent.BacTaiKhoan + 1;
                model.LoaiTaiKhoan = parent.LoaiTaiKhoan;
                model.TinhChat = parent.TinhChat;
                model.MaTaiKhoan = parent.MaTaiKhoan; // Gợi ý tiền tố
            }
        }

        await PrepareDropdownsAsync(model.TaiKhoanMeId);
        return View(model);
    }

    /// <summary>
    /// Xử lý thêm mới tài khoản (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiKhoanCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareDropdownsAsync(model.TaiKhoanMeId);
            return View(model);
        }

        var (thanhCong, thongBao, newId) = await _taiKhoanService.TaoMoiAsync(model);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Không thể tạo tài khoản.");
            await PrepareDropdownsAsync(model.TaiKhoanMeId);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Tạo mới tài khoản [{model.MaTaiKhoan}] thành công!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Màn hình chỉnh sửa tài khoản (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var account = await _taiKhoanService.LayTheoIdAsync(id);
        if (account == null)
        {
            TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        var allAccounts = await _taiKhoanService.LayDanhSachAsync();
        var currentVm = allAccounts.FirstOrDefault(t => t.Id == id);

        var model = new TaiKhoanCreateEditViewModel
        {
            Id = account.Id,
            MaTaiKhoan = account.MaTaiKhoan,
            TenTaiKhoan = account.TenTaiKhoan,
            TaiKhoanMeId = account.TaiKhoanMeId,
            BacTaiKhoan = account.BacTaiKhoan,
            LoaiTaiKhoan = account.LoaiTaiKhoan,
            TinhChat = account.TinhChat,
            LaTaiKhoanSoCai = account.LaTaiKhoanSoCai,
            DangHoatDong = account.DangHoatDong,
            DaPhatSinhGiaoDich = currentVm?.DaPhatSinhGiaoDich ?? false
        };

        await PrepareDropdownsAsync(model.TaiKhoanMeId, id);
        return View(model);
    }

    /// <summary>
    /// Xử lý cập nhật tài khoản (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, TaiKhoanCreateEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PrepareDropdownsAsync(model.TaiKhoanMeId, id);
            return View(model);
        }

        var (thanhCong, thongBao) = await _taiKhoanService.CapNhatAsync(id, model);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Không thể cập nhật tài khoản.");
            await PrepareDropdownsAsync(model.TaiKhoanMeId, id);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Cập nhật tài khoản [{model.MaTaiKhoan}] thành công!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Xử lý xóa tài khoản (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var (thanhCong, thongBao) = await _taiKhoanService.XoaAsync(id);
        if (!thanhCong)
        {
            TempData["ErrorMessage"] = thongBao;
        }
        else
        {
            TempData["SuccessMessage"] = "Đã xóa tài khoản kế toán thành công!";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// API trả về danh sách tài khoản dạng JSON phục vụ AG Grid hoặc Dropdown động
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTreeJson()
    {
        var list = await _taiKhoanService.LayDanhSachAsync();
        return Json(list);
    }

    private async Task PrepareDropdownsAsync(long? selectedParentId = null, long? excludeId = null)
    {
        var parents = await _taiKhoanService.LayDanhSachTaiKhoanMeAsync();
        if (excludeId.HasValue)
        {
            parents = parents.Where(p => p.Id != excludeId.Value).ToList();
        }

        ViewBag.TaiKhoanMeList = new SelectList(
            parents.Select(p => new { p.Id, TenHienThi = $"{p.MaTaiKhoan} - {p.TenTaiKhoan}" }),
            "Id", "TenHienThi", selectedParentId);
    }
}
