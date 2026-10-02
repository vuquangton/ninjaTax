using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

public class BadDebtController : Controller
{
    private readonly IBadDebtProvisionService _badDebtService;
    private readonly AppDbContext _context;
    private readonly ILogger<BadDebtController> _logger;

    public BadDebtController(
        IBadDebtProvisionService badDebtService,
        AppDbContext context,
        ILogger<BadDebtController> logger)
    {
        _badDebtService = badDebtService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _context.BangTrichLapDuPhongNoPhaiThus
            .Include(b => b.ButToan)
            .OrderByDescending(b => b.NgayHachToan)
            .ThenByDescending(b => b.Id)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create(DateTime? ngayHachToan = null)
    {
        var moc = ngayHachToan ?? DateTime.Today;
        var danhSachQuaHan = await _badDebtService.LayDanhSachNoQuaHanTt48Async(moc);

        // Lấy số dư Có hiện tại của TK 2293
        var tk2293 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "2293");
        decimal soDu2293 = 0m;
        if (tk2293 != null)
        {
            var butToans = await _context.ChiTietButToans
                .Where(c => c.ButToan!.NgayHachToan <= moc.Date && c.ButToan.TrangThai == Models.Entities.TrangThaiButToan.DaGhiSo)
                .Where(c => c.TaiKhoanNoId == tk2293.Id || c.TaiKhoanCoId == tk2293.Id)
                .Select(c => new { c.TaiKhoanNoId, c.TaiKhoanCoId, c.SoTien })
                .ToListAsync();

            var tongCo = butToans.Where(b => b.TaiKhoanCoId == tk2293.Id).Sum(b => b.SoTien);
            var tongNo = butToans.Where(b => b.TaiKhoanNoId == tk2293.Id).Sum(b => b.SoTien);
            soDu2293 = Math.Max(0m, tongCo - tongNo);
        }

        ViewBag.NgayHachToan = moc;
        ViewBag.SoDu2293 = soDu2293;
        return View(danhSachQuaHan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateTime ngayHachToan, string? ghiChu)
    {
        try
        {
            var bang = await _badDebtService.TaoBangTrichLapDuPhongAsync(ngayHachToan, ghiChu);
            var (ok, msg) = await _badDebtService.GhiSoBangTrichLapAsync(bang.Id);
            if (!ok)
            {
                TempData["ThongBaoLoi"] = msg;
                return RedirectToAction(nameof(Index));
            }

            TempData["ThongBaoThanhCong"] = $"Đã lập và ghi sổ Bảng trích lập dự phòng {bang.SoChungTu} thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lập bảng trích lập dự phòng TT48");
            TempData["ThongBaoLoi"] = ex.Message;
            return RedirectToAction(nameof(Create), new { ngayHachToan });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(long id)
    {
        var (ok, msg) = await _badDebtService.HuyBangTrichLapAsync(id);
        if (!ok)
        {
            TempData["ThongBaoLoi"] = msg;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã hủy bảng trích lập dự phòng thành công!";
        }
        return RedirectToAction(nameof(Index));
    }
}
