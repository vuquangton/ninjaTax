using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

/// <summary>
/// Controller Quản lý Hồ sơ Nhân sự & Hợp đồng Lao động (Phase 4 Payroll).
/// </summary>
public class NhanVienController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<NhanVienController> _logger;

    public NhanVienController(AppDbContext context, ILogger<NhanVienController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? phongBan, LoaiHopDongLaoDong? loaiHopDong, string? tuKhoa)
    {
        var query = _context.NhanViens.AsQueryable();

        if (!string.IsNullOrWhiteSpace(phongBan))
        {
            query = query.Where(n => n.PhongBan == phongBan);
        }

        if (loaiHopDong.HasValue)
        {
            query = query.Where(n => n.LoaiHopDong == loaiHopDong.Value);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var kw = tuKhoa.Trim().ToLower();
            query = query.Where(n => n.MaNhanVien.ToLower().Contains(kw) ||
                                     n.HoTen.ToLower().Contains(kw) ||
                                     (n.MaSoThue != null && n.MaSoThue.Contains(kw)) ||
                                     (n.SoCccd != null && n.SoCccd.Contains(kw)));
        }

        var danhSach = await query
            .OrderBy(n => n.MaNhanVien)
            .Select(n => new NhanVienItemViewModel
            {
                Id = n.Id,
                MaNhanVien = n.MaNhanVien,
                HoTen = n.HoTen,
                PhongBan = n.PhongBan,
                ChucVu = n.ChucVu,
                LoaiHopDong = n.LoaiHopDong,
                LuongCoBan = n.LuongCoBan,
                LuongDongBaoHiem = n.LuongDongBaoHiem,
                SoNguoiPhuThuoc = n.SoNguoiPhuThuoc,
                DongBaoHiem = n.DongBaoHiem,
                CoCamKet08 = n.CoCamKet08,
                DangLamViec = n.DangLamViec
            })
            .ToListAsync();

        var phongBans = await _context.NhanViens
            .Where(n => !string.IsNullOrEmpty(n.PhongBan))
            .Select(n => n.PhongBan!)
            .Distinct()
            .ToListAsync();

        var model = new NhanVienIndexViewModel
        {
            DanhSachNhanVien = danhSach,
            PhongBans = phongBans,
            PhongBan = phongBan,
            LoaiHopDong = loaiHopDong,
            TuKhoa = tuKhoa
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new NhanVienCreateEditViewModel
        {
            LuongCoBan = 10_000_000m,
            LuongDongBaoHiem = 10_000_000m,
            PhuCapAnTrua = 730_000m,
            DongBaoHiem = true,
            DangLamViec = true
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NhanVienCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tonTaiMa = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == model.MaNhanVien.Trim());
        if (tonTaiMa)
        {
            ModelState.AddModelError(nameof(model.MaNhanVien), $"Mã nhân viên '{model.MaNhanVien}' đã tồn tại!");
            return View(model);
        }

        var nv = new NhanVien
        {
            MaNhanVien = model.MaNhanVien.Trim().ToUpper(),
            HoTen = model.HoTen.Trim(),
            SoCccd = model.SoCccd?.Trim(),
            MaSoThue = model.MaSoThue?.Trim(),
            SoSoBhxh = model.SoSoBhxh?.Trim(),
            PhongBan = model.PhongBan?.Trim(),
            ChucVu = model.ChucVu?.Trim(),
            LoaiHopDong = model.LoaiHopDong,
            LuongCoBan = model.LuongCoBan,
            LuongDongBaoHiem = model.LuongDongBaoHiem,
            PhuCapAnTrua = model.PhuCapAnTrua,
            PhuCapTrachNhiem = model.PhuCapTrachNhiem,
            PhuCapDienThoai = model.PhuCapDienThoai,
            PhuCapTrangPhuc = model.PhuCapTrangPhuc,
            SoNguoiPhuThuoc = model.SoNguoiPhuThuoc,
            CoCamKet08 = model.CoCamKet08,
            DongBaoHiem = model.DongBaoHiem,
            LaDoanVienCongDoan = model.LaDoanVienCongDoan,
            SoTaiKhoanNganHang = model.SoTaiKhoanNganHang?.Trim(),
            TenNganHang = model.TenNganHang?.Trim(),
            DangLamViec = model.DangLamViec,
            NgayTao = DateTime.UtcNow
        };

        await _context.NhanViens.AddAsync(nv);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Thêm mới nhân viên '{nv.MaNhanVien} - {nv.HoTen}' thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var nv = await _context.NhanViens.FindAsync(id);
        if (nv == null)
        {
            return NotFound();
        }

        var model = new NhanVienCreateEditViewModel
        {
            Id = nv.Id,
            MaNhanVien = nv.MaNhanVien,
            HoTen = nv.HoTen,
            SoCccd = nv.SoCccd,
            MaSoThue = nv.MaSoThue,
            SoSoBhxh = nv.SoSoBhxh,
            PhongBan = nv.PhongBan,
            ChucVu = nv.ChucVu,
            LoaiHopDong = nv.LoaiHopDong,
            LuongCoBan = nv.LuongCoBan,
            LuongDongBaoHiem = nv.LuongDongBaoHiem,
            PhuCapAnTrua = nv.PhuCapAnTrua,
            PhuCapTrachNhiem = nv.PhuCapTrachNhiem,
            PhuCapDienThoai = nv.PhuCapDienThoai,
            PhuCapTrangPhuc = nv.PhuCapTrangPhuc,
            SoNguoiPhuThuoc = nv.SoNguoiPhuThuoc,
            CoCamKet08 = nv.CoCamKet08,
            DongBaoHiem = nv.DongBaoHiem,
            LaDoanVienCongDoan = nv.LaDoanVienCongDoan,
            SoTaiKhoanNganHang = nv.SoTaiKhoanNganHang,
            TenNganHang = nv.TenNganHang,
            DangLamViec = nv.DangLamViec
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, NhanVienCreateEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var nv = await _context.NhanViens.FindAsync(id);
        if (nv == null)
        {
            return NotFound();
        }

        var tonTaiMa = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == model.MaNhanVien.Trim() && n.Id != id);
        if (tonTaiMa)
        {
            ModelState.AddModelError(nameof(model.MaNhanVien), $"Mã nhân viên '{model.MaNhanVien}' đã tồn tại!");
            return View(model);
        }

        nv.MaNhanVien = model.MaNhanVien.Trim().ToUpper();
        nv.HoTen = model.HoTen.Trim();
        nv.SoCccd = model.SoCccd?.Trim();
        nv.MaSoThue = model.MaSoThue?.Trim();
        nv.SoSoBhxh = model.SoSoBhxh?.Trim();
        nv.PhongBan = model.PhongBan?.Trim();
        nv.ChucVu = model.ChucVu?.Trim();
        nv.LoaiHopDong = model.LoaiHopDong;
        nv.LuongCoBan = model.LuongCoBan;
        nv.LuongDongBaoHiem = model.LuongDongBaoHiem;
        nv.PhuCapAnTrua = model.PhuCapAnTrua;
        nv.PhuCapTrachNhiem = model.PhuCapTrachNhiem;
        nv.PhuCapDienThoai = model.PhuCapDienThoai;
        nv.PhuCapTrangPhuc = model.PhuCapTrangPhuc;
        nv.SoNguoiPhuThuoc = model.SoNguoiPhuThuoc;
        nv.CoCamKet08 = model.CoCamKet08;
        nv.DongBaoHiem = model.DongBaoHiem;
        nv.LaDoanVienCongDoan = model.LaDoanVienCongDoan;
        nv.SoTaiKhoanNganHang = model.SoTaiKhoanNganHang?.Trim();
        nv.TenNganHang = model.TenNganHang?.Trim();
        nv.DangLamViec = model.DangLamViec;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Cập nhật nhân viên '{nv.MaNhanVien}' thành công!";
        return RedirectToAction(nameof(Index));
    }
}
