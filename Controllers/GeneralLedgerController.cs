using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller phân hệ Tổng Hợp & Sổ Cái (General Ledger) theo Thông tư 99/2025/TT-BTC.
/// Tuân thủ quy chuẩn Controller mỏng (Thin Controller):
/// Chỉ tiếp nhận HTTP request, gọi service xử lý và trả về ViewModel cho Razor Views/AG Grid.
/// </summary>
public class GeneralLedgerController : Controller
{
    private readonly IGeneralLedgerService _glService;
    private readonly IPeriodClosingService _periodClosingService;
    private readonly ISubledgerReconciliationService _reconciliationService;
    private readonly ILogger<GeneralLedgerController> _logger;

    public GeneralLedgerController(
        IGeneralLedgerService glService,
        IPeriodClosingService periodClosingService,
        ISubledgerReconciliationService reconciliationService,
        ILogger<GeneralLedgerController> logger)
    {
        _glService = glService;
        _periodClosingService = periodClosingService;
        _reconciliationService = reconciliationService;
        _logger = logger;
    }

    /// <summary>
    /// Sổ Nhật ký chung (Mẫu S03a-DN)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
    {
        var start = tuNgay ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = denNgay ?? DateTime.Today;

        var model = await _glService.LaySoNhatKyChungAsync(start, end);
        return View(model);
    }

    /// <summary>
    /// Sổ Cái một tài khoản kế toán (Mẫu S03b-DN)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SoCai(string? maTaiKhoan, DateTime? tuNgay, DateTime? denNgay)
    {
        var start = tuNgay ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = denNgay ?? DateTime.Today;
        var tk = string.IsNullOrWhiteSpace(maTaiKhoan) ? "1111" : maTaiKhoan.Trim();

        var model = await _glService.LaySoCaiAsync(tk, start, end);
        return View(model);
    }

    /// <summary>
    /// Bảng Cân đối số phát sinh các tài khoản (Trial Balance 8 cột)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BangCanDoi(DateTime? tuNgay, DateTime? denNgay)
    {
        var start = tuNgay ?? new DateTime(DateTime.Today.Year, 1, 1);
        var end = denNgay ?? DateTime.Today;

        var model = await _glService.LayBangCanDoiTaiKhoanAsync(start, end);
        return View(model);
    }

    /// <summary>
    /// Màn hình Kết chuyển tự động cuối kỳ (Period Closing)
    /// </summary>
    [HttpGet]
    public IActionResult KetChuyenCuoiKy(int? nam, int? thang)
    {
        ViewBag.Nam = nam ?? DateTime.Today.Year;
        ViewBag.Thang = thang ?? DateTime.Today.Month;
        return View();
    }

    /// <summary>
    /// Thực hiện kết chuyển cuối kỳ (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThucHienKetChuyen(int nam, int? thang)
    {
        _logger.LogInformation("Thực hiện kết chuyển cuối kỳ: Năm={Nam}, Tháng={Thang}", nam, thang);
        var result = await _periodClosingService.TaoButToanKetChuyenAsync(nam, thang);

        if (!result.ThanhCong)
        {
            TempData["ThongBaoLoi"] = result.ThongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = result.ThongBao;
        }

        return RedirectToAction(nameof(KetChuyenCuoiKy), new { nam, thang });
    }

    /// <summary>
    /// Màn hình Đối soát Sổ phụ (Subledger) vs Sổ Cái (General Ledger)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DoiSoat(DateTime? mocThoiGian)
    {
        var moc = mocThoiGian ?? DateTime.Today;
        var model = await _reconciliationService.KiemTraDoiSoatToanHeThongAsync(moc);
        return View(model);
    }
}
