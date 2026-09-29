using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        AppDbContext context,
        IButToanService butToanService,
        ILogger<InventoryService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _logger = logger;
    }

    public async Task<decimal> GetStockBalanceAsync(long khoId, long vatTuId, DateTime? asOfDate = null)
    {
        // 1. Tổng số lượng nhập kho đã ghi sổ
        var nhapQuery = _context.ChiTietNhapKhos
            .Where(c => c.PhieuNhapKho != null &&
                        c.PhieuNhapKho.KhoId == khoId &&
                        c.VatTuHangHoaId == vatTuId &&
                        c.PhieuNhapKho.TrangThai == TrangThaiPhieuKho.DaGhiSo);

        if (asOfDate.HasValue)
        {
            var endOfDay = asOfDate.Value.Date.AddDays(1).AddTicks(-1);
            nhapQuery = nhapQuery.Where(c => c.PhieuNhapKho!.NgayHachToan <= endOfDay);
        }

        var tongNhap = await nhapQuery.SumAsync(c => (decimal?)c.SoLuong) ?? 0m;

        // 2. Tổng số lượng xuất kho đã ghi sổ
        var xuatQuery = _context.ChiTietXuatKhos
            .Where(c => c.PhieuXuatKho != null &&
                        c.PhieuXuatKho.KhoId == khoId &&
                        c.VatTuHangHoaId == vatTuId &&
                        c.PhieuXuatKho.TrangThai == TrangThaiPhieuKho.DaGhiSo);

        if (asOfDate.HasValue)
        {
            var endOfDay = asOfDate.Value.Date.AddDays(1).AddTicks(-1);
            xuatQuery = xuatQuery.Where(c => c.PhieuXuatKho!.NgayHachToan <= endOfDay);
        }

        var tongXuat = await xuatQuery.SumAsync(c => (decimal?)c.SoLuong) ?? 0m;

        return tongNhap - tongXuat;
    }

    public async Task<decimal> GetTotalStockBalanceAcrossWarehousesAsync(long vatTuId, long? branchId = null, DateTime? asOfDate = null)
    {
        var nhapQuery = _context.ChiTietNhapKhos
            .Where(c => c.PhieuNhapKho != null &&
                        c.VatTuHangHoaId == vatTuId &&
                        c.PhieuNhapKho.TrangThai == TrangThaiPhieuKho.DaGhiSo);

        var xuatQuery = _context.ChiTietXuatKhos
            .Where(c => c.PhieuXuatKho != null &&
                        c.VatTuHangHoaId == vatTuId &&
                        c.PhieuXuatKho.TrangThai == TrangThaiPhieuKho.DaGhiSo);

        if (branchId.HasValue && branchId.Value > 0)
        {
            nhapQuery = nhapQuery.Where(c => c.PhieuNhapKho!.ChiNhanhId == branchId.Value);
            xuatQuery = xuatQuery.Where(c => c.PhieuXuatKho!.ChiNhanhId == branchId.Value);
        }

        if (asOfDate.HasValue)
        {
            var endOfDay = asOfDate.Value.Date.AddDays(1).AddTicks(-1);
            nhapQuery = nhapQuery.Where(c => c.PhieuNhapKho!.NgayHachToan <= endOfDay);
            xuatQuery = xuatQuery.Where(c => c.PhieuXuatKho!.NgayHachToan <= endOfDay);
        }

        var tongNhap = await nhapQuery.SumAsync(c => (decimal?)c.SoLuong) ?? 0m;
        var tongXuat = await xuatQuery.SumAsync(c => (decimal?)c.SoLuong) ?? 0m;

        return tongNhap - tongXuat;
    }

    public async Task ValidateStockAvailabilityAsync(long khoId, long vatTuId, decimal soLuongXuat, DateTime ngayXuat, long? excludeXuatKhoId = null)
    {
        if (soLuongXuat <= 0)
        {
            throw new ArgumentException("Số lượng xuất kho phải lớn hơn 0.");
        }

        var vatTu = await _context.VatTuHangHoas.FindAsync(vatTuId);
        if (vatTu == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vật tư hàng hóa ID {vatTuId}.");
        }

        // Bỏ qua kiểm tra tồn kho nếu là loại Dịch vụ không quản lý tồn
        if (!vatTu.DangTheoDoiTonKho || vatTu.LoaiVatTu == LoaiVatTuHangHoa.DichVu)
        {
            return;
        }

        var currentBalance = await GetStockBalanceAsync(khoId, vatTuId, ngayXuat);

        // Nếu đang sửa một phiếu xuất đã ghi sổ trước đó, cộng lại số lượng của chính phiếu đó vào tồn khả dụng
        if (excludeXuatKhoId.HasValue && excludeXuatKhoId.Value > 0)
        {
            var existingQty = await _context.ChiTietXuatKhos
                .Where(c => c.PhieuXuatKhoId == excludeXuatKhoId.Value && c.VatTuHangHoaId == vatTuId)
                .SumAsync(c => (decimal?)c.SoLuong) ?? 0m;

            currentBalance += existingQty;
        }

        if (currentBalance < soLuongXuat)
        {
            var kho = await _context.Khos.FindAsync(khoId);
            var tenKho = kho != null ? $"{kho.MaKho} - {kho.TenKho}" : $"Kho ID {khoId}";

            _logger.LogWarning("Chặn xuất âm kho: {Vt} ({Ma}) tại {Kho}. Yêu cầu xuất: {Req}, Tồn khả dụng: {Cur}",
                vatTu.TenVatTu, vatTu.MaVatTu, tenKho, soLuongXuat, currentBalance);

            throw new InvalidOperationException(
                $"Cảnh báo vi phạm xuất âm kho: Mặt hàng '{vatTu.TenVatTu}' ({vatTu.MaVatTu}) tại '{tenKho}' chỉ còn tồn {currentBalance:N2} {vatTu.DonViTinh}, không đủ để xuất {soLuongXuat:N2} {vatTu.DonViTinh}. Vui lòng nhập hàng trước khi xuất kho.");
        }
    }

    public async Task<decimal> CalculateWeightedAverageCostAsync(long khoId, long vatTuId, DateTime asOfDate)
    {
        var endOfDay = asOfDate.Date.AddDays(1).AddTicks(-1);

        // Tính tổng tiền nhập và tổng số lượng nhập lũy kế đã ghi sổ đến ngày xuất
        var nhapList = await _context.ChiTietNhapKhos
            .Where(c => c.PhieuNhapKho != null &&
                        c.PhieuNhapKho.KhoId == khoId &&
                        c.VatTuHangHoaId == vatTuId &&
                        c.PhieuNhapKho.TrangThai == TrangThaiPhieuKho.DaGhiSo &&
                        c.PhieuNhapKho.NgayHachToan <= endOfDay)
            .Select(c => new { c.SoLuong, c.ThanhTien })
            .ToListAsync();

        var tongSoLuongNhap = nhapList.Sum(c => c.SoLuong);
        var tongThanhTienNhap = nhapList.Sum(c => c.ThanhTien);

        if (tongSoLuongNhap > 0 && tongThanhTienNhap > 0)
        {
            return Math.Round(tongThanhTienNhap / tongSoLuongNhap, 4);
        }

        // Nếu chưa có nhập trong kỳ tại kho này, lấy đơn giá mua gần nhất từ danh mục
        var vatTu = await _context.VatTuHangHoas.FindAsync(vatTuId);
        if (vatTu != null && vatTu.DonGiaMuaGanNhat > 0)
        {
            return vatTu.DonGiaMuaGanNhat;
        }

        return 0m;
    }

    public async Task<PhieuNhapKho> SavePhieuNhapKhoAsync(PhieuNhapKho phieuNhap)
    {
        phieuNhap.SoPhieu = (phieuNhap.SoPhieu ?? string.Empty).Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(phieuNhap.SoPhieu))
        {
            throw new ArgumentException("Số phiếu nhập kho không được để trống.");
        }

        if (phieuNhap.KhoId <= 0)
        {
            throw new ArgumentException("Kho nhập hàng không hợp lệ.");
        }

        if (phieuNhap.ChiTietNhapKhos == null || !phieuNhap.ChiTietNhapKhos.Any())
        {
            throw new ArgumentException("Phiếu nhập kho phải có ít nhất 1 dòng mặt hàng.");
        }

        // Tính lại tổng số lượng và tổng tiền
        phieuNhap.TongSoLuong = phieuNhap.ChiTietNhapKhos.Sum(c => c.SoLuong);
        phieuNhap.TongTienHang = phieuNhap.ChiTietNhapKhos.Sum(c => c.ThanhTien);

        if (phieuNhap.Id == 0)
        {
            phieuNhap.NgayTao = DateTime.UtcNow;
            _context.PhieuNhapKhos.Add(phieuNhap);
        }
        else
        {
            var existing = await _context.PhieuNhapKhos
                .Include(p => p.ChiTietNhapKhos)
                .FirstOrDefaultAsync(p => p.Id == phieuNhap.Id);

            if (existing == null) throw new KeyNotFoundException("Không tìm thấy phiếu nhập kho.");
            if (existing.TrangThai == TrangThaiPhieuKho.DaGhiSo)
            {
                throw new InvalidOperationException("Không thể sửa phiếu nhập kho đã ghi sổ. Vui lòng hủy ghi sổ trước.");
            }

            existing.KhoId = phieuNhap.KhoId;
            existing.SoPhieu = phieuNhap.SoPhieu;
            existing.NgayNhap = phieuNhap.NgayNhap;
            existing.NgayHachToan = phieuNhap.NgayHachToan;
            existing.LoaiNhapKho = phieuNhap.LoaiNhapKho;
            existing.NhaCungCapId = phieuNhap.NhaCungCapId;
            existing.DienGiai = phieuNhap.DienGiai;
            existing.TongSoLuong = phieuNhap.TongSoLuong;
            existing.TongTienHang = phieuNhap.TongTienHang;
            existing.NgayCapNhat = DateTime.UtcNow;

            _context.ChiTietNhapKhos.RemoveRange(existing.ChiTietNhapKhos);
            existing.ChiTietNhapKhos = phieuNhap.ChiTietNhapKhos;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã lưu phiếu nhập kho {SoPhieu}", phieuNhap.SoPhieu);
        return phieuNhap;
    }

    public async Task<PhieuNhapKho> GhiSoPhieuNhapKhoAsync(long phieuNhapKhoId)
    {
        var phieuNhap = await _context.PhieuNhapKhos
            .Include(p => p.ChiTietNhapKhos)
                .ThenInclude(c => c.VatTuHangHoa)
            .Include(p => p.Kho)
            .Include(p => p.NhaCungCap)
            .FirstOrDefaultAsync(p => p.Id == phieuNhapKhoId);

        if (phieuNhap == null)
        {
            throw new KeyNotFoundException("Không tìm thấy phiếu nhập kho.");
        }

        if (phieuNhap.TrangThai == TrangThaiPhieuKho.DaGhiSo)
        {
            throw new InvalidOperationException("Phiếu nhập kho này đã được ghi sổ trước đó.");
        }

        if (!phieuNhap.ChiTietNhapKhos.Any())
        {
            throw new InvalidOperationException("Phiếu nhập kho không có chi tiết mặt hàng để ghi sổ.");
        }

        // Bất biến TT99: Kiểm tra tuyệt đối cấm TK 911
        var tkIds = phieuNhap.ChiTietNhapKhos
            .SelectMany(c => new[] { c.TaiKhoanNoId, c.TaiKhoanCoId })
            .Distinct()
            .ToList();

        var accounts = await _context.TaiKhoans
            .Where(t => tkIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.MaTaiKhoan);

        if (accounts.Values.Any(ma => ma == "911" || ma.StartsWith("911")))
        {
            throw new InvalidOperationException("Nghiêm cấm tuyệt đối sử dụng Tài khoản 911 theo chuẩn TT 99/2025/TT-BTC.");
        }

        // Sinh Bút toán kép Sổ Nhật ký chung (General Ledger)
        var butToan = new ButToan
        {
            SoChungTu = $"PKT-NK-{phieuNhap.SoPhieu}",
            NgayHachToan = phieuNhap.NgayHachToan,
            NgayChungTu = phieuNhap.NgayNhap,
            SoChungTuGoc = phieuNhap.SoPhieu,
            NgayChungTuGoc = phieuNhap.NgayNhap,
            DienGiai = string.IsNullOrWhiteSpace(phieuNhap.DienGiai)
                ? $"Nhập kho: {phieuNhap.SoPhieu} tại {phieuNhap.Kho?.TenKho}"
                : phieuNhap.DienGiai,
            TrangThai = TrangThaiButToan.DaGhiSo
        };

        var lines = new List<ChiTietButToan>();
        int dongSo = 1;

        foreach (var item in phieuNhap.ChiTietNhapKhos)
        {
            lines.Add(new ChiTietButToan
            {
                DongSo = dongSo++,
                TaiKhoanNoId = item.TaiKhoanNoId,
                TaiKhoanCoId = item.TaiKhoanCoId,
                SoTien = item.ThanhTien,
                DoiTuongId = phieuNhap.NhaCungCapId,
                DienGiai = $"Nhập kho: {item.VatTuHangHoa?.TenVatTu ?? "Hàng hóa"}"
            });

            // Cập nhật giá mua gần nhất vào danh mục vật tư
            if (item.VatTuHangHoa != null && item.DonGia > 0)
            {
                item.VatTuHangHoa.DonGiaMuaGanNhat = item.DonGia;
            }
        }

        butToan.ChiTietButToans = lines;

        var (ok, msg, createdButToan) = await _butToanService.TaoMoiAsync(butToan);
        if (!ok || createdButToan == null)
        {
            throw new InvalidOperationException($"Lỗi hạch toán Sổ Cái cho phiếu nhập kho: {msg}");
        }

        phieuNhap.ButToanId = createdButToan.Id;
        phieuNhap.TrangThai = TrangThaiPhieuKho.DaGhiSo;
        phieuNhap.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã ghi sổ phiếu nhập kho {SoPhieu} - Bút toán GL ID: {BtId}", phieuNhap.SoPhieu, createdButToan.Id);
        return phieuNhap;
    }

    public async Task<bool> HuyGhiSoPhieuNhapKhoAsync(long phieuNhapKhoId)
    {
        var phieuNhap = await _context.PhieuNhapKhos
            .Include(p => p.ChiTietNhapKhos)
            .FirstOrDefaultAsync(p => p.Id == phieuNhapKhoId);

        if (phieuNhap == null || phieuNhap.TrangThai != TrangThaiPhieuKho.DaGhiSo)
        {
            return false;
        }

        // Bất biến: Kiểm tra xem hủy phiếu nhập này có làm tồn kho hiện tại bị âm không
        foreach (var item in phieuNhap.ChiTietNhapKhos)
        {
            var currentStock = await GetStockBalanceAsync(phieuNhap.KhoId, item.VatTuHangHoaId);
            if (currentStock < item.SoLuong)
            {
                throw new InvalidOperationException(
                    $"Không thể hủy ghi sổ phiếu nhập {phieuNhap.SoPhieu}! Mặt hàng ID {item.VatTuHangHoaId} hiện chỉ còn tồn {currentStock:N2}, nếu hủy phiếu nhập (giảm {item.SoLuong:N2}) sẽ gây âm kho.");
            }
        }

        // Xóa bút toán sổ cái liên kết
        if (phieuNhap.ButToanId.HasValue)
        {
            await _butToanService.XoaAsync(phieuNhap.ButToanId.Value);
            phieuNhap.ButToanId = null;
        }

        phieuNhap.TrangThai = TrangThaiPhieuKho.TamTinh;
        phieuNhap.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã hủy ghi sổ phiếu nhập kho {SoPhieu}", phieuNhap.SoPhieu);
        return true;
    }

    public async Task<PhieuNhapKho?> GetPhieuNhapByIdAsync(long id)
    {
        return await _context.PhieuNhapKhos
            .Include(p => p.ChiNhanh)
            .Include(p => p.Kho)
            .Include(p => p.NhaCungCap)
            .Include(p => p.ChiTietNhapKhos)
                .ThenInclude(c => c.VatTuHangHoa)
            .Include(p => p.ChiTietNhapKhos)
                .ThenInclude(c => c.TaiKhoanNo)
            .Include(p => p.ChiTietNhapKhos)
                .ThenInclude(c => c.TaiKhoanCo)
            .Include(p => p.ButToan)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PhieuXuatKho> SavePhieuXuatKhoAsync(PhieuXuatKho phieuXuat)
    {
        phieuXuat.SoPhieu = (phieuXuat.SoPhieu ?? string.Empty).Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(phieuXuat.SoPhieu))
        {
            throw new ArgumentException("Số phiếu xuất kho không được để trống.");
        }

        if (phieuXuat.KhoId <= 0)
        {
            throw new ArgumentException("Kho xuất hàng không hợp lệ.");
        }

        if (phieuXuat.ChiTietXuatKhos == null || !phieuXuat.ChiTietXuatKhos.Any())
        {
            throw new ArgumentException("Phiếu xuất kho phải có ít nhất 1 dòng mặt hàng.");
        }

        // Validate tồn kho cho từng dòng
        foreach (var line in phieuXuat.ChiTietXuatKhos)
        {
            await ValidateStockAvailabilityAsync(phieuXuat.KhoId, line.VatTuHangHoaId, line.SoLuong, phieuXuat.NgayHachToan, phieuXuat.Id > 0 ? phieuXuat.Id : null);
        }

        phieuXuat.TongSoLuong = phieuXuat.ChiTietXuatKhos.Sum(c => c.SoLuong);
        phieuXuat.TongTienGiaVon = phieuXuat.ChiTietXuatKhos.Sum(c => c.TienGiaVon);

        if (phieuXuat.Id == 0)
        {
            phieuXuat.NgayTao = DateTime.UtcNow;
            _context.PhieuXuatKhos.Add(phieuXuat);
        }
        else
        {
            var existing = await _context.PhieuXuatKhos
                .Include(p => p.ChiTietXuatKhos)
                .FirstOrDefaultAsync(p => p.Id == phieuXuat.Id);

            if (existing == null) throw new KeyNotFoundException("Không tìm thấy phiếu xuất kho.");
            if (existing.TrangThai == TrangThaiPhieuKho.DaGhiSo)
            {
                throw new InvalidOperationException("Không thể sửa phiếu xuất kho đã ghi sổ. Vui lòng hủy ghi sổ trước.");
            }

            existing.KhoId = phieuXuat.KhoId;
            existing.SoPhieu = phieuXuat.SoPhieu;
            existing.NgayXuat = phieuXuat.NgayXuat;
            existing.NgayHachToan = phieuXuat.NgayHachToan;
            existing.LoaiXuatKho = phieuXuat.LoaiXuatKho;
            existing.KhachHangId = phieuXuat.KhachHangId;
            existing.PhongBanId = phieuXuat.PhongBanId;
            existing.DienGiai = phieuXuat.DienGiai;
            existing.TongSoLuong = phieuXuat.TongSoLuong;
            existing.TongTienGiaVon = phieuXuat.TongTienGiaVon;
            existing.NgayCapNhat = DateTime.UtcNow;

            _context.ChiTietXuatKhos.RemoveRange(existing.ChiTietXuatKhos);
            existing.ChiTietXuatKhos = phieuXuat.ChiTietXuatKhos;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã lưu phiếu xuất kho {SoPhieu}", phieuXuat.SoPhieu);
        return phieuXuat;
    }

    public async Task<PhieuXuatKho> GhiSoPhieuXuatKhoAsync(long phieuXuatKhoId)
    {
        var phieuXuat = await _context.PhieuXuatKhos
            .Include(p => p.ChiTietXuatKhos)
                .ThenInclude(c => c.VatTuHangHoa)
            .Include(p => p.Kho)
            .Include(p => p.KhachHang)
            .Include(p => p.PhongBan)
            .FirstOrDefaultAsync(p => p.Id == phieuXuatKhoId);

        if (phieuXuat == null)
        {
            throw new KeyNotFoundException("Không tìm thấy phiếu xuất kho.");
        }

        if (phieuXuat.TrangThai == TrangThaiPhieuKho.DaGhiSo)
        {
            throw new InvalidOperationException("Phiếu xuất kho này đã được ghi sổ trước đó.");
        }

        if (!phieuXuat.ChiTietXuatKhos.Any())
        {
            throw new InvalidOperationException("Phiếu xuất kho không có chi tiết mặt hàng để ghi sổ.");
        }

        // Bất biến: Kiểm tra tính khả dụng tồn kho & Tự động tính giá vốn BQGQ nếu đơn giá = 0
        foreach (var item in phieuXuat.ChiTietXuatKhos)
        {
            await ValidateStockAvailabilityAsync(phieuXuat.KhoId, item.VatTuHangHoaId, item.SoLuong, phieuXuat.NgayHachToan);

            if (item.DonGiaVon <= 0)
            {
                item.DonGiaVon = await CalculateWeightedAverageCostAsync(phieuXuat.KhoId, item.VatTuHangHoaId, phieuXuat.NgayHachToan);
                item.TienGiaVon = Math.Round(item.SoLuong * item.DonGiaVon, 4);
            }

            if (item.TienGiaVon <= 0)
            {
                throw new InvalidOperationException($"Đơn giá vốn hoặc thành tiền của vật tư '{item.VatTuHangHoa?.TenVatTu ?? item.VatTuHangHoaId.ToString()}' bằng 0. Không thể ghi sổ kế toán với giá trị 0.");
            }
        }

        phieuXuat.TongSoLuong = phieuXuat.ChiTietXuatKhos.Sum(c => c.SoLuong);
        phieuXuat.TongTienGiaVon = phieuXuat.ChiTietXuatKhos.Sum(c => c.TienGiaVon);

        // Bất biến TT99: Kiểm tra tuyệt đối cấm TK 911
        var tkIds = phieuXuat.ChiTietXuatKhos
            .SelectMany(c => new[] { c.TaiKhoanNoId, c.TaiKhoanCoId })
            .Distinct()
            .ToList();

        var accounts = await _context.TaiKhoans
            .Where(t => tkIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.MaTaiKhoan);

        if (accounts.Values.Any(ma => ma == "911" || ma.StartsWith("911")))
        {
            throw new InvalidOperationException("Nghiêm cấm tuyệt đối sử dụng Tài khoản 911 theo chuẩn TT 99/2025/TT-BTC.");
        }

        // Sinh Bút toán Giá vốn kép Sổ Nhật ký chung (General Ledger)
        var butToan = new ButToan
        {
            SoChungTu = $"PKT-XK-{phieuXuat.SoPhieu}",
            NgayHachToan = phieuXuat.NgayHachToan,
            NgayChungTu = phieuXuat.NgayXuat,
            SoChungTuGoc = phieuXuat.SoPhieu,
            NgayChungTuGoc = phieuXuat.NgayXuat,
            DienGiai = string.IsNullOrWhiteSpace(phieuXuat.DienGiai)
                ? $"Giá vốn xuất kho: {phieuXuat.SoPhieu} tại {phieuXuat.Kho?.TenKho}"
                : phieuXuat.DienGiai,
            TrangThai = TrangThaiButToan.DaGhiSo
        };

        var lines = new List<ChiTietButToan>();
        int dongSo = 1;

        foreach (var item in phieuXuat.ChiTietXuatKhos)
        {
            lines.Add(new ChiTietButToan
            {
                DongSo = dongSo++,
                TaiKhoanNoId = item.TaiKhoanNoId, // TK 632 / 154 / 642
                TaiKhoanCoId = item.TaiKhoanCoId, // TK 1561 / 152
                SoTien = item.TienGiaVon,
                PhongBanId = phieuXuat.PhongBanId, // Phân khúc báo cáo tài chính bộ phận
                DoiTuongId = phieuXuat.KhachHangId,
                DienGiai = $"Giá vốn xuất kho: {item.VatTuHangHoa?.TenVatTu ?? "Hàng hóa"}"
            });
        }

        butToan.ChiTietButToans = lines;

        var (ok, msg, createdButToan) = await _butToanService.TaoMoiAsync(butToan);
        if (!ok || createdButToan == null)
        {
            throw new InvalidOperationException($"Lỗi hạch toán Sổ Cái cho phiếu xuất kho: {msg}");
        }

        phieuXuat.ButToanId = createdButToan.Id;
        phieuXuat.TrangThai = TrangThaiPhieuKho.DaGhiSo;
        phieuXuat.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã ghi sổ phiếu xuất kho {SoPhieu} - Bút toán GL ID: {BtId}", phieuXuat.SoPhieu, createdButToan.Id);
        return phieuXuat;
    }

    public async Task<bool> HuyGhiSoPhieuXuatKhoAsync(long phieuXuatKhoId)
    {
        var phieuXuat = await _context.PhieuXuatKhos.FindAsync(phieuXuatKhoId);
        if (phieuXuat == null || phieuXuat.TrangThai != TrangThaiPhieuKho.DaGhiSo)
        {
            return false;
        }

        if (phieuXuat.ButToanId.HasValue)
        {
            await _butToanService.XoaAsync(phieuXuat.ButToanId.Value);
            phieuXuat.ButToanId = null;
        }

        phieuXuat.TrangThai = TrangThaiPhieuKho.TamTinh;
        phieuXuat.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã hủy ghi sổ phiếu xuất kho {SoPhieu}", phieuXuat.SoPhieu);
        return true;
    }

    public async Task<PhieuXuatKho?> GetPhieuXuatByIdAsync(long id)
    {
        return await _context.PhieuXuatKhos
            .Include(p => p.ChiNhanh)
            .Include(p => p.Kho)
            .Include(p => p.KhachHang)
            .Include(p => p.PhongBan)
            .Include(p => p.ChiTietXuatKhos)
                .ThenInclude(c => c.VatTuHangHoa)
            .Include(p => p.ChiTietXuatKhos)
                .ThenInclude(c => c.TaiKhoanNo)
            .Include(p => p.ChiTietXuatKhos)
                .ThenInclude(c => c.TaiKhoanCo)
            .Include(p => p.ButToan)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Kho>> GetActiveWarehousesAsync(long? branchId = null)
    {
        var query = _context.Khos
            .Include(k => k.ChiNhanh)
            .Include(k => k.ThuKho)
            .Include(k => k.TaiKhoanKhoMacDinh)
            .Where(k => k.DangHoatDong);

        if (branchId.HasValue && branchId.Value > 0)
        {
            query = query.Where(k => k.ChiNhanhId == branchId.Value);
        }

        return await query.OrderBy(k => k.MaKho).ToListAsync();
    }

    public async Task<Kho?> GetWarehouseByIdAsync(long id)
    {
        return await _context.Khos
            .Include(k => k.ChiNhanh)
            .Include(k => k.ThuKho)
            .Include(k => k.TaiKhoanKhoMacDinh)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Kho> SaveWarehouseAsync(Kho kho)
    {
        kho.MaKho = (kho.MaKho ?? string.Empty).Trim().ToUpper();
        kho.TenKho = (kho.TenKho ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(kho.MaKho))
        {
            throw new ArgumentException("Mã kho không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(kho.TenKho))
        {
            throw new ArgumentException("Tên kho không được để trống.");
        }

        var isDuplicate = await _context.Khos.AnyAsync(k =>
            k.ChiNhanhId == kho.ChiNhanhId &&
            k.MaKho == kho.MaKho &&
            k.Id != kho.Id);

        if (isDuplicate)
        {
            throw new ArgumentException($"Mã kho '{kho.MaKho}' đã tồn tại trong chi nhánh này.");
        }

        if (kho.Id == 0)
        {
            kho.NgayTao = DateTime.UtcNow;
            _context.Khos.Add(kho);
        }
        else
        {
            var existing = await _context.Khos.FindAsync(kho.Id);
            if (existing == null) throw new KeyNotFoundException("Không tìm thấy kho hàng.");

            existing.ChiNhanhId = kho.ChiNhanhId;
            existing.MaKho = kho.MaKho;
            existing.TenKho = kho.TenKho;
            existing.DiaChi = kho.DiaChi;
            existing.ThuKhoId = kho.ThuKhoId;
            existing.TaiKhoanKhoMacDinhId = kho.TaiKhoanKhoMacDinhId;
            existing.DangHoatDong = kho.DangHoatDong;
            existing.GhiChu = kho.GhiChu;
            existing.NgayCapNhat = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã lưu kho hàng {Ma} - {Ten}", kho.MaKho, kho.TenKho);
        return kho;
    }

    public async Task<List<Kho>> GetAllWarehousesAsync(long? branchId = null)
    {
        var query = _context.Khos
            .Include(k => k.ChiNhanh)
            .Include(k => k.ThuKho)
            .Include(k => k.TaiKhoanKhoMacDinh)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue && branchId.Value > 0)
        {
            query = query.Where(k => k.ChiNhanhId == branchId.Value);
        }

        return await query.OrderBy(k => k.MaKho).ToListAsync();
    }

    public async Task<bool> CanDeleteWarehouseAsync(long khoId)
    {
        var coNhap = await _context.PhieuNhapKhos.AnyAsync(p => p.KhoId == khoId);
        if (coNhap) return false;

        var coXuat = await _context.PhieuXuatKhos.AnyAsync(p => p.KhoId == khoId);
        if (coXuat) return false;

        return true;
    }

    public async Task<(bool Success, string? Message)> DeleteWarehouseAsync(long khoId)
    {
        var kho = await _context.Khos.FindAsync(khoId);
        if (kho == null)
        {
            return (false, "Không tìm thấy kho hàng cần xóa.");
        }

        var canDelete = await CanDeleteWarehouseAsync(khoId);
        if (!canDelete)
        {
            return (false, $"Kho hàng '{kho.MaKho} - {kho.TenKho}' đã phát sinh phiếu nhập/xuất kho. Không được phép xóa! Vui lòng chuyển trạng thái sang 'Ngừng hoạt động'.");
        }

        _context.Khos.Remove(kho);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã xóa kho hàng {Ma}", kho.MaKho);
        return (true, null);
    }

    public async Task<BaoCaoNhapXuatTonViewModel> LapBaoCaoNhapXuatTonAsync(DateTime tuNgay, DateTime denNgay, long? khoId = null, long? branchId = null)
    {
        var startOfDay = tuNgay.Date;
        var endOfDay = denNgay.Date.AddDays(1).AddTicks(-1);

        var items = await _context.VatTuHangHoas
            .AsNoTracking()
            .Where(v => v.DangTheoDoiTonKho && v.LoaiVatTu != LoaiVatTuHangHoa.DichVu)
            .OrderBy(v => v.MaVatTu)
            .ToListAsync();

        var nhapQuery = _context.ChiTietNhapKhos
            .Include(c => c.PhieuNhapKho)
            .Where(c => c.PhieuNhapKho != null && c.PhieuNhapKho.TrangThai == TrangThaiPhieuKho.DaGhiSo)
            .AsQueryable();

        var xuatQuery = _context.ChiTietXuatKhos
            .Include(c => c.PhieuXuatKho)
            .Where(c => c.PhieuXuatKho != null && c.PhieuXuatKho.TrangThai == TrangThaiPhieuKho.DaGhiSo)
            .AsQueryable();

        if (branchId.HasValue && branchId.Value > 0)
        {
            nhapQuery = nhapQuery.Where(c => c.PhieuNhapKho!.ChiNhanhId == branchId.Value);
            xuatQuery = xuatQuery.Where(c => c.PhieuXuatKho!.ChiNhanhId == branchId.Value);
        }

        if (khoId.HasValue && khoId.Value > 0)
        {
            nhapQuery = nhapQuery.Where(c => c.PhieuNhapKho!.KhoId == khoId.Value);
            xuatQuery = xuatQuery.Where(c => c.PhieuXuatKho!.KhoId == khoId.Value);
        }

        var allNhap = await nhapQuery.ToListAsync();
        var allXuat = await xuatQuery.ToListAsync();

        var reportItems = new List<DongNhapXuatTonViewModel>();

        foreach (var vt in items)
        {
            // 1. Tồn đầu kỳ (< startOfDay)
            var nhapDau = allNhap.Where(c => c.VatTuHangHoaId == vt.Id && c.PhieuNhapKho!.NgayHachToan < startOfDay).ToList();
            var xuatDau = allXuat.Where(c => c.VatTuHangHoaId == vt.Id && c.PhieuXuatKho!.NgayHachToan < startOfDay).ToList();

            var slTonDau = nhapDau.Sum(c => c.SoLuong) - xuatDau.Sum(c => c.SoLuong);
            var ttTonDau = nhapDau.Sum(c => c.ThanhTien) - xuatDau.Sum(c => c.TienGiaVon);

            // 2. Nhập trong kỳ (startOfDay <= hạch toán <= endOfDay)
            var nhapTrongKy = allNhap.Where(c => c.VatTuHangHoaId == vt.Id && c.PhieuNhapKho!.NgayHachToan >= startOfDay && c.PhieuNhapKho.NgayHachToan <= endOfDay).ToList();
            var slNhapTrongKy = nhapTrongKy.Sum(c => c.SoLuong);
            var ttNhapTrongKy = nhapTrongKy.Sum(c => c.ThanhTien);

            // 3. Xuất trong kỳ (startOfDay <= hạch toán <= endOfDay)
            var xuatTrongKy = allXuat.Where(c => c.VatTuHangHoaId == vt.Id && c.PhieuXuatKho!.NgayHachToan >= startOfDay && c.PhieuXuatKho.NgayHachToan <= endOfDay).ToList();
            var slXuatTrongKy = xuatTrongKy.Sum(c => c.SoLuong);
            var ttXuatTrongKy = xuatTrongKy.Sum(c => c.TienGiaVon);

            // 4. Tồn cuối kỳ
            var slTonCuoi = slTonDau + slNhapTrongKy - slXuatTrongKy;
            var ttTonCuoi = ttTonDau + ttNhapTrongKy - ttXuatTrongKy;

            // Chỉ hiển thị mặt hàng có phát sinh hoặc còn tồn
            if (slTonDau != 0 || ttTonDau != 0 || slNhapTrongKy != 0 || slXuatTrongKy != 0 || slTonCuoi != 0 || ttTonCuoi != 0)
            {
                reportItems.Add(new DongNhapXuatTonViewModel
                {
                    VatTuHangHoaId = vt.Id,
                    MaVatTu = vt.MaVatTu,
                    TenVatTu = vt.TenVatTu,
                    DonViTinh = vt.DonViTinh,
                    LoaiVatTu = vt.LoaiVatTu,
                    TonDauSoLuong = slTonDau,
                    TonDauThanhTien = ttTonDau,
                    NhapSoLuong = slNhapTrongKy,
                    NhapThanhTien = ttNhapTrongKy,
                    XuatSoLuong = slXuatTrongKy,
                    XuatThanhTien = ttXuatTrongKy,
                    TonCuoiSoLuong = slTonCuoi,
                    TonCuoiThanhTien = ttTonCuoi
                });
            }
        }

        // 5. Tính số dư Sổ Cái các TK Kho (152, 155, 156) đến endOfDay
        var tkKhoPrefixes = new[] { "152", "155", "156" };
        var tkKhoIds = await _context.TaiKhoans
            .Where(t => tkKhoPrefixes.Any(p => t.MaTaiKhoan.StartsWith(p)))
            .Select(t => t.Id)
            .ToListAsync();

        var voucherLines = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Where(c => c.ButToan != null &&
                        c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= endOfDay)
            .ToListAsync();

        var glDebit = voucherLines.Where(c => tkKhoIds.Contains(c.TaiKhoanNoId)).Sum(c => c.SoTien);
        var glCredit = voucherLines.Where(c => tkKhoIds.Contains(c.TaiKhoanCoId)).Sum(c => c.SoTien);
        var glBalance = glDebit - glCredit;

        string tenKho = "Tất cả kho";
        if (khoId.HasValue && khoId.Value > 0)
        {
            var k = await _context.Khos.FindAsync(khoId.Value);
            if (k != null) tenKho = $"{k.MaKho} - {k.TenKho}";
        }

        string tenChiNhanh = "Toàn công ty";
        if (branchId.HasValue && branchId.Value > 0)
        {
            var b = await _context.ChiNhanhs.FindAsync(branchId.Value);
            if (b != null) tenChiNhanh = b.TenChiNhanh;
        }

        return new BaoCaoNhapXuatTonViewModel
        {
            TuNgay = tuNgay,
            DenNgay = denNgay,
            SelectedKhoId = khoId,
            SelectedBranchId = branchId,
            TenKho = tenKho,
            TenChiNhanh = tenChiNhanh,
            Items = reportItems,
            SoDuSoCaiTkKho = glBalance
        };
    }

    public async Task<(bool Success, string? Message, int SoDongCapNhat, decimal TongChenhLech)> RecalculatePeriodWeightedAverageCostAsync(
        DateTime tuNgay, 
        DateTime denNgay, 
        long? khoId = null, 
        long? branchId = null)
    {
        var startOfDay = tuNgay.Date;
        var endOfDay = denNgay.Date.AddDays(1).AddTicks(-1);

        // 1. Kiểm tra khóa sổ kế toán
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh != null && !cauHinh.ChoPhepGhiSo(endOfDay))
        {
            return (false, $"Kỳ kế toán đến ngày {denNgay:dd/MM/yyyy} đã bị khóa sổ. Không thể tính lại giá xuất kho.", 0, 0);
        }

        // 2. Lấy danh sách các dòng phiếu xuất kho ĐÃ GHI SỔ trong kỳ
        var queryXuat = _context.ChiTietXuatKhos
            .Include(c => c.PhieuXuatKho)
            .Include(c => c.VatTuHangHoa)
            .Where(c => c.PhieuXuatKho != null &&
                        c.PhieuXuatKho.TrangThai == TrangThaiPhieuKho.DaGhiSo &&
                        c.PhieuXuatKho.NgayHachToan >= startOfDay &&
                        c.PhieuXuatKho.NgayHachToan <= endOfDay);

        if (khoId.HasValue && khoId.Value > 0)
        {
            queryXuat = queryXuat.Where(c => c.PhieuXuatKho!.KhoId == khoId.Value);
        }

        if (branchId.HasValue && branchId.Value > 0)
        {
            queryXuat = queryXuat.Where(c => c.PhieuXuatKho!.ChiNhanhId == branchId.Value);
        }

        var listXuat = await queryXuat.ToListAsync();
        if (listXuat.Count == 0)
        {
            return (true, "Không có phiếu xuất kho nào cần tính lại giá vốn trong khoảng thời gian này.", 0, 0);
        }

        // Nhóm các dòng xuất theo (KhoId, VatTuHangHoaId)
        var groupedXuat = listXuat.GroupBy(c => new { c.PhieuXuatKho!.KhoId, c.VatTuHangHoaId });
        int soDongCapNhat = 0;
        decimal tongChenhLech = 0m;

        foreach (var group in groupedXuat)
        {
            long currentKhoId = group.Key.KhoId;
            long vatTuId = group.Key.VatTuHangHoaId;

            // Tính tồn đầu kỳ
            var nhapTruocKy = await _context.ChiTietNhapKhos
                .Where(c => c.PhieuNhapKho != null &&
                            c.PhieuNhapKho.KhoId == currentKhoId &&
                            c.VatTuHangHoaId == vatTuId &&
                            c.PhieuNhapKho.TrangThai == TrangThaiPhieuKho.DaGhiSo &&
                            c.PhieuNhapKho.NgayHachToan < startOfDay)
                .Select(c => new { c.SoLuong, ThanhTien = c.ThanhTien + c.ChiPhiMuaHangPhanBo })
                .ToListAsync();

            var xuatTruocKy = await _context.ChiTietXuatKhos
                .Where(c => c.PhieuXuatKho != null &&
                            c.PhieuXuatKho.KhoId == currentKhoId &&
                            c.VatTuHangHoaId == vatTuId &&
                            c.PhieuXuatKho.TrangThai == TrangThaiPhieuKho.DaGhiSo &&
                            c.PhieuXuatKho.NgayHachToan < startOfDay)
                .Select(c => new { c.SoLuong, c.TienGiaVon })
                .ToListAsync();

            decimal slDauKy = nhapTruocKy.Sum(n => n.SoLuong) - xuatTruocKy.Sum(x => x.SoLuong);
            decimal tienDauKy = nhapTruocKy.Sum(n => n.ThanhTien) - xuatTruocKy.Sum(x => x.TienGiaVon);

            if (slDauKy < 0) slDauKy = 0;
            if (tienDauKy < 0) tienDauKy = 0;

            // Tính nhập trong kỳ (kèm chi phí mua hàng phân bổ)
            var nhapTrongKy = await _context.ChiTietNhapKhos
                .Where(c => c.PhieuNhapKho != null &&
                            c.PhieuNhapKho.KhoId == currentKhoId &&
                            c.VatTuHangHoaId == vatTuId &&
                            c.PhieuNhapKho.TrangThai == TrangThaiPhieuKho.DaGhiSo &&
                            c.PhieuNhapKho.NgayHachToan >= startOfDay &&
                            c.PhieuNhapKho.NgayHachToan <= endOfDay)
                .Select(c => new { c.SoLuong, ThanhTien = c.ThanhTien + c.ChiPhiMuaHangPhanBo })
                .ToListAsync();

            decimal slNhapTrongKy = nhapTrongKy.Sum(n => n.SoLuong);
            decimal tienNhapTrongKy = nhapTrongKy.Sum(n => n.ThanhTien);

            decimal tongSlKhaDung = slDauKy + slNhapTrongKy;
            decimal tongTienKhaDung = tienDauKy + tienNhapTrongKy;

            decimal donGiaBq = 0m;
            if (tongSlKhaDung > 0 && tongTienKhaDung > 0)
            {
                donGiaBq = Math.Round(tongTienKhaDung / tongSlKhaDung, 4);
            }

            // Áp đơn giá BQGQ cho toàn bộ dòng xuất của mặt hàng này tại kho
            foreach (var dongXuat in group)
            {
                decimal donGiaMoi = donGiaBq > 0 ? donGiaBq : dongXuat.DonGiaVon;
                decimal tienGiaVonMoi = Math.Round(dongXuat.SoLuong * donGiaMoi, 4);
                decimal chenhLech = tienGiaVonMoi - dongXuat.TienGiaVon;

                dongXuat.DonGiaVonCuoiKy = donGiaMoi;
                dongXuat.ChenhLechGiaVon = chenhLech;
                dongXuat.DonGiaVon = donGiaMoi;
                dongXuat.TienGiaVon = tienGiaVonMoi;

                tongChenhLech += chenhLech;
                soDongCapNhat++;
            }
        }

        // Cập nhật lại các bút toán xuất kho tương ứng (Nợ 632 / Có 1561)
        var phieuXuatIds = listXuat.Select(x => x.PhieuXuatKhoId).Distinct().ToList();
        var phieuXuats = await _context.PhieuXuatKhos
            .Include(p => p.ChiTietXuatKhos)
            .Include(p => p.ButToan)
                .ThenInclude(b => b!.ChiTietButToans)
            .Where(p => phieuXuatIds.Contains(p.Id))
            .ToListAsync();

        foreach (var px in phieuXuats)
        {
            px.TongTienGiaVon = px.ChiTietXuatKhos.Sum(c => c.TienGiaVon);

            if (px.ButToan != null)
            {
                px.ButToan.TongTien = px.TongTienGiaVon;
                px.ButToan.TongNo = px.TongTienGiaVon;
                px.ButToan.TongCo = px.TongTienGiaVon;

                var dong632 = px.ButToan.ChiTietButToans.FirstOrDefault();
                if (dong632 != null)
                {
                    dong632.SoTien = px.TongTienGiaVon;
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Tính lại giá vốn BQGQ kỳ {Tu:dd/MM/yyyy} - {Den:dd/MM/yyyy} thành công. Cập nhật {SoDong} dòng, chênh lệch {ChenhLech:N0} VNĐ.", tuNgay, denNgay, soDongCapNhat, tongChenhLech);

        return (true, $"Đã tính lại giá xuất kho thành công cho {soDongCapNhat} dòng xuất.", soDongCapNhat, tongChenhLech);
    }

    public async Task<PhieuDieuChuyenKho> TaoPhieuDieuChuyenKhoAsync(PhieuDieuChuyenKho phieu)
    {
        if (phieu.KhoXuatId == phieu.KhoNhapId)
        {
            throw new InvalidOperationException("Kho xuất và kho nhập không được trùng nhau.");
        }

        if (phieu.ChiTietDieuChuyens == null || !phieu.ChiTietDieuChuyens.Any())
        {
            throw new InvalidOperationException("Phiếu điều chuyển phải có ít nhất một dòng hàng hóa.");
        }

        // 1. Kiểm tra tồn kho tại Kho Xuất (Anti-Negative Stock)
        foreach (var item in phieu.ChiTietDieuChuyens)
        {
            await ValidateStockAvailabilityAsync(phieu.KhoXuatId, item.VatTuHangHoaId, item.SoLuong, phieu.NgayHachToan);
        }

        // 2. Tính tổng số lượng và giá trị
        phieu.TongSoLuong = phieu.ChiTietDieuChuyens.Sum(c => c.SoLuong);
        phieu.TongGiaTri = phieu.ChiTietDieuChuyens.Sum(c => c.ThanhTien);
        phieu.TrangThai = TrangThaiDieuChuyenKho.DaHoanThanh;

        // 3. Lấy tài khoản mặc định nếu chưa gán
        var tk1561 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1561")
                     ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "156")
                     ?? await _context.TaiKhoans.FirstAsync();

        foreach (var item in phieu.ChiTietDieuChuyens)
        {
            item.TaiKhoanNhapId ??= tk1561.Id;
            item.TaiKhoanXuatId ??= tk1561.Id;
        }

        // 4. Sinh Bút toán chuyển kho nếu tài khoản Nợ khác tài khoản Có (ví dụ: chuyển nguyên vật liệu 152 sang hàng hoá 1561)
        // Nếu cùng tài khoản (1561 -> 1561), nghiệp vụ ghi nhận biến động vật lý qua Phiếu Xuất/Phiếu Nhập và Sổ kho S10-DN, không phát sinh bút toán Nợ 1561/Có 1561
        var needsGlPosting = phieu.ChiTietDieuChuyens.Any(c => c.TaiKhoanNhapId != c.TaiKhoanXuatId);
        if (needsGlPosting)
        {
            var butToan = new ButToan
            {
                SoChungTu = $"PKT-{phieu.SoPhieu}",
                NgayHachToan = phieu.NgayHachToan,
                NgayChungTu = phieu.NgayDieuChuyen,
                SoChungTuGoc = phieu.SoPhieu,
                NgayChungTuGoc = phieu.NgayDieuChuyen,
                DienGiai = string.IsNullOrWhiteSpace(phieu.LyDoDieuChuyen)
                    ? $"Điều chuyển kho nội bộ {phieu.SoPhieu}"
                    : phieu.LyDoDieuChuyen,
                TrangThai = TrangThaiButToan.DaGhiSo
            };

            var lines = new List<ChiTietButToan>();
            int dongSo = 1;

            foreach (var item in phieu.ChiTietDieuChuyens.Where(c => c.TaiKhoanNhapId != c.TaiKhoanXuatId))
            {
                lines.Add(new ChiTietButToan
                {
                    DongSo = dongSo++,
                    TaiKhoanNoId = item.TaiKhoanNhapId!.Value,
                    TaiKhoanCoId = item.TaiKhoanXuatId!.Value,
                    SoTien = item.ThanhTien,
                    DienGiai = $"Điều chuyển hàng: {item.DonViTinh} sang kho nhập"
                });
            }

            butToan.ChiTietButToans = lines;
            var (ok, msg, createdBt) = await _butToanService.TaoMoiAsync(butToan);
            if (!ok || createdBt == null)
            {
                throw new InvalidOperationException($"Lỗi hạch toán sổ cái phiếu điều chuyển kho: {msg}");
            }

            phieu.ButToanId = createdBt.Id;
        }

        _context.PhieuDieuChuyenKhos.Add(phieu);

        // 5. Tự động sinh đồng bộ PhieuXuatKho và PhieuNhapKho nội bộ
        var px = new PhieuXuatKho
        {
            ChiNhanhId = phieu.ChiNhanhId,
            KhoId = phieu.KhoXuatId,
            SoPhieu = $"PXK-{phieu.SoPhieu}",
            NgayXuat = phieu.NgayDieuChuyen,
            NgayHachToan = phieu.NgayHachToan,
            LoaiXuatKho = LoaiXuatKho.SuDungNoiBo,
            DienGiai = phieu.LyDoDieuChuyen ?? "Xuất điều chuyển kho nội bộ",
            TongSoLuong = phieu.TongSoLuong,
            TongTienGiaVon = phieu.TongGiaTri,
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietXuatKhos = phieu.ChiTietDieuChuyens.Select(c => new ChiTietXuatKho
            {
                VatTuHangHoaId = c.VatTuHangHoaId,
                SoLuong = c.SoLuong,
                DonGiaVon = c.DonGiaVon,
                TienGiaVon = c.ThanhTien,
                TaiKhoanNoId = c.TaiKhoanNhapId!.Value,
                TaiKhoanCoId = c.TaiKhoanXuatId!.Value
            }).ToList()
        };
        _context.PhieuXuatKhos.Add(px);

        var pn = new PhieuNhapKho
        {
            ChiNhanhId = phieu.ChiNhanhId,
            KhoId = phieu.KhoNhapId,
            SoPhieu = $"PNK-{phieu.SoPhieu}",
            NgayNhap = phieu.NgayDieuChuyen,
            NgayHachToan = phieu.NgayHachToan,
            LoaiNhapKho = LoaiNhapKho.NhapKhac,
            DienGiai = phieu.LyDoDieuChuyen ?? "Nhập điều chuyển kho nội bộ",
            TongSoLuong = phieu.TongSoLuong,
            TongTienHang = phieu.TongGiaTri,
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = phieu.ChiTietDieuChuyens.Select(c => new ChiTietNhapKho
            {
                VatTuHangHoaId = c.VatTuHangHoaId,
                SoLuong = c.SoLuong,
                DonGia = c.DonGiaVon,
                ThanhTien = c.ThanhTien,
                TaiKhoanNoId = c.TaiKhoanNhapId!.Value,
                TaiKhoanCoId = c.TaiKhoanXuatId!.Value
            }).ToList()
        };
        _context.PhieuNhapKhos.Add(pn);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lập và ghi sổ thành công phiếu điều chuyển kho {SoPhieu}", phieu.SoPhieu);
        return phieu;
    }

    public async Task<bool> HuyPhieuDieuChuyenKhoAsync(long phieuDieuChuyenId)
    {
        var phieu = await _context.PhieuDieuChuyenKhos
            .Include(p => p.ChiTietDieuChuyens)
            .FirstOrDefaultAsync(p => p.Id == phieuDieuChuyenId);

        if (phieu == null) return false;
        if (phieu.TrangThai == TrangThaiDieuChuyenKho.DaHuy) return true;

        if (phieu.ButToanId.HasValue)
        {
            await _butToanService.XoaAsync(phieu.ButToanId.Value);
            phieu.ButToanId = null;
        }

        // Hủy phiếu xuất và nhập tương ứng
        var pxSoPhieu = $"PXK-{phieu.SoPhieu}";
        var pnSoPhieu = $"PNK-{phieu.SoPhieu}";

        var px = await _context.PhieuXuatKhos.FirstOrDefaultAsync(p => p.SoPhieu == pxSoPhieu);
        if (px != null) px.TrangThai = TrangThaiPhieuKho.DaHuy;

        var pn = await _context.PhieuNhapKhos.FirstOrDefaultAsync(p => p.SoPhieu == pnSoPhieu);
        if (pn != null) pn.TrangThai = TrangThaiPhieuKho.DaHuy;

        phieu.TrangThai = TrangThaiDieuChuyenKho.DaHuy;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã hủy phiếu điều chuyển kho {SoPhieu}", phieu.SoPhieu);
        return true;
    }

    public async Task<List<PhieuDieuChuyenKho>> LayDanhSachPhieuDieuChuyenAsync(DateTime? tuNgay = null, DateTime? denNgay = null, long? branchId = null)
    {
        var query = _context.PhieuDieuChuyenKhos
            .Include(p => p.KhoXuat)
            .Include(p => p.KhoNhap)
            .Include(p => p.ChiNhanh)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue && branchId.Value > 0)
        {
            query = query.Where(p => p.ChiNhanhId == branchId.Value);
        }

        if (tuNgay.HasValue)
        {
            query = query.Where(p => p.NgayDieuChuyen >= tuNgay.Value.Date);
        }

        if (denNgay.HasValue)
        {
            var endOfDay = denNgay.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.NgayDieuChuyen <= endOfDay);
        }

        return await query.OrderByDescending(p => p.NgayDieuChuyen).ThenByDescending(p => p.Id).ToListAsync();
    }

    public async Task<PhieuDieuChuyenKho?> LayChiTietPhieuDieuChuyenAsync(long id)
    {
        return await _context.PhieuDieuChuyenKhos
            .Include(p => p.KhoXuat)
            .Include(p => p.KhoNhap)
            .Include(p => p.ChiNhanh)
            .Include(p => p.ButToan)
            .Include(p => p.ChiTietDieuChuyens)
                .ThenInclude(c => c.VatTuHangHoa)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}

