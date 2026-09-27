using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

public class BaoCaoTaiChinhController : Controller
{
    private readonly IFinancialReportService _financialReportService;
    private readonly ILogger<BaoCaoTaiChinhController> _logger;

    public BaoCaoTaiChinhController(
        IFinancialReportService financialReportService,
        ILogger<BaoCaoTaiChinhController> logger)
    {
        _financialReportService = financialReportService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var dashboard = await _financialReportService.LayDashboardAsync(targetYear);
        return View(dashboard);
    }

    public async Task<IActionResult> B01(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var b01 = await _financialReportService.LapBaoCaoB01Async(targetYear);
        return View(b01);
    }

    public async Task<IActionResult> B02(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var b02 = await _financialReportService.LapBaoCaoB02Async(targetYear);
        return View(b02);
    }

    public async Task<IActionResult> B03(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var b03 = await _financialReportService.LapBaoCaoB03Async(targetYear);
        return View(b03);
    }

    public async Task<IActionResult> ThuyetMinh(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var thuyetMinh = await _financialReportService.LapThuyetMinhB09Async(targetYear);
        return View(thuyetMinh);
    }

    public async Task<IActionResult> BaoCaoBoPhan(int? nam, long? branchId)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var report = await _financialReportService.LapBaoCaoBoPhanAsync(targetYear, branchId);
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaSo(int nam)
    {
        try
        {
            await _financialReportService.KhoaSoBctcNamAsync(nam);
            TempData["SuccessMessage"] = $"Đã khóa sổ Báo cáo Tài chính năm {nam} thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khóa sổ BCTC năm {Nam}", nam);
            TempData["ErrorMessage"] = $"Lỗi khóa sổ: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { nam });
    }
}
