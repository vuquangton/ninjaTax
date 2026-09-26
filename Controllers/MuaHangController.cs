using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller xử lý phân hệ Mua hàng, tiếp nhận HĐĐT đầu vào và tự động ghi sổ kho/GL
/// </summary>
public class MuaHangController : Controller
{
    private readonly IHachToanMuaHangService _muaHangService;
    private readonly AppDbContext _context;
    private readonly ILogger<MuaHangController> _logger;

    public MuaHangController(
        IHachToanMuaHangService muaHangService,
        AppDbContext context,
        ILogger<MuaHangController> logger)
    {
        _muaHangService = muaHangService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Xem danh sách hóa đơn mua hàng");
        var danhSach = await _muaHangService.LayDanhSachAsync();

        var viewModel = new HoaDonMuaIndexViewModel
        {
            DanhSachHoaDon = danhSach,
            TongGiaTriMua = danhSach.Sum(h => h.TongThanhToan),
            TongConPhaiTra = danhSach.Sum(h => h.ConPhaiTra)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var hoaDon = await _muaHangService.LayTheoIdAsync(id);
        if (hoaDon == null)
        {
            return NotFound();
        }

        return View(hoaDon);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new HoaDonMuaCreateViewModel();
        await NapDuLieuChonAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HoaDonMuaCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await NapDuLieuChonAsync(model);
            return View(model);
        }

        var ncc = await _context.DoiTuongs.FindAsync(model.NhaCungCapId);

        var hoaDon = new HoaDonMuaHang
        {
            SoChungTu = model.SoChungTu ?? string.Empty,
            NgayHachToan = model.NgayHachToan,
            NgayChungTu = model.NgayChungTu,
            KHMauSoHoaDon = model.KHMauSoHoaDon,
            KyHieuHoaDon = model.KyHieuHoaDon,
            SoHoaDon = model.SoHoaDon,
            NgayHoaDon = model.NgayHoaDon,
            NhaCungCapId = model.NhaCungCapId,
            TenNCC = ncc?.TenDoiTuong,
            MaSoThueNCC = ncc?.MaSoThue,
            DiaChiNCC = ncc?.DiaChi,
            DienGiai = model.DienGiai,
            HanThanhToan = model.HanThanhToan,
            MuaHangKiemKho = model.MuaHangKiemKho,
            ChiTietHangs = model.ChiTiets.Select((c, idx) => new ChiTietHoaDonMua
            {
                DongSo = idx + 1,
                VatTuHangHoaId = c.VatTuHangHoaId,
                SoLuong = c.SoLuong,
                DonGia = c.DonGia,
                ThanhTien = c.SoLuong * c.DonGia,
                TiLeChietKhau = c.TiLeChietKhau,
                ThueSuatVat = c.ThueSuatVat,
                TaiKhoanNoId = c.TaiKhoanNoId,
                TaiKhoanThueId = c.TaiKhoanThueId,
                TaiKhoanCoId = c.TaiKhoanCoId
            }).ToList()
        };

        var (thanhCong, thongBao, ketQua) = await _muaHangService.TaoMoiAsync(hoaDon);
        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi khi lưu hóa đơn mua.");
            await NapDuLieuChonAsync(model);
            return View(model);
        }

        TempData["ThongBaoThanhCong"] = $"Đã lưu thành công hóa đơn mua {ketQua?.SoChungTu}!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GhiSo(long id)
    {
        var (thanhCong, thongBao, butToan) = await _muaHangService.GhiSoAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = $"Đã tự động ghi sổ kế toán thành công (Bút toán {butToan?.SoChungTu})!";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BoGhiSo(long id)
    {
        var (thanhCong, thongBao) = await _muaHangService.BoGhiSoAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã bỏ ghi sổ hóa đơn mua.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var (thanhCong, thongBao) = await _muaHangService.XoaAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã xóa hóa đơn mua thành công.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task NapDuLieuChonAsync(HoaDonMuaCreateViewModel model)
    {
        model.DanhSachNhaCungCap = await _context.DoiTuongs
            .Where(d => d.DangHoatDong && (d.Loai == LoaiDoiTuong.NhaCungCap || d.Loai == LoaiDoiTuong.Khac))
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
            .Where(t => t.MaTaiKhoan.StartsWith("152") || t.MaTaiKhoan.StartsWith("156") || t.MaTaiKhoan.StartsWith("642"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();

        model.DanhSachTaiKhoanCo = accounts
            .Where(t => t.MaTaiKhoan.StartsWith("331") || t.MaTaiKhoan.StartsWith("111") || t.MaTaiKhoan.StartsWith("112"))
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}" })
            .ToList();
    }
}
