using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

public class QuyetToanThueController : Controller
{
    private readonly ITaxFinalizationService _taxFinalizationService;
    private readonly ILogger<QuyetToanThueController> _logger;

    public QuyetToanThueController(
        ITaxFinalizationService taxFinalizationService,
        ILogger<QuyetToanThueController> logger)
    {
        _taxFinalizationService = taxFinalizationService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var tndn = await _taxFinalizationService.LapQuyetToanTndnAsync(targetYear);
        var tncn = await _taxFinalizationService.LapQuyetToanTncnAsync(targetYear);

        ViewBag.Nam = targetYear;
        ViewBag.Tndn = tndn;
        ViewBag.Tncn = tncn;

        return View();
    }

    public async Task<IActionResult> Tndn03(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var tndn = await _taxFinalizationService.LapQuyetToanTndnAsync(targetYear);
        return View(tndn);
    }

    public async Task<IActionResult> Tncn05(int? nam)
    {
        int targetYear = nam ?? (DateTime.Today.Month < 4 ? DateTime.Today.Year - 1 : DateTime.Today.Year);
        var tncn = await _taxFinalizationService.LapQuyetToanTncnAsync(targetYear);
        return View(tncn);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LuuTndn(int nam)
    {
        try
        {
            var tndn = await _taxFinalizationService.LapQuyetToanTndnAsync(nam);
            await _taxFinalizationService.LuuQuyetToanTndnAsync(tndn);
            TempData["SuccessMessage"] = $"Đã lưu Tờ khai Quyết toán TNDN (Mẫu 03/TNDN) năm {nam} thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi lưu tờ khai TNDN năm {Nam}", nam);
            TempData["ErrorMessage"] = $"Lỗi lưu tờ khai: {ex.Message}";
        }

        return RedirectToAction(nameof(Tndn03), new { nam });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LuuTncn(int nam)
    {
        try
        {
            var tncn = await _taxFinalizationService.LapQuyetToanTncnAsync(nam);
            await _taxFinalizationService.LuuQuyetToanTncnAsync(tncn);
            TempData["SuccessMessage"] = $"Đã lưu Tờ khai Quyết toán TNCN (Mẫu 05/QTT-TNCN) năm {nam} thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi lưu tờ khai TNCN năm {Nam}", nam);
            TempData["ErrorMessage"] = $"Lỗi lưu tờ khai: {ex.Message}";
        }

        return RedirectToAction(nameof(Tncn05), new { nam });
    }
}
