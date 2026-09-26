using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller xử lý nghiệp vụ Chi Tiền Mặt (TK 1111) và Ủy Nhiệm Chi / Báo Nợ (TK 1121).
/// Tích hợp chốt chặn Bẫy Âm Quỹ và Bẫy 20 Triệu.
/// </summary>
public class ChiTienController : Controller
{
    private readonly IThuChiService _thuChiService;
    private readonly AppDbContext _context;
    private readonly ILogger<ChiTienController> _logger;

    public ChiTienController(
        IThuChiService thuChiService,
        AppDbContext context,
        ILogger<ChiTienController> logger)
    {
        _thuChiService = thuChiService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(LoaiChungTuThuChi? loaiChungTu, DateTime? tuNgay, DateTime? denNgay, string? tuKhoa)
    {
        var filter = await _thuChiService.TimKiemChungTuAsync(loaiChungTu, tuNgay, denNgay, tuKhoa);
        if (!loaiChungTu.HasValue)
        {
            filter.DanhSach = filter.DanhSach
                .Where(c => c.LoaiChungTu == LoaiChungTuThuChi.ChiTienMat || c.LoaiChungTu == LoaiChungTuThuChi.UyNhiemChi)
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
    public async Task<IActionResult> Create(LoaiChungTuThuChi loai = LoaiChungTuThuChi.ChiTienMat)
    {
        await LoadDropdownsAsync();

        var tonQuy = await _thuChiService.TinhTonQuyKhaDungAsync(DateTime.Today);

        var model = new ThuChiCreateViewModel
        {
            LoaiChungTu = loai,
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LyDo = loai == LoaiChungTuThuChi.ChiTienMat ? "Chi tiền trả nhà cung cấp / chi phí" : "Ủy nhiệm chi chuyển khoản thanh toán",
            TonQuyHienTai = tonQuy
        };

        var tk1111 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1111")
                     ?? await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "111");
        var tk1121 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1121")
                     ?? await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "112");
        var tk331 = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        model.ChiTiets.Add(new ChiTietThuChiItemViewModel
        {
            DienGiai = model.LyDo,
            TaiKhoanNoId = tk331.Id,
            TaiKhoanCoId = loai == LoaiChungTuThuChi.ChiTienMat ? tk1111.Id : tk1121.Id,
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
            await LoadDropdownsAsync();
            model.TonQuyHienTai = await _thuChiService.TinhTonQuyKhaDungAsync(DateTime.Today);
            return View(model);
        }

        try
        {
            var chungTu = await _thuChiService.TaoChungTuThuChiAsync(model);
            if (chungTu.ViPhamQuyTac20Tr)
            {
                TempData["CanhBao"] = "CẢNH BÁO: Chứng từ chi tiền mặt có giá trị >= 20.000.000 VNĐ hoặc thanh toán cho hóa đơn >= 20.000.000 VNĐ! Theo TT 219/2013 và TT 26/2015, khoản chi này sẽ không đủ điều kiện khấu trừ thuế GTGT và có rủi ro bị loại chi phí khi quyết toán thuế TNDN. Nên dùng Ủy Nhiệm Chi.";
            }
            else
            {
                TempData["ThanhCong"] = $"Đã lập chứng từ chi số {chungTu.SoChungTu} thành công.";
            }

            return RedirectToAction(nameof(Details), new { id = chungTu.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lập chứng từ chi");
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDropdownsAsync();
            model.TonQuyHienTai = await _thuChiService.TinhTonQuyKhaDungAsync(DateTime.Today);
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
            TempData["ThanhCong"] = $"Đã ghi sổ thành công chứng từ chi {chungTu.SoChungTu} vào Sổ cái Core GL.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi ghi sổ chứng từ chi {Id}", id);
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
            TempData["ThanhCong"] = $"Đã hủy chứng từ chi {chungTu.SoChungTu}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hủy chứng từ chi {Id}", id);
            TempData["Loi"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
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

    private async Task LoadDropdownsAsync()
    {
        ViewBag.DoiTuongs = new SelectList(await _context.DoiTuongs.OrderBy(d => d.TenDoiTuong).ToListAsync(), "Id", "TenDoiTuong");
        ViewBag.TaiKhoanNos = new SelectList(await _context.TaiKhoans.Where(t => t.MaTaiKhoan != "911").OrderBy(t => t.MaTaiKhoan).ToListAsync(), "Id", "MaTaiKhoan");
        ViewBag.TaiKhoanCos = new SelectList(await _context.TaiKhoans.Where(t => t.MaTaiKhoan.StartsWith("111") || t.MaTaiKhoan.StartsWith("112")).OrderBy(t => t.MaTaiKhoan).ToListAsync(), "Id", "MaTaiKhoan");
        ViewBag.TaiKhoanNganHangs = new SelectList(await _context.TaiKhoanNganHangs.Where(t => t.DangHoatDong).OrderBy(t => t.TenNganHang).ToListAsync(), "Id", "TenNganHang");

        // Hóa đơn mua chưa thanh toán hết
        ViewBag.HoaDonMuaHangs = new SelectList(
            await _context.HoaDonMuaHangs
                .Where(h => (h.TongThanhToan - h.DaThanhToan) > 0)
                .OrderByDescending(h => h.NgayHoaDon)
                .Select(h => new { h.Id, TenHienThi = $"{h.SoHoaDon} - Còn: {(h.TongThanhToan - h.DaThanhToan):N0} đ" })
                .ToListAsync(),
            "Id", "TenHienThi");
    }
}
