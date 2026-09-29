using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

public class LandedCostController : Controller
{
    private readonly ILandedCostService _landedCostService;
    private readonly AppDbContext _context;
    private readonly ILogger<LandedCostController> _logger;

    public LandedCostController(
        ILandedCostService landedCostService, 
        AppDbContext context, 
        ILogger<LandedCostController> logger)
    {
        _landedCostService = landedCostService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _landedCostService.GetAllAsync();
        var vm = new LandedCostIndexViewModel
        {
            DanhSachChiPhi = list,
            TongChiPhiChuaPhanBo = list.Where(c => !c.DaPhanBo).Sum(c => c.TongChiPhi),
            TongChiPhiDaPhanBo = list.Where(c => c.DaPhanBo).Sum(c => c.TongChiPhi)
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var nccList = await _context.DoiTuongs
            .Where(d => d.DangHoatDong && d.Loai == LoaiDoiTuong.NhaCungCap)
            .OrderBy(d => d.TenDoiTuong)
            .ToListAsync();

        ViewBag.NhaCungCapList = new SelectList(nccList, "Id", "TenDoiTuong");

        var model = new LandedCostCreateViewModel
        {
            SoChungTu = $"CPMH-{DateTime.Now:yyyyMMddHHmm}",
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LandedCostCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var nccList = await _context.DoiTuongs
                .Where(d => d.DangHoatDong && d.Loai == LoaiDoiTuong.NhaCungCap)
                .OrderBy(d => d.TenDoiTuong)
                .ToListAsync();
            ViewBag.NhaCungCapList = new SelectList(nccList, "Id", "TenDoiTuong", model.NhaCungCapDichVuId);
            return View(model);
        }

        var branch = await _context.ChiNhanhs.FirstOrDefaultAsync() ?? new ChiNhanh { Id = 1 };

        decimal vatAmount = Math.Round(model.TongChiPhi * (model.ThueSuatVat / 100m), 4);
        var chungTu = new ChungTuChiPhiMuaHang
        {
            ChiNhanhId = branch.Id,
            SoChungTu = model.SoChungTu.Trim(),
            NgayChungTu = model.NgayChungTu,
            NgayHachToan = model.NgayHachToan,
            NhaCungCapDichVuId = model.NhaCungCapDichVuId,
            DienGiai = model.DienGiai.Trim(),
            TongChiPhi = model.TongChiPhi,
            ThueSuatVat = model.ThueSuatVat,
            TienThueVat = vatAmount,
            TongThanhToan = model.TongChiPhi + vatAmount,
            PhuongThucPhanBo = model.PhuongThucPhanBo
        };

        await _landedCostService.CreateAsync(chungTu);
        TempData["SuccessMessage"] = $"Đã tạo chứng từ chi phí mua hàng {chungTu.SoChungTu}. Vui lòng thực hiện phân bổ.";
        return RedirectToAction(nameof(Allocate), new { id = chungTu.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Allocate(long id)
    {
        var chungTu = await _landedCostService.GetByIdAsync(id);
        if (chungTu == null)
        {
            return NotFound();
        }

        // Lấy danh sách tài khoản kho (152, 1561) và tài khoản công nợ/tiền (331, 111, 112)
        var tkKhoList = await _context.TaiKhoans
            .Where(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "152")
            .ToListAsync();

        var tkDoiUngList = await _context.TaiKhoans
            .Where(t => t.MaTaiKhoan == "331" || t.MaTaiKhoan == "1111" || t.MaTaiKhoan == "1121")
            .ToListAsync();

        ViewBag.TaiKhoanChiPhiList = new SelectList(tkKhoList, "Id", "TenTaiKhoan");
        ViewBag.TaiKhoanDoiUngList = new SelectList(tkDoiUngList, "Id", "TenTaiKhoan");

        // Lấy 50 dòng nhập kho gần nhất đã ghi sổ
        var recentLines = await _context.ChiTietNhapKhos
            .Include(c => c.PhieuNhapKho)
            .Include(c => c.VatTuHangHoa)
            .Where(c => c.PhieuNhapKho != null && c.PhieuNhapKho.TrangThai == TrangThaiPhieuKho.DaGhiSo)
            .OrderByDescending(c => c.PhieuNhapKho!.NgayHachToan)
            .Take(50)
            .ToListAsync();

        var vm = new LandedCostAllocateViewModel
        {
            ChungTuChiPhiId = chungTu.Id,
            ChungTuChiPhi = chungTu,
            PhuongThucPhanBo = chungTu.PhuongThucPhanBo,
            TaiKhoanChiPhiId = tkKhoList.FirstOrDefault()?.Id ?? 0,
            TaiKhoanDoiUngId = tkDoiUngList.FirstOrDefault()?.Id ?? 0,
            AvailableChiTietNhapKhos = recentLines
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Allocate(LandedCostAllocateViewModel model)
    {
        if (model.SelectedChiTietNhapKhoIds == null || model.SelectedChiTietNhapKhoIds.Count == 0)
        {
            ModelState.AddModelError("", "Vui lòng tick chọn ít nhất một dòng nhập kho để phân bổ chi phí.");
            return await Allocate(model.ChungTuChiPhiId);
        }

        var result = await _landedCostService.AllocateCostAsync(
            model.ChungTuChiPhiId,
            model.SelectedChiTietNhapKhoIds,
            model.PhuongThucPhanBo,
            model.TaiKhoanChiPhiId,
            model.TaiKhoanDoiUngId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return await Allocate(model.ChungTuChiPhiId);
        }

        TempData["SuccessMessage"] = "Đã phân bổ chi phí mua hàng vào nguyên giá nhập kho thành công.";
        return RedirectToAction(nameof(Details), new { id = model.ChungTuChiPhiId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var chungTu = await _landedCostService.GetByIdAsync(id);
        if (chungTu == null)
        {
            return NotFound();
        }

        return View(chungTu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        var result = await _landedCostService.CancelAllocationAsync(id);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Đã hủy phân bổ chi phí mua hàng và hoàn tác nguyên giá nhập kho.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
