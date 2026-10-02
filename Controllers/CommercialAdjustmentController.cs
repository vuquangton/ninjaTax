using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

public class CommercialAdjustmentController : Controller
{
    private readonly ICommercialAdjustmentService _adjustmentService;
    private readonly AppDbContext _context;
    private readonly ILogger<CommercialAdjustmentController> _logger;

    public CommercialAdjustmentController(
        ICommercialAdjustmentService adjustmentService,
        AppDbContext context,
        ILogger<CommercialAdjustmentController> logger)
    {
        _adjustmentService = adjustmentService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(LoaiDieuChinhThuongMai? loai = null, DateTime? tuNgay = null, DateTime? denNgay = null, long? branchId = null)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().ToListAsync();
        ViewBag.Branches = new SelectList(branches, "Id", "TenChiNhanh", branchId);
        ViewBag.CurrentLoai = loai;

        var list = await _adjustmentService.LayDanhSachAsync(loai, tuNgay, denNgay, branchId);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create(LoaiDieuChinhThuongMai? loai = null)
    {
        var branch = await _context.ChiNhanhs.FirstOrDefaultAsync();
        var branchId = branch?.Id ?? 1;

        var selectedLoai = loai ?? LoaiDieuChinhThuongMai.HangBanTraLai;
        var prefix = selectedLoai switch
        {
            LoaiDieuChinhThuongMai.HangBanTraLai => "HBTL",
            LoaiDieuChinhThuongMai.HangMuaTraLai => "HMTL",
            LoaiDieuChinhThuongMai.ChietKhauThuongMaiBan => "CKTM",
            _ => "GGHM"
        };

        var isPurchase = selectedLoai == LoaiDieuChinhThuongMai.HangMuaTraLai || selectedLoai == LoaiDieuChinhThuongMai.GiamGiaHangMua;
        var doiTuongs = await _context.DoiTuongs
            .Where(d => d.DangHoatDong && d.Loai == (isPurchase ? LoaiDoiTuong.NhaCungCap : LoaiDoiTuong.KhachHang))
            .OrderBy(d => d.TenDoiTuong)
            .ToListAsync();
        ViewBag.DoiTuongs = new SelectList(doiTuongs, "Id", "TenDoiTuong");

        var khos = await _context.Khos.Where(k => k.DangHoatDong && k.ChiNhanhId == branchId).ToListAsync();
        ViewBag.Khos = new SelectList(khos, "Id", "TenKho");

        var vatTus = await _context.VatTuHangHoas.Where(v => v.DangHoatDong).OrderBy(v => v.TenVatTu).ToListAsync();
        ViewBag.VatTus = vatTus;

        var model = new ChungTuDieuChinhThuongMai
        {
            ChiNhanhId = branchId,
            LoaiDieuChinh = selectedLoai,
            SoChungTu = $"{prefix}-{DateTime.Now:yyyyMMddHHmm}",
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            HinhThucXuLy = HinhThucXuLyDieuChinh.GiamTruCongNo
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChungTuDieuChinhThuongMai model, List<long> vatTuId, List<decimal> soLuong, List<decimal> donGia, List<decimal> thueSuat, List<decimal> donGiaVon)
    {
        if (model.DoiTuongId <= 0)
        {
            ModelState.AddModelError("", "Vui lòng chọn Đối tượng (Khách hàng / Nhà cung cấp).");
        }

        if (vatTuId == null || !vatTuId.Any())
        {
            ModelState.AddModelError("", "Vui lòng thêm ít nhất 1 dòng mặt hàng điều chỉnh.");
        }

        if (!ModelState.IsValid || vatTuId == null)
        {
            await PopulateDropDownsAsync(model);
            return View(model);
        }

        var details = new List<ChiTietDieuChinhThuongMai>();
        var safeQty = soLuong ?? new List<decimal>();
        var safePrice = donGia ?? new List<decimal>();
        var safeVat = thueSuat ?? new List<decimal>();
        var safeCost = donGiaVon ?? new List<decimal>();

        for (int i = 0; i < vatTuId.Count; i++)
        {
            var vtId = vatTuId[i];
            var sl = (i < safeQty.Count) ? safeQty[i] : 1;
            var dg = (i < safePrice.Count) ? safePrice[i] : 0;
            var vat = (i < safeVat.Count) ? safeVat[i] : 0;
            var gv = (i < safeCost.Count) ? safeCost[i] : 0;
            var vt = await _context.VatTuHangHoas.FindAsync(vtId);

            details.Add(new ChiTietDieuChinhThuongMai
            {
                VatTuHangHoaId = vtId,
                DonViTinh = vt?.DonViTinh ?? "Cái",
                SoLuong = sl,
                DonGia = dg,
                ThueSuatVat = vat,
                DonGiaVonNhapLai = gv
            });
        }

        model.ChiTietDieuChinhs = details;

        try
        {
            var result = await _adjustmentService.TaoChungTuAsync(model);
            TempData["SuccessMessage"] = $"Đã lập và ghi sổ chứng từ điều chỉnh {result.SoChungTu} thành công.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi lập chứng từ điều chỉnh thương mại");
            ModelState.AddModelError("", ex.Message);
            await PopulateDropDownsAsync(model);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var chungTu = await _adjustmentService.LayChiTietAsync(id);
        if (chungTu == null) return NotFound();
        return View(chungTu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        var ok = await _adjustmentService.HuyGhiSoAsync(id);
        if (ok)
        {
            TempData["SuccessMessage"] = "Đã hủy chứng từ điều chỉnh thương mại thành công.";
        }
        else
        {
            TempData["ErrorMessage"] = "Không tìm thấy hoặc không thể hủy chứng từ này.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropDownsAsync(ChungTuDieuChinhThuongMai model)
    {
        var isPurchase = model.LoaiDieuChinh == LoaiDieuChinhThuongMai.HangMuaTraLai || model.LoaiDieuChinh == LoaiDieuChinhThuongMai.GiamGiaHangMua;
        var doiTuongs = await _context.DoiTuongs
            .Where(d => d.DangHoatDong && d.Loai == (isPurchase ? LoaiDoiTuong.NhaCungCap : LoaiDoiTuong.KhachHang))
            .OrderBy(d => d.TenDoiTuong)
            .ToListAsync();
        ViewBag.DoiTuongs = new SelectList(doiTuongs, "Id", "TenDoiTuong", model.DoiTuongId);

        var khos = await _context.Khos.Where(k => k.DangHoatDong && k.ChiNhanhId == model.ChiNhanhId).ToListAsync();
        ViewBag.Khos = new SelectList(khos, "Id", "TenKho", model.KhoId);

        var vatTus = await _context.VatTuHangHoas.Where(v => v.DangHoatDong).OrderBy(v => v.TenVatTu).ToListAsync();
        ViewBag.VatTus = vatTus;
    }
}
