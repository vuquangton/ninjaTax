using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller xử lý phân hệ Bán hàng, phát hành HĐĐT (NĐ 123/2020) và tự động sinh bút toán kép Doanh thu & Giá vốn (TT99)
/// </summary>
public class BanHangController : Controller
{
    private readonly IHachToanBanHangService _banHangService;
    private readonly AppDbContext _context;
    private readonly ILogger<BanHangController> _logger;

    public BanHangController(
        IHachToanBanHangService banHangService,
        AppDbContext context,
        ILogger<BanHangController> logger)
    {
        _banHangService = banHangService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Xem danh sách hóa đơn bán hàng");
        var danhSach = await _banHangService.LayDanhSachAsync();

        var viewModel = new HoaDonBanIndexViewModel
        {
            DanhSachHoaDon = danhSach,
            TongDoanhThuBan = danhSach.Sum(h => h.TongThanhToan),
            TongConPhaiThu = danhSach.Sum(h => h.ConPhaiThu)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var hoaDon = await _banHangService.LayTheoIdAsync(id);
        if (hoaDon == null)
        {
            return NotFound();
        }

        return View(hoaDon);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new HoaDonBanCreateViewModel();
        await NapDuLieuChonAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HoaDonBanCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await NapDuLieuChonAsync(model);
            return View(model);
        }

        var khachHang = await _context.DoiTuongs.FindAsync(model.KhachHangId);

        var hoaDon = new HoaDonBanHang
        {
            SoChungTu = model.SoChungTu ?? string.Empty,
            NgayHachToan = model.NgayHachToan,
            NgayChungTu = model.NgayChungTu,
            KHMauSo = model.KHMauSo,
            KyHieu = model.KyHieu,
            SoHoaDon = model.SoHoaDon ?? string.Empty,
            NgayHoaDon = model.NgayHoaDon,
            KhachHangId = model.KhachHangId,
            TenKhachHang = khachHang?.TenDoiTuong,
            MaSoThueKH = khachHang?.MaSoThue,
            DiaChiKH = khachHang?.DiaChi,
            DienGiai = model.DienGiai,
            HanThanhToan = model.HanThanhToan,
            BanHangKiemXuatKho = model.BanHangKiemXuatKho,
            ChiTietBans = model.ChiTiets.Select((c, idx) => new ChiTietHoaDonBan
            {
                DongSo = idx + 1,
                VatTuHangHoaId = c.VatTuHangHoaId,
                SoLuong = c.SoLuong,
                DonGia = c.DonGia,
                ThanhTien = c.SoLuong * c.DonGia,
                TiLeChietKhau = c.TiLeChietKhau,
                ThueSuatVat = c.ThueSuatVat,
                DonGiaVon = c.DonGiaVon,
                TaiKhoanNoId = c.TaiKhoanNoId,
                TaiKhoanDoanhThuId = c.TaiKhoanDoanhThuId,
                TaiKhoanThueId = c.TaiKhoanThueId,
                TaiKhoanGiaVonId = c.TaiKhoanGiaVonId,
                TaiKhoanKhoId = c.TaiKhoanKhoId
            }).ToList()
        };

        var (thanhCong, thongBao, ketQua) = await _banHangService.TaoMoiAsync(hoaDon);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi khi lưu hóa đơn bán hàng.");
            await NapDuLieuChonAsync(model);
            return View(model);
        }

        TempData["ThongBaoThanhCong"] = $"Đã lưu thành công hóa đơn bán {ketQua?.SoChungTu}!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PhatHanhHddt(long id)
    {
        var (thanhCong, thongBao) = await _banHangService.PhatHanhHddtAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Phát hành hóa đơn điện tử thành công (Đã cấp Mã CQT & Ký số)!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GhiSo(long id)
    {
        var (thanhCong, thongBao, doanhThu, giaVon) = await _banHangService.GhiSoAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            string thongBaoGhiSo = $"Đã ghi sổ thành công Bút toán doanh thu ({doanhThu?.SoChungTu})";
            if (giaVon != null)
            {
                thongBaoGhiSo += $" và Bút toán giá vốn ({giaVon.SoChungTu})";
            }
            TempData["ThongBaoThanhCong"] = thongBaoGhiSo + "!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BoGhiSo(long id)
    {
        var (thanhCong, thongBao) = await _banHangService.BoGhiSoAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã bỏ ghi sổ hóa đơn bán thành công.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyHoaDon(long id, string lyDo)
    {
        var (thanhCong, thongBao) = await _banHangService.HuyHoaDonAsync(id, lyDo);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã hủy hóa đơn điện tử thành công.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task NapDuLieuChonAsync(HoaDonBanCreateViewModel model)
    {
        model.DanhSachKhachHang = await _context.DoiTuongs
            .Where(d => d.DangHoatDong && (d.Loai == LoaiDoiTuong.KhachHang || d.Loai == LoaiDoiTuong.Khac))
            .OrderBy(d => d.MaDoiTuong)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = $"{d.MaDoiTuong} - {d.TenDoiTuong}" })
            .ToListAsync();

        model.DanhSachVatTu = await _context.VatTuHangHoas
            .Where(v => v.DangHoatDong)
            .OrderBy(v => v.MaVatTu)
            .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.MaVatTu} - {v.TenVatTu} ({v.DonViTinh})" })
            .ToListAsync();

        var accounts = await _context.TaiKhoans
            .Where(t => t.DangHoatDong && !t.MaTaiKhoan.StartsWith("911") && !t.LaTaiKhoanSoCai)
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();

        model.DanhSachTaiKhoanNo = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("131") || t.MaTaiKhoan.StartsWith("111") || t.MaTaiKhoan.StartsWith("112"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanDoanhThu = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("511") || t.MaTaiKhoan.StartsWith("711"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();
    }
}
