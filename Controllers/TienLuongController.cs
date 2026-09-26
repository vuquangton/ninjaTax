using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller Quản lý Tính lương, Trích nộp Bảo hiểm, Thuế TNCN & Hạch toán Sổ cái TT99 (Phase 4 Payroll).
/// </summary>
public class TienLuongController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly ILogger<TienLuongController> _logger;

    public TienLuongController(IPayrollService payrollService, ILogger<TienLuongController> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? kyKeToan)
    {
        var danhSach = await _payrollService.LayDanhSachBangLuongAsync();
        var model = new BangLuongIndexViewModel
        {
            KyKeToan = string.IsNullOrWhiteSpace(kyKeToan) ? DateTime.Today.ToString("yyyy-MM") : kyKeToan.Trim(),
            DanhSachBangLuong = danhSach
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> BangLuong(string kyKeToan)
    {
        var ky = string.IsNullOrWhiteSpace(kyKeToan) ? DateTime.Today.ToString("yyyy-MM") : kyKeToan.Trim();
        try
        {
            var model = await _payrollService.TinhLuongThangAsync(ky);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải bảng lương kỳ {KyKeToan}", ky);
            TempData["ErrorMessage"] = $"Lỗi tính bảng lương: {ex.Message}";
            return RedirectToAction(nameof(Index), new { kyKeToan = ky });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TinhLuong(string kyKeToan)
    {
        try
        {
            await _payrollService.TinhLuongThangAsync(kyKeToan);
            TempData["SuccessMessage"] = $"Tính lương kỳ {kyKeToan} thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính lương kỳ {KyKeToan}", kyKeToan);
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
        }

        return RedirectToAction(nameof(BangLuong), new { kyKeToan });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GhiSo(long bangLuongId, string kyKeToan)
    {
        try
        {
            var bl = await _payrollService.GhiSoBangLuongAsync(bangLuongId);
            TempData["SuccessMessage"] = $"Ghi sổ Sổ Cái TT99 thành công cho bảng lương kỳ {bl.KyKeToan}! Đã sinh 3 bút toán (Chi phí lương, Bảo hiểm DN gánh, Khấu trừ lương & TNCN).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi ghi sổ bảng lương Id={Id}", bangLuongId);
            TempData["ErrorMessage"] = $"Lỗi ghi sổ: {ex.Message}";
        }

        return RedirectToAction(nameof(BangLuong), new { kyKeToan });
    }

    [HttpGet]
    public async Task<IActionResult> PhieuLuong(long bangLuongId, long nhanVienId)
    {
        try
        {
            var payslip = await _payrollService.LayPhieuLuongNhanVienAsync(bangLuongId, nhanVienId);
            return View(payslip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tải phiếu lương BL={BlId}, NV={NvId}", bangLuongId, nhanVienId);
            TempData["ErrorMessage"] = $"Lỗi tải phiếu lương: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
