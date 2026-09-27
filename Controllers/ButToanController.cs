using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller xử lý giao diện Sổ Nhật ký chung và Chứng từ Bút toán kế toán.
/// Tuân thủ quy chuẩn Controller mỏng (Thin Controller):
/// Chỉ điều phối HTTP request/response, ủy quyền toàn bộ nghiệp vụ kiểm tra kép và xử lý số liệu cho IButToanService.
/// </summary>
public class ButToanController : Controller
{
    private readonly IButToanService _butToanService;
    private readonly ILogger<ButToanController> _logger;

    public ButToanController(IButToanService butToanService, ILogger<ButToanController> logger)
    {
        _butToanService = butToanService;
        _logger = logger;
    }

    /// <summary>
    /// Hiển thị danh sách sổ Nhật ký chung
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Truy cập danh sách Sổ Nhật ký chung");
        var danhSach = await _butToanService.LayDanhSachAsync();

        var viewModel = new ButToanIndexViewModel
        {
            DanhSachButToan = danhSach,
            TongSoChungTu = danhSach.Count,
            TongPhatSinh = danhSach.Sum(b => b.TongTien)
        };

        return View(viewModel);
    }

    /// <summary>
    /// API trả về danh sách bút toán phân trang, sắp xếp dạng JSON cho AG Grid
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListJson(int page = 1, int pageSize = 50, string? sort = null, string? dir = null)
    {
        var danhSach = await _butToanService.LayDanhSachAsync();
        var query = danhSach.AsQueryable();

        if (!string.IsNullOrWhiteSpace(sort))
        {
            bool isDesc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sort.ToLowerInvariant() switch
            {
                "sochungtu" => isDesc ? query.OrderByDescending(b => b.SoChungTu) : query.OrderBy(b => b.SoChungTu),
                "ngayhachtoan" => isDesc ? query.OrderByDescending(b => b.NgayHachToan) : query.OrderBy(b => b.NgayHachToan),
                "tongtien" => isDesc ? query.OrderByDescending(b => b.TongTien) : query.OrderBy(b => b.TongTien),
                _ => query
            };
        }

        var totalCount = query.Count();
        var rows = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                id = b.Id,
                soChungTu = b.SoChungTu,
                ngayHachToan = b.NgayHachToan.ToString("yyyy-MM-dd"),
                dienGiai = b.DienGiai,
                tongTien = b.TongTien,
                trangThai = (int)b.TrangThai,
                trangThaiText = b.TrangThai == TrangThaiButToan.DaGhiSo ? "Đã ghi sổ" : "Chưa ghi sổ"
            })
            .ToList();

        return Json(new { rows, totalCount });
    }

    /// <summary>
    /// Hiển thị chi tiết chứng từ bút toán
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        _logger.LogInformation("Xem chi tiết bút toán Id: {Id}", id);
        var butToan = await _butToanService.LayTheoIdAsync(id);

        if (butToan == null)
        {
            _logger.LogWarning("Không tìm thấy bút toán Id: {Id}", id);
            return NotFound();
        }

        return View(butToan);
    }

    /// <summary>
    /// Hiển thị form lập chứng từ bút toán mới
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation("Mở màn hình lập chứng từ bút toán mới");
        var viewModel = new ButToanCreateViewModel();
        await NapDanhSachChonAsync(viewModel);
        return View(viewModel);
    }

    /// <summary>
    /// Tiếp nhận và lưu chứng từ bút toán mới
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ButToanCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await NapDanhSachChonAsync(model);
            return View(model);
        }

        // Chuyển đổi ViewModel sang Entity
        var butToan = new ButToan
        {
            SoChungTu = model.SoChungTu ?? string.Empty,
            NgayHachToan = model.NgayHachToan,
            NgayChungTu = model.NgayChungTu,
            SoChungTuGoc = model.SoChungTuGoc,
            NgayChungTuGoc = model.NgayChungTuGoc,
            DienGiai = model.DienGiai,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = model.ChiTiets.Select((c, index) => new ChiTietButToan
            {
                DongSo = index + 1,
                TaiKhoanNoId = c.TaiKhoanNoId,
                TaiKhoanCoId = c.TaiKhoanCoId,
                SoTien = c.SoTien,
                DienGiai = string.IsNullOrWhiteSpace(c.DienGiai) ? model.DienGiai : c.DienGiai,
                DoiTuongId = c.DoiTuongId > 0 ? c.DoiTuongId : null
            }).ToList()
        };

        var (thanhCong, thongBao, ketQua) = await _butToanService.TaoMoiAsync(butToan);

        if (!thanhCong)
        {
            ModelState.AddModelError(string.Empty, thongBao ?? "Lỗi khi lưu bút toán.");
            await NapDanhSachChonAsync(model);
            return View(model);
        }

        TempData["ThongBaoThanhCong"] = $"Đã tạo thành công chứng từ {ketQua?.SoChungTu}!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Ghi sổ chứng từ
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GhiSo(long id)
    {
        var (thanhCong, thongBao) = await _butToanService.GhiSoAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã ghi sổ chứng từ thành công.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Bỏ ghi sổ chứng từ
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BoGhiSo(long id)
    {
        var (thanhCong, thongBao) = await _butToanService.BoGhiSoAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã bỏ ghi sổ chứng từ.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Xóa chứng từ (chỉ áp dụng với chứng từ chưa ghi sổ)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var (thanhCong, thongBao) = await _butToanService.XoaAsync(id);
        if (!thanhCong)
        {
            TempData["ThongBaoLoi"] = thongBao;
        }
        else
        {
            TempData["ThongBaoThanhCong"] = "Đã xóa chứng từ thành công.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task NapDanhSachChonAsync(ButToanCreateViewModel model)
    {
        var (taiKhoans, doiTuongs) = await _butToanService.LayDanhMucTaoButToanAsync();

        model.DanhSachTaiKhoan = taiKhoans
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.MaTaiKhoan} - {t.TenTaiKhoan}"
            })
            .ToList();

        model.DanhSachDoiTuong = doiTuongs
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.MaDoiTuong} - {d.TenDoiTuong}"
            })
            .ToList();
    }
}
