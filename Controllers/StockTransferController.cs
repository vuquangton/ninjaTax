using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

public class StockTransferController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly AppDbContext _context;
    private readonly ILogger<StockTransferController> _logger;

    public StockTransferController(
        IInventoryService inventoryService,
        AppDbContext context,
        ILogger<StockTransferController> logger)
    {
        _inventoryService = inventoryService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? tuNgay = null, DateTime? denNgay = null, long? branchId = null)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().ToListAsync();
        ViewBag.Branches = new SelectList(branches, "Id", "TenChiNhanh", branchId);

        var list = await _inventoryService.LayDanhSachPhieuDieuChuyenAsync(tuNgay, denNgay, branchId);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var branch = await _context.ChiNhanhs.FirstOrDefaultAsync();
        var branchId = branch?.Id ?? 1;

        var khos = await _inventoryService.GetActiveWarehousesAsync(branchId);
        ViewBag.Khos = new SelectList(khos, "Id", "TenKho");

        var vatTus = await _context.VatTuHangHoas
            .Where(v => v.DangHoatDong && v.DangTheoDoiTonKho)
            .OrderBy(v => v.TenVatTu)
            .ToListAsync();
        ViewBag.VatTus = vatTus;

        var model = new PhieuDieuChuyenKho
        {
            ChiNhanhId = branchId,
            SoPhieu = $"DCK-{DateTime.Now:yyyyMMddHHmm}",
            NgayDieuChuyen = DateTime.Today,
            NgayHachToan = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PhieuDieuChuyenKho model, List<long> vatTuId, List<decimal> soLuong, List<decimal> donGia)
    {
        if (model.KhoXuatId <= 0 || model.KhoNhapId <= 0)
        {
            ModelState.AddModelError("", "Vui lòng chọn Kho xuất và Kho nhập hợp lệ.");
        }
        else if (model.KhoXuatId == model.KhoNhapId)
        {
            ModelState.AddModelError("", "Kho xuất và Kho nhập không được trùng nhau.");
        }

        if (vatTuId == null || !vatTuId.Any())
        {
            ModelState.AddModelError("", "Vui lòng thêm ít nhất 1 mặt hàng điều chuyển.");
        }

        if (!ModelState.IsValid || vatTuId == null)
        {
            var khos = await _inventoryService.GetActiveWarehousesAsync(model.ChiNhanhId);
            ViewBag.Khos = new SelectList(khos, "Id", "TenKho");
            ViewBag.VatTus = await _context.VatTuHangHoas.Where(v => v.DangHoatDong && v.DangTheoDoiTonKho).ToListAsync();
            return View(model);
        }

        var details = new List<ChiTietDieuChuyenKho>();
        var safeQty = soLuong ?? new List<decimal>();
        var safePrice = donGia ?? new List<decimal>();

        for (int i = 0; i < vatTuId.Count; i++)
        {
            var vtId = vatTuId[i];
            var sl = (i < safeQty.Count) ? safeQty[i] : 1;
            var dg = (i < safePrice.Count) ? safePrice[i] : 0;
            var vt = await _context.VatTuHangHoas.FindAsync(vtId);

            details.Add(new ChiTietDieuChuyenKho
            {
                VatTuHangHoaId = vtId,
                DonViTinh = vt?.DonViTinh ?? "Cái",
                SoLuong = sl,
                DonGiaVon = dg,
                ThanhTien = Math.Round(sl * dg, 4)
            });
        }

        model.ChiTietDieuChuyens = details;

        try
        {
            var result = await _inventoryService.TaoPhieuDieuChuyenKhoAsync(model);
            TempData["SuccessMessage"] = $"Đã lập và ghi sổ phiếu điều chuyển kho {result.SoPhieu} thành công.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi lập phiếu điều chuyển kho");
            ModelState.AddModelError("", ex.Message);

            var khos = await _inventoryService.GetActiveWarehousesAsync(model.ChiNhanhId);
            ViewBag.Khos = new SelectList(khos, "Id", "TenKho");
            ViewBag.VatTus = await _context.VatTuHangHoas.Where(v => v.DangHoatDong && v.DangTheoDoiTonKho).ToListAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var phieu = await _inventoryService.LayChiTietPhieuDieuChuyenAsync(id);
        if (phieu == null) return NotFound();
        return View(phieu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        var ok = await _inventoryService.HuyPhieuDieuChuyenKhoAsync(id);
        if (ok)
        {
            TempData["SuccessMessage"] = "Đã hủy phiếu điều chuyển kho và phục hồi tồn kho thành công.";
        }
        else
        {
            TempData["ErrorMessage"] = "Không tìm thấy hoặc không thể hủy phiếu điều chuyển này.";
        }
        return RedirectToAction(nameof(Index));
    }
}
