using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller xử lý nghiệp vụ Thu Tiền Mặt (TK 1111) và Báo Có Ngân Hàng (TK 1121).
/// </summary>
public class ThuTienController : Controller
{
    private readonly IThuChiService _thuChiService;
    private readonly AppDbContext _context;
    private readonly ILogger<ThuTienController> _logger;

    public ThuTienController(
        IThuChiService thuChiService,
        AppDbContext context,
        ILogger<ThuTienController> logger)
    {
        _thuChiService = thuChiService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(LoaiChungTuThuChi? loaiChungTu, DateTime? tuNgay, DateTime? denNgay, string? tuKhoa)
    {
        // Mặc định lọc các chứng từ thu (ThuTienMat hoặc BaoCoNganHang)
        var filter = await _thuChiService.TimKiemChungTuAsync(loaiChungTu, tuNgay, denNgay, tuKhoa);
        if (!loaiChungTu.HasValue)
        {
            filter.DanhSach = filter.DanhSach
                .Where(c => c.LoaiChungTu == LoaiChungTuThuChi.ThuTienMat || c.LoaiChungTu == LoaiChungTuThuChi.BaoCoNganHang)
                .ToList();
        }
        return View(filter);
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var chungTu = await _thuChiService.LayChiTietChungTuAsync(id);
        if (chungTu == null)
        {
            return NotFound();
        }
        return View(chungTu);
    }

    [HttpGet]
    public async Task<IActionResult> Create(LoaiChungTuThuChi loai = LoaiChungTuThuChi.ThuTienMat)
    {
        await LoadDropdownsAsync(loai);

        var model = new ThuChiCreateViewModel
        {
            LoaiChungTu = loai,
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LyDo = loai == LoaiChungTuThuChi.ThuTienMat ? "Thu tiền khách hàng / doanh thu" : "Thu tiền gửi báo Có ngân hàng",
            TonQuyHienTai = await _thuChiService.TinhTonQuyKhaDungAsync(DateTime.Today)
        };

        // Khởi tạo dòng chi tiết mặc định
        var tk1111 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1111")
                     ?? await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "111");
        var tk1121 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1121")
                     ?? await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "112");
        var tk131 = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");

        model.ChiTiets.Add(new ChiTietThuChiItemViewModel
        {
            DienGiai = model.LyDo,
            TaiKhoanNoId = loai == LoaiChungTuThuChi.ThuTienMat ? tk1111.Id : tk1121.Id,
            TaiKhoanCoId = tk131.Id,
            SoTien = 0
        });

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThuChiCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(model.LoaiChungTu);
            return View(model);
        }

        try
        {
            var chungTu = await _thuChiService.TaoChungTuThuChiAsync(model);
            TempData["ThanhCong"] = $"Đã lập chứng từ thu số {chungTu.SoChungTu} thành công.";
            return RedirectToAction(nameof(Details), new { id = chungTu.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lập chứng từ thu");
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDropdownsAsync(model.LoaiChungTu);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GhiSo(long id)
    {
        try
        {
            var chungTu = await _thuChiService.GhiSoChungTuAsync(id);
            TempData["ThanhCong"] = $"Đã ghi sổ thành công chứng từ {chungTu.SoChungTu} vào Sổ cái Core GL.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi ghi sổ chứng từ {Id}", id);
            TempData["Loi"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(long id, string lyDo)
    {
        try
        {
            var chungTu = await _thuChiService.HuyChungTuAsync(id, lyDo);
            TempData["ThanhCong"] = $"Đã hủy chứng từ {chungTu.SoChungTu}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hủy chứng từ {Id}", id);
            TempData["Loi"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> SoQuy(DateTime? tuNgay, DateTime? denNgay, string loaiSo = "1111", long? taiKhoanNganHangId = null)
    {
        var start = tuNgay ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = denNgay ?? DateTime.Today;

        ViewBag.TaiKhoanNganHangs = new SelectList(await _thuChiService.LayDanhSachTaiKhoanNganHangAsync(), "Id", "TenNganHang", taiKhoanNganHangId);

        var baoCao = await _thuChiService.LayBaoCaoSoQuyAsync(loaiSo, start, end, taiKhoanNganHangId);
        return View(baoCao);
    }

    [HttpGet]
    public async Task<IActionResult> In(long id)
    {
        var chungTu = await _thuChiService.LayChiTietChungTuAsync(id);
        if (chungTu == null)
        {
            return NotFound();
        }
        return View(chungTu);
    }

    private async Task LoadDropdownsAsync(LoaiChungTuThuChi loai)
    {
        ViewBag.DoiTuongs = new SelectList(await _context.DoiTuongs.OrderBy(d => d.TenDoiTuong).ToListAsync(), "Id", "TenDoiTuong");
        ViewBag.TaiKhoanNos = new SelectList(await _context.TaiKhoans.Where(t => t.MaTaiKhoan.StartsWith("111") || t.MaTaiKhoan.StartsWith("112")).OrderBy(t => t.MaTaiKhoan).ToListAsync(), "Id", "MaTaiKhoan");
        ViewBag.TaiKhoanCos = new SelectList(await _context.TaiKhoans.Where(t => t.MaTaiKhoan != "911").OrderBy(t => t.MaTaiKhoan).ToListAsync(), "Id", "MaTaiKhoan");
        ViewBag.TaiKhoanNganHangs = new SelectList(await _context.TaiKhoanNganHangs.Where(t => t.DangHoatDong).OrderBy(t => t.TenNganHang).ToListAsync(), "Id", "TenNganHang");

        // Hóa đơn bán chưa thanh toán hết
        ViewBag.HoaDonBanHangs = new SelectList(
            await _context.HoaDonBanHangs
                .Where(h => (h.TongThanhToan - h.DaThuTien) > 0)
                .OrderByDescending(h => h.NgayHoaDon)
                .Select(h => new { h.Id, TenHienThi = $"{h.SoHoaDon} - Còn: {(h.TongThanhToan - h.DaThuTien):N0} đ" })
                .ToListAsync(),
            "Id", "TenHienThi");
    }
}
