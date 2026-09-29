using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller quản lý Danh mục Vật tư, hàng hóa và dịch vụ (chuẩn hóa kiến trúc Thin Controller).
/// </summary>
public class VatTuHangHoaController : Controller
{
    private readonly IVatTuHangHoaService _vatTuService;
    private readonly AppDbContext _context;
    private readonly ILogger<VatTuHangHoaController> _logger;

    public VatTuHangHoaController(
        IVatTuHangHoaService vatTuService,
        AppDbContext context,
        ILogger<VatTuHangHoaController> logger)
    {
        _vatTuService = vatTuService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? timKiem, LoaiVatTuHangHoa? loai)
    {
        _logger.LogInformation("Xem danh sách vật tư hàng hóa");
        var danhSach = await _vatTuService.LayDanhSachAsync(timKiem, loai);
        return View(new VatTuHangHoaIndexViewModel
        {
            TimKiem = timKiem,
            LoaiFilter = loai,
            DanhSachVatTu = danhSach
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new VatTuHangHoaCreateViewModel();
        await NapDanhSachTaiKhoanAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VatTuHangHoaCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await NapDanhSachTaiKhoanAsync(model);
            return View(model);
        }

        var (thanhCong, thongBao, id) = await _vatTuService.TaoMoiAsync(model);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi tạo mới vật tư.");
            await NapDanhSachTaiKhoanAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Đã thêm mới vật tư hàng hóa: {model.MaVatTu.Trim().ToUpper()}";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var entity = await _vatTuService.LayTheoIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        var daPhatSinh = await _vatTuService.KiemTraDaPhatSinhGiaoDichAsync(id);

        var model = new VatTuHangHoaEditViewModel
        {
            Id = entity.Id,
            MaVatTu = entity.MaVatTu,
            TenVatTu = entity.TenVatTu,
            DonViTinh = entity.DonViTinh,
            LoaiVatTu = entity.LoaiVatTu,
            TaiKhoanKhoId = entity.TaiKhoanKhoId,
            TaiKhoanDoanhThuId = entity.TaiKhoanDoanhThuId,
            TaiKhoanGiaVonId = entity.TaiKhoanGiaVonId,
            ThueSuatVatMacDinh = entity.ThueSuatVatMacDinh,
            DonGiaMuaGanNhat = entity.DonGiaMuaGanNhat,
            DonGiaBanTieuChuan = entity.DonGiaBanTieuChuan,
            DangTheoDoiTonKho = entity.DangTheoDoiTonKho,
            DangHoatDong = entity.DangHoatDong,
            DaPhatSinhGiaoDich = daPhatSinh
        };

        await NapDanhSachTaiKhoanEditAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, VatTuHangHoaEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await NapDanhSachTaiKhoanEditAsync(model);
            return View(model);
        }

        var (thanhCong, thongBao) = await _vatTuService.CapNhatAsync(id, model);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi cập nhật vật tư.");
            await NapDanhSachTaiKhoanEditAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Đã cập nhật vật tư hàng hóa: {model.MaVatTu}";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var entity = await _vatTuService.LayTheoIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        var daPhatSinh = await _vatTuService.KiemTraDaPhatSinhGiaoDichAsync(id);

        var vm = new VatTuHangHoaItemViewModel
        {
            Id = entity.Id,
            MaVatTu = entity.MaVatTu,
            TenVatTu = entity.TenVatTu,
            DonViTinh = entity.DonViTinh,
            LoaiVatTu = entity.LoaiVatTu,
            ThueSuatVatMacDinh = entity.ThueSuatVatMacDinh,
            DonGiaMuaGanNhat = entity.DonGiaMuaGanNhat,
            DonGiaBanTieuChuan = entity.DonGiaBanTieuChuan,
            DangHoatDong = entity.DangHoatDong,
            DangTheoDoiTonKho = entity.DangTheoDoiTonKho,
            DaPhatSinhGiaoDich = daPhatSinh,
            TaiKhoanKhoMa = entity.TaiKhoanKho?.MaTaiKhoan,
            TaiKhoanDoanhThuMa = entity.TaiKhoanDoanhThu?.MaTaiKhoan,
            TaiKhoanGiaVonMa = entity.TaiKhoanGiaVon?.MaTaiKhoan
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var (thanhCong, thongBao) = await _vatTuService.XoaAsync(id);
        if (thanhCong)
        {
            TempData["SuccessMessage"] = "Đã xóa vật tư hàng hóa thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = thongBao ?? "Không thể xóa vật tư hàng hóa.";
        }

        return RedirectToAction(nameof(Index));
    }

    #region Helper Methods

    private async Task NapDanhSachTaiKhoanAsync(VatTuHangHoaCreateViewModel model)
    {
        var accounts = await _context.TaiKhoans
            .Where(t => t.DangHoatDong && !t.MaTaiKhoan.StartsWith("911"))
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();

        model.DanhSachTaiKhoanKho = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("152") || t.MaTaiKhoan.StartsWith("153") || t.MaTaiKhoan.StartsWith("155") || t.MaTaiKhoan.StartsWith("156"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanDoanhThu = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("511") || t.MaTaiKhoan.StartsWith("711"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanGiaVon = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("632") || t.MaTaiKhoan.StartsWith("811") || t.MaTaiKhoan.StartsWith("642"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();
    }

    private async Task NapDanhSachTaiKhoanEditAsync(VatTuHangHoaEditViewModel model)
    {
        var accounts = await _context.TaiKhoans
            .Where(t => t.DangHoatDong && !t.MaTaiKhoan.StartsWith("911"))
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();

        model.DanhSachTaiKhoanKho = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("152") || t.MaTaiKhoan.StartsWith("153") || t.MaTaiKhoan.StartsWith("155") || t.MaTaiKhoan.StartsWith("156"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanDoanhThu = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("511") || t.MaTaiKhoan.StartsWith("711"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanGiaVon = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("632") || t.MaTaiKhoan.StartsWith("811") || t.MaTaiKhoan.StartsWith("642"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();
    }

    #endregion
}
