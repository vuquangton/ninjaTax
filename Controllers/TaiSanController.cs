using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller quản lý Tài sản cố định (TK 211), Công cụ dụng cụ (TK 242) và Bảng trích khấu hao tự động (TT 45 & TT99).
/// </summary>
public class TaiSanController : Controller
{
    private readonly ITaiSanService _taiSanService;
    private readonly AppDbContext _context;
    private readonly ILogger<TaiSanController> _logger;

    public TaiSanController(
        ITaiSanService taiSanService,
        AppDbContext context,
        ILogger<TaiSanController> logger)
    {
        _taiSanService = taiSanService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(LoaiTaiSan? loai, TrangThaiTaiSan? trangThai, string? tuKhoa)
    {
        var model = await _taiSanService.TimKiemTaiSanAsync(loai, trangThai, tuKhoa);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var taiSan = await _taiSanService.LayChiTietTaiSanAsync(id);
        if (taiSan == null)
        {
            return NotFound();
        }
        return View(taiSan);
    }

    [HttpGet]
    public async Task<IActionResult> Create(LoaiTaiSan loai = LoaiTaiSan.TaiSanCoDinh)
    {
        await LoadDropdownsAsync(loai);

        var model = new TaiSanCreateViewModel
        {
            LoaiTaiSan = loai,
            NgayGhiTang = DateTime.Today,
            NgayBatDauKhauHao = DateTime.Today,
            ThoiGianSuDungThang = loai == LoaiTaiSan.TaiSanCoDinh ? 60 : 24
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiSanCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(model.LoaiTaiSan);
            return View(model);
        }

        try
        {
            var taiSan = await _taiSanService.KhaiBaoTaiSanAsync(model);
            TempData["ThanhCong"] = $"Khai báo thành công {(taiSan.LoaiTaiSan == LoaiTaiSan.TaiSanCoDinh ? "Tài sản cố định" : "Công cụ dụng cụ")} mã {taiSan.MaTaiSan}.";
            return RedirectToAction(nameof(Details), new { id = taiSan.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi khai báo tài sản");
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDropdownsAsync(model.LoaiTaiSan);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> BangKhauHao(string? kyKeToan)
    {
        var ky = string.IsNullOrWhiteSpace(kyKeToan) ? DateTime.Today.ToString("yyyy-MM") : kyKeToan.Trim();
        try
        {
            var model = await _taiSanService.XemBangKhauHaoKyAsync(ky);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Loi"] = ex.Message;
            return View(new BangKhauHaoKyViewModel { KyKeToan = ky });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChayKhauHao(string kyKeToan)
    {
        try
        {
            var result = await _taiSanService.ChayVaGhiSoKhauHaoKyAsync(kyKeToan);
            TempData["ThanhCong"] = $"Đã thực hiện trích khấu hao và tự động sinh bút toán sổ cái GL cho kỳ {kyKeToan} thành công ({result.Count} tài sản/CCDC).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi trích khấu hao kỳ {Ky}", kyKeToan);
            TempData["Loi"] = ex.Message;
        }

        return RedirectToAction(nameof(BangKhauHao), new { kyKeToan });
    }

    private async Task LoadDropdownsAsync(LoaiTaiSan loai)
    {
        var tkNguyenGias = loai == LoaiTaiSan.TaiSanCoDinh
            ? await _context.TaiKhoans.Where(t => t.MaTaiKhoan.StartsWith("211")).OrderBy(t => t.MaTaiKhoan).ToListAsync()
            : await _context.TaiKhoans.Where(t => t.MaTaiKhoan.StartsWith("242") || t.MaTaiKhoan.StartsWith("153")).OrderBy(t => t.MaTaiKhoan).ToListAsync();

        ViewBag.TaiKhoanNguyenGias = new SelectList(tkNguyenGias, "Id", "TenHienThiDropdown");

        var tkKhauHaos = await _context.TaiKhoans.Where(t => t.MaTaiKhoan.StartsWith("214") || t.MaTaiKhoan.StartsWith("242")).OrderBy(t => t.MaTaiKhoan).ToListAsync();
        ViewBag.TaiKhoanKhauHaos = new SelectList(tkKhauHaos, "Id", "TenHienThiDropdown");

        var tkChiPhis = await _context.TaiKhoans.Where(t => t.MaTaiKhoan.StartsWith("642") || t.MaTaiKhoan.StartsWith("641") || t.MaTaiKhoan.StartsWith("627") || t.MaTaiKhoan.StartsWith("154")).OrderBy(t => t.MaTaiKhoan).ToListAsync();
        ViewBag.TaiKhoanChiPhis = new SelectList(tkChiPhis, "Id", "TenHienThiDropdown");
    }
}
