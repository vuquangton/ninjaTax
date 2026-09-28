using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller quản lý danh mục Tài khoản ngân hàng công ty (TK 1121).
/// </summary>
public class TaiKhoanNganHangController : Controller
{
    private readonly ITaiKhoanNganHangService _bankService;
    private readonly AppDbContext _context;
    private readonly ILogger<TaiKhoanNganHangController> _logger;

    public TaiKhoanNganHangController(
        ITaiKhoanNganHangService bankService,
        AppDbContext context,
        ILogger<TaiKhoanNganHangController> logger)
    {
        _bankService = bankService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? timKiem)
    {
        var danhSach = await _bankService.LayDanhSachAsync(timKiem);
        return View(new TaiKhoanNganHangIndexViewModel
        {
            TimKiem = timKiem,
            DanhSach = danhSach
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new TaiKhoanNganHangCreateEditViewModel
        {
            DangHoatDong = true
        };
        await PopulateAccountDropdownAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiKhoanNganHangCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAccountDropdownAsync(vm);
            return View(vm);
        }

        var (thanhCong, thongBao, id) = await _bankService.TaoMoiAsync(vm);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi tạo tài khoản ngân hàng.");
            await PopulateAccountDropdownAsync(vm);
            return View(vm);
        }

        TempData["SuccessMessage"] = $"Đã thêm tài khoản ngân hàng: {vm.SoTaiKhoan} ({vm.TenNganHang})";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var tk = await _bankService.LayTheoIdAsync(id);
        if (tk == null)
        {
            return NotFound();
        }

        var vm = new TaiKhoanNganHangCreateEditViewModel
        {
            Id = tk.Id,
            SoTaiKhoan = tk.SoTaiKhoan,
            TenNganHang = tk.TenNganHang,
            ChiNhanh = tk.ChiNhanh,
            ChuTaiKhoan = tk.ChuTaiKhoan,
            SoDuBanDau = tk.SoDuBanDau,
            TaiKhoanKeToanId = tk.TaiKhoanKeToanId,
            DangHoatDong = tk.DangHoatDong,
            GhiChu = tk.GhiChu
        };

        await PopulateAccountDropdownAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, TaiKhoanNganHangCreateEditViewModel vm)
    {
        if (id != vm.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateAccountDropdownAsync(vm);
            return View(vm);
        }

        var (thanhCong, thongBao) = await _bankService.CapNhatAsync(id, vm);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi cập nhật tài khoản ngân hàng.");
            await PopulateAccountDropdownAsync(vm);
            return View(vm);
        }

        TempData["SuccessMessage"] = $"Đã cập nhật tài khoản ngân hàng: {vm.SoTaiKhoan}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var (thanhCong, thongBao) = await _bankService.XoaAsync(id);
        if (thanhCong)
        {
            TempData["SuccessMessage"] = "Đã xóa tài khoản ngân hàng thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = thongBao ?? "Không thể xóa tài khoản ngân hàng.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAccountDropdownAsync(TaiKhoanNganHangCreateEditViewModel vm)
    {
        var accounts = await _context.TaiKhoans.AsNoTracking()
            .Where(t => t.DangHoatDong && !t.MaTaiKhoan.StartsWith("911") && t.MaTaiKhoan.StartsWith("112"))
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();

        vm.AccountList = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Chọn TK Kế toán 112 --" }
        };
        vm.AccountList.AddRange(accounts.Select(a => new SelectListItem
        {
            Value = a.Id.ToString(),
            Text = $"{a.MaTaiKhoan} - {a.TenTaiKhoan}",
            Selected = vm.TaiKhoanKeToanId.HasValue && vm.TaiKhoanKeToanId.Value == a.Id
        }));
    }
}
