using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

public class FxRevaluationController : Controller
{
    private readonly IFxRevaluationService _fxService;
    private readonly AppDbContext _context;
    private readonly ILogger<FxRevaluationController> _logger;

    public FxRevaluationController(
        IFxRevaluationService fxService,
        AppDbContext context,
        ILogger<FxRevaluationController> logger)
    {
        _fxService = fxService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _context.DanhGiaLaiNgoaiTes
            .Include(d => d.ButToanDanhGiaLai)
            .Include(d => d.ButToanKetChuyen413)
            .OrderByDescending(d => d.NgayHachToan)
            .ThenByDescending(d => d.Id)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create(DateTime? ngayDanhGia, string? loaiTien, decimal? tyGiaMua, decimal? tyGiaBan)
    {
        var ngay = ngayDanhGia ?? new DateTime(DateTime.Today.Year, 12, 31);
        var tien = string.IsNullOrWhiteSpace(loaiTien) ? "USD" : loaiTien.ToUpper();
        var mua = tyGiaMua ?? 25450m;
        var ban = tyGiaBan ?? 25820m;

        var preview = await _fxService.XemTruocDanhGiaLaiAsync(ngay, tien, mua, ban);
        return View(preview);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateTime ngayDanhGia, string loaiTien, decimal tyGiaMua, decimal tyGiaBan, string? ghiChu)
    {
        try
        {
            var result = await _fxService.ThucHienDanhGiaLaiCuoiKyAsync(ngayDanhGia, loaiTien, tyGiaMua, tyGiaBan, ghiChu);
            TempData["ThongBaoThanhCong"] = $"Đã thực hiện đánh giá lại ngoại tệ {result.SoChungTu} thành công! Số dư TK 4131 đã kết chuyển sạch về 0.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh giá lại tỷ giá ngoại tệ");
            TempData["ThongBaoLoi"] = ex.Message;
            return RedirectToAction(nameof(Create), new { ngayDanhGia, loaiTien, tyGiaMua, tyGiaBan });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(long id)
    {
        var (ok, msg) = await _fxService.HuyDanhGiaLaiAsync(id);
        if (!ok)
        {
            TempData["ThongBaoLoi"] = msg;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã hủy chứng từ đánh giá lại ngoại tệ thành công!";
        }
        return RedirectToAction(nameof(Index));
    }
}
