using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller xử lý công nợ chi tiết (AR/AP Subledger), theo dõi tuổi nợ (Aging) và đối trừ chứng từ
/// </summary>
public class CongNoController : Controller
{
    private readonly ICongNoService _congNoService;
    private readonly AppDbContext _context;
    private readonly ILogger<CongNoController> _logger;

    public CongNoController(
        ICongNoService congNoService,
        AppDbContext context,
        ILogger<CongNoController> logger)
    {
        _congNoService = congNoService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? mocThoiGian)
    {
        var moc = mocThoiGian ?? DateTime.Today;
        _logger.LogInformation("Truy vấn báo cáo công nợ ngày: {Date}", moc);

        var arAging = await _congNoService.BaoCaoTuoiNoPhaiThuAsync(moc);
        var apAging = await _congNoService.BaoCaoTuoiNoPhaiTraAsync(moc);

        var model = new CongNoIndexViewModel
        {
            MocThoiGian = moc,
            BaoCaoTuoiNoPhaiThu = arAging,
            BaoCaoTuoiNoPhaiTra = apAging
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DoiTru()
    {
        var model = new DoiTruCongNoCreateViewModel();
        await NapDoiTuongChonAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTru(DoiTruCongNoCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await NapDoiTuongChonAsync(model);
            return View(model);
        }

        if (model.Loai == LoaiCongNo.PhaiThuKhachHang)
        {
            if (model.HoaDonId.HasValue && model.HoaDonId.Value > 0)
            {
                var (ok, msg) = await _congNoService.DoiTruHoaDonBanAsync(model.HoaDonId.Value, model.SoTien, null, model.GhiChu);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, msg ?? "Lỗi khi đối trừ hóa đơn bán.");
                    await NapDoiTuongChonAsync(model);
                    return View(model);
                }
            }
            else
            {
                var (ok, msg, daDoiTru) = await _congNoService.DoiTruFifoKhachHangAsync(model.DoiTuongId, model.SoTien);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, msg ?? "Lỗi khi đối trừ FIFO.");
                    await NapDoiTuongChonAsync(model);
                    return View(model);
                }
                TempData["ThongBaoThanhCong"] = $"Đã đối trừ FIFO số tiền {daDoiTru:N0} VNĐ cho khách hàng!";
                return RedirectToAction(nameof(Index));
            }
        }
        else
        {
            if (model.HoaDonId.HasValue && model.HoaDonId.Value > 0)
            {
                var (ok, msg) = await _congNoService.DoiTruHoaDonMuaAsync(model.HoaDonId.Value, model.SoTien, null, model.GhiChu);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, msg ?? "Lỗi khi đối trừ hóa đơn mua.");
                    await NapDoiTuongChonAsync(model);
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn hóa đơn mua cụ thể để thanh toán.");
                await NapDoiTuongChonAsync(model);
                return View(model);
            }
        }

        TempData["ThongBaoThanhCong"] = "Ghi nhận thanh toán và đối trừ công nợ thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetHoaDonChuaThanhToan(LoaiCongNo loai, long doiTuongId)
    {
        if (loai == LoaiCongNo.PhaiThuKhachHang)
        {
            var hds = await _context.HoaDonBanHangs
                .Where(h => h.KhachHangId == doiTuongId && (h.TongThanhToan - h.DaThuTien) > 0 && h.TrangThai != TrangThaiHddt.DaHuy)
                .OrderBy(h => h.NgayHoaDon)
                .Select(h => new
                {
                    id = h.Id,
                    soChungTu = h.SoChungTu,
                    soHoaDon = h.SoHoaDon ?? "Chưa cấp số",
                    ngay = h.NgayHoaDon.ToString("dd/MM/yyyy"),
                    tongTien = h.TongThanhToan,
                    conNo = h.TongThanhToan - h.DaThuTien
                })
                .ToListAsync();
            return Json(hds);
        }
        else
        {
            var hds = await _context.HoaDonMuaHangs
                .Where(h => h.NhaCungCapId == doiTuongId && (h.TongThanhToan - h.DaThanhToan) > 0)
                .OrderBy(h => h.NgayHoaDon)
                .Select(h => new
                {
                    id = h.Id,
                    soChungTu = h.SoChungTu,
                    soHoaDon = h.SoHoaDon ?? "Chưa có",
                    ngay = h.NgayHoaDon.ToString("dd/MM/yyyy"),
                    tongTien = h.TongThanhToan,
                    conNo = h.TongThanhToan - h.DaThanhToan
                })
                .ToListAsync();
            return Json(hds);
        }
    }

    private async Task NapDoiTuongChonAsync(DoiTruCongNoCreateViewModel model)
    {
        var loaiDoiTuong = model.Loai == LoaiCongNo.PhaiThuKhachHang ? LoaiDoiTuong.KhachHang : LoaiDoiTuong.NhaCungCap;
        model.DanhSachDoiTuong = await _context.DoiTuongs
            .Where(d => d.DangHoatDong && (d.Loai == loaiDoiTuong || d.Loai == LoaiDoiTuong.Khac))
            .OrderBy(d => d.MaDoiTuong)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = $"{d.MaDoiTuong} - {d.TenDoiTuong}" })
            .ToListAsync();
    }
}
