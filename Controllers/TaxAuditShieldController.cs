using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

public class TaxAuditShieldController : Controller
{
    private readonly ITaxAuditShieldService _shieldService;
    private readonly ILogger<TaxAuditShieldController> _logger;

    public TaxAuditShieldController(
        ITaxAuditShieldService shieldService,
        ILogger<TaxAuditShieldController> logger)
    {
        _shieldService = shieldService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var report = await _shieldService.QuetToanBoBayThueAsync(targetYear);
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuetLai(int nam)
    {
        try
        {
            var report = await _shieldService.QuetToanBoBayThueAsync(nam);
            TempData["SuccessMessage"] = $"Đã quét lại thành công 8 bẫy rủi ro thanh tra thuế năm {nam}! Phát hiện {report.TongSoPhatHien} điểm cần lưu ý.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi quét rủi ro thuế năm {Nam}", nam);
            TempData["ErrorMessage"] = $"Lỗi quét rủi ro thuế: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { nam });
    }
}
