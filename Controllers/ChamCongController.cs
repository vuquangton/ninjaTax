using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller Quản lý Chấm công & Phân loại Thời gian Lao động (Phase 4 Timesheet).
/// </summary>
public class ChamCongController : Controller
{
    private readonly ITimesheetService _timesheetService;
    private readonly ILogger<ChamCongController> _logger;

    public ChamCongController(ITimesheetService timesheetService, ILogger<ChamCongController> logger)
    {
        _timesheetService = timesheetService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? kyKeToan, int soNgayCongChuan = 22)
    {
        var ky = string.IsNullOrWhiteSpace(kyKeToan) ? DateTime.Today.ToString("yyyy-MM") : kyKeToan.Trim();
        var model = await _timesheetService.LayHoacTaoBangChamCongAsync(ky, soNgayCongChuan);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Luu(BangChamCongViewModel model)
    {
        try
        {
            await _timesheetService.CapNhatChiTietChamCongAsync(model.Id, model.DongChamCongs);
            TempData["SuccessMessage"] = $"Lưu dữ liệu chấm công kỳ {model.KyKeToan} thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu bảng chấm công Id={Id}", model.Id);
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { kyKeToan = model.KyKeToan });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Chot(long id, string kyKeToan)
    {
        try
        {
            await _timesheetService.ChotBangChamCongAsync(id);
            TempData["SuccessMessage"] = $"Đã chốt bảng chấm công kỳ {kyKeToan} thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi chốt bảng chấm công Id={Id}", id);
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { kyKeToan });
    }
}
