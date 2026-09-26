using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller mỏng quản lý Danh mục Vật tư hàng hóa và dịch vụ
/// </summary>
public class VatTuHangHoaController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<VatTuHangHoaController> _logger;

    public VatTuHangHoaController(AppDbContext context, ILogger<VatTuHangHoaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Xem danh sách vật tư hàng hóa");
        var danhSach = await _context.VatTuHangHoas
            .Include(v => v.TaiKhoanKho)
            .Include(v => v.TaiKhoanDoanhThu)
            .Include(v => v.TaiKhoanGiaVon)
            .OrderBy(v => v.MaVatTu)
            .ToListAsync();

        return View(new VatTuHangHoaIndexViewModel { DanhSachVatTu = danhSach });
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

        if (await _context.VatTuHangHoas.AnyAsync(v => v.MaVatTu == model.MaVatTu))
        {
            ModelState.AddModelError("MaVatTu", "Mã vật tư này đã tồn tại trong hệ thống.");
            await NapDanhSachTaiKhoanAsync(model);
            return View(model);
        }

        var vatTu = new VatTuHangHoa
        {
            MaVatTu = model.MaVatTu.Trim().ToUpper(),
            TenVatTu = model.TenVatTu.Trim(),
            DonViTinh = model.DonViTinh.Trim(),
            LoaiVatTu = model.LoaiVatTu,
            TaiKhoanKhoId = model.TaiKhoanKhoId,
            TaiKhoanDoanhThuId = model.TaiKhoanDoanhThuId,
            TaiKhoanGiaVonId = model.TaiKhoanGiaVonId,
            ThueSuatVatMacDinh = model.ThueSuatVatMacDinh,
            DonGiaMuaGanNhat = model.DonGiaMuaGanNhat,
            DonGiaBanTieuChuan = model.DonGiaBanTieuChuan,
            DangTheoDoiTonKho = model.DangTheoDoiTonKho,
            DangHoatDong = true
        };

        _context.VatTuHangHoas.Add(vatTu);
        await _context.SaveChangesAsync();

        TempData["ThongBaoThanhCong"] = $"Đã thêm vật tư hàng hóa: {vatTu.MaVatTu}";
        return RedirectToAction(nameof(Index));
    }

    private async Task NapDanhSachTaiKhoanAsync(VatTuHangHoaCreateViewModel model)
    {
        var accounts = await _context.TaiKhoans
            .Where(t => t.DangHoatDong && !t.MaTaiKhoan.StartsWith("911"))
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();

        model.DanhSachTaiKhoanKho = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("152") || t.MaTaiKhoan.StartsWith("156"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanDoanhThu = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("511"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanGiaVon = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("632"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();
    }
}
