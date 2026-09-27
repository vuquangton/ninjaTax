using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Controllers;

public class InventoryController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly AppDbContext _context;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        AppDbContext context,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _context = context;
        _logger = logger;
    }

    // GET: /Inventory
    public async Task<IActionResult> Index(long? branchId = null, long? khoId = null, int? loaiPhieu = null, DateTime? tuNgay = null, DateTime? denNgay = null)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        var warehouses = await _inventoryService.GetActiveWarehousesAsync(branchId);

        var inwardQuery = _context.PhieuNhapKhos
            .Include(p => p.Kho)
            .Include(p => p.NhaCungCap)
            .AsNoTracking();

        var outwardQuery = _context.PhieuXuatKhos
            .Include(p => p.Kho)
            .Include(p => p.KhachHang)
            .Include(p => p.PhongBan)
            .AsNoTracking();

        if (branchId.HasValue && branchId.Value > 0)
        {
            inwardQuery = inwardQuery.Where(p => p.ChiNhanhId == branchId.Value);
            outwardQuery = outwardQuery.Where(p => p.ChiNhanhId == branchId.Value);
        }

        if (khoId.HasValue && khoId.Value > 0)
        {
            inwardQuery = inwardQuery.Where(p => p.KhoId == khoId.Value);
            outwardQuery = outwardQuery.Where(p => p.KhoId == khoId.Value);
        }

        if (tuNgay.HasValue)
        {
            inwardQuery = inwardQuery.Where(p => p.NgayHachToan >= tuNgay.Value.Date);
            outwardQuery = outwardQuery.Where(p => p.NgayHachToan >= tuNgay.Value.Date);
        }

        if (denNgay.HasValue)
        {
            var endOfDay = denNgay.Value.Date.AddDays(1).AddTicks(-1);
            inwardQuery = inwardQuery.Where(p => p.NgayHachToan <= endOfDay);
            outwardQuery = outwardQuery.Where(p => p.NgayHachToan <= endOfDay);
        }

        var vm = new InventoryIndexViewModel
        {
            SelectedBranchId = branchId,
            SelectedKhoId = khoId,
            SelectedLoaiPhieu = loaiPhieu,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            BranchList = branches.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
                Selected = branchId == b.Id
            }).ToList(),
            WarehouseList = warehouses.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = $"{w.MaKho} - {w.TenKho}",
                Selected = khoId == w.Id
            }).ToList()
        };

        if (!loaiPhieu.HasValue || loaiPhieu.Value == 1)
        {
            var inList = await inwardQuery.OrderByDescending(p => p.NgayHachToan).ThenByDescending(p => p.Id).ToListAsync();
            vm.InwardVouchers = inList.Select(p => new InwardVoucherListItemViewModel
            {
                Id = p.Id,
                SoPhieu = p.SoPhieu,
                NgayNhap = p.NgayNhap,
                NgayHachToan = p.NgayHachToan,
                TenKho = p.Kho?.TenKho ?? "Chưa rõ",
                LoaiNhapKho = p.LoaiNhapKho,
                TenNhaCungCap = p.NhaCungCap?.TenDoiTuong,
                DienGiai = p.DienGiai,
                TongSoLuong = p.TongSoLuong,
                TongTienHang = p.TongTienHang,
                TrangThai = p.TrangThai,
                ButToanId = p.ButToanId
            }).ToList();
        }

        if (!loaiPhieu.HasValue || loaiPhieu.Value == 2)
        {
            var outList = await outwardQuery.OrderByDescending(p => p.NgayHachToan).ThenByDescending(p => p.Id).ToListAsync();
            vm.OutwardVouchers = outList.Select(p => new OutwardVoucherListItemViewModel
            {
                Id = p.Id,
                SoPhieu = p.SoPhieu,
                NgayXuat = p.NgayXuat,
                NgayHachToan = p.NgayHachToan,
                TenKho = p.Kho?.TenKho ?? "Chưa rõ",
                LoaiXuatKho = p.LoaiXuatKho,
                TenKhachHang = p.KhachHang?.TenDoiTuong,
                TenPhongBan = p.PhongBan?.TenPhongBan,
                DienGiai = p.DienGiai,
                TongSoLuong = p.TongSoLuong,
                TongTienGiaVon = p.TongTienGiaVon,
                TrangThai = p.TrangThai,
                ButToanId = p.ButToanId
            }).ToList();
        }

        return View(vm);
    }

    // GET: /Inventory/BaoCaoNhapXuatTon
    public async Task<IActionResult> BaoCaoNhapXuatTon(DateTime? tuNgay, DateTime? denNgay, long? khoId, long? branchId)
    {
        var fromDate = tuNgay ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var toDate = denNgay ?? DateTime.Today;

        var vm = await _inventoryService.LapBaoCaoNhapXuatTonAsync(fromDate, toDate, khoId, branchId);

        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        vm.BranchList = branches.Select(b => new SelectListItem
        {
            Value = b.Id.ToString(),
            Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
            Selected = b.Id == branchId
        }).ToList();

        var warehouses = await _inventoryService.GetActiveWarehousesAsync(branchId);
        vm.WarehouseList = warehouses.Select(w => new SelectListItem
        {
            Value = w.Id.ToString(),
            Text = $"{w.MaKho} - {w.TenKho}",
            Selected = w.Id == khoId
        }).ToList();

        return View(vm);
    }

    // GET: /Inventory/CreateInward
    public async Task<IActionResult> CreateInward(long? branchId = null)
    {
        var defaultBranch = branchId ?? (await _context.ChiNhanhs.Select(b => b.Id).FirstOrDefaultAsync());
        var warehouses = await _inventoryService.GetActiveWarehousesAsync(defaultBranch);
        var defaultKho = warehouses.FirstOrDefault()?.Id ?? 0;

        var count = await _context.PhieuNhapKhos.CountAsync() + 1;
        var vm = new PhieuNhapKhoEditViewModel
        {
            ChiNhanhId = defaultBranch,
            KhoId = defaultKho,
            SoPhieu = $"PNK-{DateTime.Today:yyyyMM}-{count:D4}",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LoaiNhapKho = LoaiNhapKho.MuaNgoai,
            GhiSoNgay = true
        };

        await PopulateInwardDropdownsAsync(vm);
        return View(vm);
    }

    // POST: /Inventory/CreateInward
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInward(PhieuNhapKhoEditViewModel vm)
    {
        if (!ModelState.IsValid || vm.ChiTiets == null || !vm.ChiTiets.Any())
        {
            if (vm.ChiTiets == null || !vm.ChiTiets.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập ít nhất một dòng mặt hàng.");
            }
            await PopulateInwardDropdownsAsync(vm);
            return View(vm);
        }

        try
        {
            var pnk = new PhieuNhapKho
            {
                ChiNhanhId = vm.ChiNhanhId,
                KhoId = vm.KhoId,
                SoPhieu = (vm.SoPhieu ?? string.Empty).Trim().ToUpper(),
                NgayNhap = vm.NgayNhap,
                NgayHachToan = vm.NgayHachToan,
                LoaiNhapKho = vm.LoaiNhapKho,
                NhaCungCapId = vm.NhaCungCapId > 0 ? vm.NhaCungCapId : null,
                DienGiai = (vm.DienGiai ?? string.Empty).Trim(),
                TrangThai = TrangThaiPhieuKho.TamTinh,
                ChiTietNhapKhos = vm.ChiTiets.Select(c => new ChiTietNhapKho
                {
                    VatTuHangHoaId = c.VatTuHangHoaId,
                    SoLuong = c.SoLuong,
                    DonGia = c.DonGia,
                    ThanhTien = c.SoLuong * c.DonGia,
                    TaiKhoanNoId = c.TaiKhoanNoId,
                    TaiKhoanCoId = c.TaiKhoanCoId,
                    SoLo = c.SoLo?.Trim(),
                    HanSuDung = c.HanSuDung
                }).ToList()
            };

            await _inventoryService.SavePhieuNhapKhoAsync(pnk);

            if (vm.GhiSoNgay)
            {
                await _inventoryService.GhiSoPhieuNhapKhoAsync(pnk.Id);
                TempData["SuccessMessage"] = $"Đã lập và ghi sổ phiếu nhập kho '{pnk.SoPhieu}' thành công.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Đã lưu tạm phiếu nhập kho '{pnk.SoPhieu}' thành công.";
            }

            return RedirectToAction(nameof(Index), new { branchId = pnk.ChiNhanhId, loaiPhieu = 1 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo phiếu nhập kho: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateInwardDropdownsAsync(vm);
            return View(vm);
        }
    }

    // GET: /Inventory/CreateOutward
    public async Task<IActionResult> CreateOutward(long? branchId = null)
    {
        var defaultBranch = branchId ?? (await _context.ChiNhanhs.Select(b => b.Id).FirstOrDefaultAsync());
        var warehouses = await _inventoryService.GetActiveWarehousesAsync(defaultBranch);
        var defaultKho = warehouses.FirstOrDefault()?.Id ?? 0;

        var count = await _context.PhieuXuatKhos.CountAsync() + 1;
        var vm = new PhieuXuatKhoEditViewModel
        {
            ChiNhanhId = defaultBranch,
            KhoId = defaultKho,
            SoPhieu = $"PXK-{DateTime.Today:yyyyMM}-{count:D4}",
            NgayXuat = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LoaiXuatKho = LoaiXuatKho.BanHang,
            GhiSoNgay = true
        };

        await PopulateOutwardDropdownsAsync(vm);
        return View(vm);
    }

    // POST: /Inventory/CreateOutward
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOutward(PhieuXuatKhoEditViewModel vm)
    {
        if (!ModelState.IsValid || vm.ChiTiets == null || !vm.ChiTiets.Any())
        {
            if (vm.ChiTiets == null || !vm.ChiTiets.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập ít nhất một dòng mặt hàng xuất kho.");
            }
            await PopulateOutwardDropdownsAsync(vm);
            return View(vm);
        }

        try
        {
            var pxk = new PhieuXuatKho
            {
                ChiNhanhId = vm.ChiNhanhId,
                KhoId = vm.KhoId,
                SoPhieu = (vm.SoPhieu ?? string.Empty).Trim().ToUpper(),
                NgayXuat = vm.NgayXuat,
                NgayHachToan = vm.NgayHachToan,
                LoaiXuatKho = vm.LoaiXuatKho,
                KhachHangId = vm.KhachHangId > 0 ? vm.KhachHangId : null,
                PhongBanId = vm.PhongBanId > 0 ? vm.PhongBanId : null,
                DienGiai = (vm.DienGiai ?? string.Empty).Trim(),
                TrangThai = TrangThaiPhieuKho.TamTinh,
                ChiTietXuatKhos = vm.ChiTiets.Select(c => new ChiTietXuatKho
                {
                    VatTuHangHoaId = c.VatTuHangHoaId,
                    SoLuong = c.SoLuong,
                    DonGiaVon = c.DonGiaVon,
                    TienGiaVon = c.SoLuong * c.DonGiaVon,
                    TaiKhoanNoId = c.TaiKhoanNoId,
                    TaiKhoanCoId = c.TaiKhoanCoId
                }).ToList()
            };

            await _inventoryService.SavePhieuXuatKhoAsync(pxk);

            if (vm.GhiSoNgay)
            {
                await _inventoryService.GhiSoPhieuXuatKhoAsync(pxk.Id);
                TempData["SuccessMessage"] = $"Đã lập và ghi sổ phiếu xuất kho '{pxk.SoPhieu}' thành công.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Đã lưu tạm phiếu xuất kho '{pxk.SoPhieu}' thành công.";
            }

            return RedirectToAction(nameof(Index), new { branchId = pxk.ChiNhanhId, loaiPhieu = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo phiếu xuất kho: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateOutwardDropdownsAsync(vm);
            return View(vm);
        }
    }

    // POST: /Inventory/PostInward/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostInward(long id)
    {
        try
        {
            var pnk = await _inventoryService.GhiSoPhieuNhapKhoAsync(id);
            TempData["SuccessMessage"] = $"Đã ghi sổ phiếu nhập kho '{pnk.SoPhieu}' thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { loaiPhieu = 1 });
    }

    // POST: /Inventory/UnpostInward/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnpostInward(long id)
    {
        try
        {
            var ok = await _inventoryService.HuyGhiSoPhieuNhapKhoAsync(id);
            if (ok)
            {
                TempData["SuccessMessage"] = "Đã hủy ghi sổ phiếu nhập kho thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy ghi sổ phiếu nhập kho.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { loaiPhieu = 1 });
    }

    // POST: /Inventory/PostOutward/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostOutward(long id)
    {
        try
        {
            var pxk = await _inventoryService.GhiSoPhieuXuatKhoAsync(id);
            TempData["SuccessMessage"] = $"Đã ghi sổ phiếu xuất kho '{pxk.SoPhieu}' thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { loaiPhieu = 2 });
    }

    // POST: /Inventory/UnpostOutward/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnpostOutward(long id)
    {
        try
        {
            var ok = await _inventoryService.HuyGhiSoPhieuXuatKhoAsync(id);
            if (ok)
            {
                TempData["SuccessMessage"] = "Đã hủy ghi sổ phiếu xuất kho thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy ghi sổ phiếu xuất kho.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { loaiPhieu = 2 });
    }

    // GET: /Inventory/GetStockBalanceJson
    [HttpGet]
    public async Task<IActionResult> GetStockBalanceJson(long khoId, long vatTuId)
    {
        var balance = await _inventoryService.GetStockBalanceAsync(khoId, vatTuId);
        var avgCost = await _inventoryService.CalculateWeightedAverageCostAsync(khoId, vatTuId, DateTime.Today);
        return Json(new { stockBalance = balance, averageCost = avgCost });
    }

    #region Helper Dropdowns

    private async Task PopulateInwardDropdownsAsync(PhieuNhapKhoEditViewModel vm)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        vm.BranchList = branches.Select(b => new SelectListItem
        {
            Value = b.Id.ToString(),
            Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
            Selected = b.Id == vm.ChiNhanhId
        }).ToList();

        var warehouses = await _inventoryService.GetActiveWarehousesAsync(vm.ChiNhanhId);
        vm.WarehouseList = warehouses.Select(w => new SelectListItem
        {
            Value = w.Id.ToString(),
            Text = $"{w.MaKho} - {w.TenKho}",
            Selected = w.Id == vm.KhoId
        }).ToList();

        var suppliers = await _context.DoiTuongs.AsNoTracking()
            .Where(d => d.Loai == LoaiDoiTuong.NhaCungCap && d.DangHoatDong)
            .OrderBy(d => d.TenDoiTuong).ToListAsync();
        vm.SupplierList = suppliers.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.MaDoiTuong} - {s.TenDoiTuong}",
            Selected = s.Id == vm.NhaCungCapId
        }).ToList();

        var items = await _context.VatTuHangHoas.AsNoTracking()
            .Where(v => v.DangHoatDong && v.DangTheoDoiTonKho)
            .OrderBy(v => v.MaVatTu).ToListAsync();
        vm.ItemList = items.Select(v => new SelectListItem
        {
            Value = v.Id.ToString(),
            Text = $"{v.MaVatTu} - {v.TenVatTu} ({v.DonViTinh}) [Giá mua: {v.DonGiaMuaGanNhat:N0}đ]"
        }).ToList();
    }

    private async Task PopulateOutwardDropdownsAsync(PhieuXuatKhoEditViewModel vm)
    {
        var branches = await _context.ChiNhanhs.AsNoTracking().OrderBy(b => b.MaChiNhanh).ToListAsync();
        vm.BranchList = branches.Select(b => new SelectListItem
        {
            Value = b.Id.ToString(),
            Text = $"{b.MaChiNhanh} - {b.TenChiNhanh}",
            Selected = b.Id == vm.ChiNhanhId
        }).ToList();

        var warehouses = await _inventoryService.GetActiveWarehousesAsync(vm.ChiNhanhId);
        vm.WarehouseList = warehouses.Select(w => new SelectListItem
        {
            Value = w.Id.ToString(),
            Text = $"{w.MaKho} - {w.TenKho}",
            Selected = w.Id == vm.KhoId
        }).ToList();

        var customers = await _context.DoiTuongs.AsNoTracking()
            .Where(d => d.Loai == LoaiDoiTuong.KhachHang && d.DangHoatDong)
            .OrderBy(d => d.TenDoiTuong).ToListAsync();
        vm.CustomerList = customers.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.MaDoiTuong} - {c.TenDoiTuong}",
            Selected = c.Id == vm.KhachHangId
        }).ToList();

        var depts = await _context.PhongBans.AsNoTracking()
            .Where(p => p.ChiNhanhId == vm.ChiNhanhId && p.DangHoatDong)
            .OrderBy(p => p.MaPhongBan).ToListAsync();
        vm.DepartmentList = depts.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.MaPhongBan} - {p.TenPhongBan}",
            Selected = p.Id == vm.PhongBanId
        }).ToList();

        var items = await _context.VatTuHangHoas.AsNoTracking()
            .Where(v => v.DangHoatDong && v.DangTheoDoiTonKho)
            .OrderBy(v => v.MaVatTu).ToListAsync();
        vm.ItemList = items.Select(v => new SelectListItem
        {
            Value = v.Id.ToString(),
            Text = $"{v.MaVatTu} - {v.TenVatTu} ({v.DonViTinh})"
        }).ToList();
    }

    #endregion
}
