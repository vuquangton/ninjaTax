using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller quản lý danh mục Khách hàng, Nhà cung cấp, Đối tác liên danh (DoiTuong).
/// </summary>
public class DoiTuongController : Controller
{
    private readonly IDoiTuongService _doiTuongService;
    private readonly ILogger<DoiTuongController> _logger;

    public DoiTuongController(IDoiTuongService doiTuongService, ILogger<DoiTuongController> logger)
    {
        _doiTuongService = doiTuongService;
        _logger = logger;
    }

    // GET: /DoiTuong
    [HttpGet]
    public async Task<IActionResult> Index(string? timKiem, LoaiDoiTuong? loai)
    {
        var danhSach = await _doiTuongService.LayDanhSachAsync(timKiem, loai);
        var vm = new DoiTuongIndexViewModel
        {
            TimKiem = timKiem,
            LoaiFilter = loai,
            DanhSach = danhSach
        };
        return View(vm);
    }

    // GET: /DoiTuong/Create
    [HttpGet]
    public IActionResult Create(LoaiDoiTuong loai = LoaiDoiTuong.KhachHang)
    {
        var vm = new DoiTuongCreateEditViewModel
        {
            Loai = loai,
            DangHoatDong = true
        };
        return View(vm);
    }

    // POST: /DoiTuong/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoiTuongCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var (thanhCong, thongBao, id) = await _doiTuongService.TaoMoiAsync(vm);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi khi tạo đối tượng.");
            return View(vm);
        }

        TempData["SuccessMessage"] = $"Thêm mới đối tượng '{vm.MaDoiTuong}' thành công!";
        return RedirectToAction(nameof(Index), new { loai = vm.Loai });
    }

    // GET: /DoiTuong/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var dt = await _doiTuongService.LayTheoIdAsync(id);
        if (dt == null)
        {
            return NotFound();
        }

        var vm = new DoiTuongCreateEditViewModel
        {
            Id = dt.Id,
            MaDoiTuong = dt.MaDoiTuong,
            TenDoiTuong = dt.TenDoiTuong,
            Loai = dt.Loai,
            MaSoThue = dt.MaSoThue,
            DiaChi = dt.DiaChi,
            SoDienThoai = dt.SoDienThoai,
            Email = dt.Email,
            NguoiLienHe = dt.NguoiLienHe,
            SoTaiKhoanNganHang = dt.SoTaiKhoanNganHang,
            TenNganHang = dt.TenNganHang,
            DangHoatDong = dt.DangHoatDong
        };

        return View(vm);
    }

    // POST: /DoiTuong/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, DoiTuongCreateEditViewModel vm)
    {
        if (id != vm.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var (thanhCong, thongBao) = await _doiTuongService.CapNhatAsync(id, vm);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi khi cập nhật đối tượng.");
            return View(vm);
        }

        TempData["SuccessMessage"] = $"Cập nhật đối tượng '{vm.MaDoiTuong}' thành công!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /DoiTuong/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var dt = await _doiTuongService.LayTheoIdAsync(id);
        if (dt == null)
        {
            return NotFound();
        }

        var daPhatSinh = await _doiTuongService.KiemTraDaPhatSinhGiaoDichAsync(id);

        var vm = new DoiTuongItemViewModel
        {
            Id = dt.Id,
            MaDoiTuong = dt.MaDoiTuong,
            TenDoiTuong = dt.TenDoiTuong,
            Loai = dt.Loai,
            MaSoThue = dt.MaSoThue,
            DiaChi = dt.DiaChi,
            SoDienThoai = dt.SoDienThoai,
            Email = dt.Email,
            NguoiLienHe = dt.NguoiLienHe,
            DangHoatDong = dt.DangHoatDong,
            DaPhatSinhGiaoDich = daPhatSinh
        };

        return View(vm);
    }

    // POST: /DoiTuong/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var (thanhCong, thongBao) = await _doiTuongService.XoaAsync(id);
        if (thanhCong)
        {
            TempData["SuccessMessage"] = "Đã xóa đối tượng thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = thongBao ?? "Không thể xóa đối tượng.";
        }

        return RedirectToAction(nameof(Index));
    }
}
