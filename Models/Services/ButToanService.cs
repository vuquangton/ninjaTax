using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ xử lý nghiệp vụ hạch toán bút toán (Core General Ledger).
/// Tuân thủ nghiêm ngặt các nguyên tắc kế toán theo Thông tư TT99:
/// 1. Nguyên tắc cân đối bút toán kép: Tổng phát sinh Nợ == Tổng phát sinh Có (TongNo == TongCo).
/// 2. Bắt buộc theo dõi nguồn gốc: Số chứng từ gốc (SoChungTuGoc) và Ngày chứng từ gốc (NgayChungTuGoc).
/// 3. Tài khoản 911 dành riêng cho chứng từ kết chuyển cuối kỳ (PKT-KC-), không sử dụng cho chứng từ thông thường.
/// 4. Không được phép ghi sổ các tài khoản đã ngừng hoạt động hoặc tài khoản tổng hợp mẹ.
/// </summary>
public class ButToanService : IButToanService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ButToanService> _logger;

    public ButToanService(AppDbContext context, ILogger<ButToanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ButToan>> LayDanhSachAsync()
    {
        _logger.LogInformation("Lấy danh sách sổ nhật ký chung bút toán");
        return await _context.ButToans
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanNo)
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanCo)
            .OrderByDescending(b => b.NgayHachToan)
            .ThenByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<ButToan?> LayTheoIdAsync(long id)
    {
        _logger.LogInformation("Tìm kiếm bút toán với Id: {Id}", id);
        return await _context.ButToans
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanNo)
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanCo)
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.DoiTuong)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public (bool HopLe, string? Loi) KiemTraHopLe(ButToan butToan)
    {
        // 1. Kiểm tra chứng từ gốc theo TT99
        if (string.IsNullOrWhiteSpace(butToan.SoChungTuGoc))
        {
            return (false, "Theo quy định Thông tư TT99, Số chứng từ gốc (SoChungTuGoc) là trường bắt buộc để phục vụ kiểm toán.");
        }

        if (butToan.NgayChungTuGoc == default)
        {
            return (false, "Ngày chứng từ gốc không hợp lệ.");
        }

        // 2. Kiểm tra danh sách dòng chi tiết
        if (butToan.ChiTietButToans == null || butToan.ChiTietButToans.Count == 0)
        {
            return (false, "Bút toán phải có ít nhất một dòng định khoản.");
        }

        decimal tongNo = 0m;
        decimal tongCo = 0m;

        foreach (var chiTiet in butToan.ChiTietButToans)
        {
            if (chiTiet.SoTien <= 0m)
            {
                return (false, $"Dòng {chiTiet.DongSo}: Số tiền định khoản phải lớn hơn 0.");
            }

            if (chiTiet.TaiKhoanNoId == chiTiet.TaiKhoanCoId)
            {
                return (false, $"Dòng {chiTiet.DongSo}: Tài khoản Nợ và Tài khoản Có không được trùng nhau.");
            }

            // Mỗi dòng định khoản kép có Nợ = Có = SoTien
            tongNo += chiTiet.SoTien;
            tongCo += chiTiet.SoTien;
        }

        // 3. Kiểm tra tính cân đối kép
        if (tongNo != tongCo || tongNo <= 0m)
        {
            return (false, $"Bút toán không cân đối: Tổng Nợ ({tongNo:N2}) khác Tổng Có ({tongCo:N2}).");
        }

        butToan.TongNo = tongNo;
        butToan.TongCo = tongCo;
        butToan.TongTien = tongNo;

        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao, ButToan? ButToan)> TaoMoiAsync(ButToan butToan)
    {
        _logger.LogInformation("Khởi tạo bút toán mới số: {SoChungTu}", butToan.SoChungTu);

        // Kiểm tra hợp lệ cơ bản
        var (hopLe, loi) = KiemTraHopLe(butToan);
        if (!hopLe)
        {
            _logger.LogWarning("Xác thực bút toán thất bại: {Loi}", loi);
            return (false, loi, null);
        }

        // Kiểm tra Khóa sổ kế toán (Phase 6)
        var cauHinhKeToan = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinhKeToan != null && !cauHinhKeToan.ChoPhepGhiSo(butToan.NgayHachToan))
        {
            return (false, $"Vi phạm khóa sổ kế toán: Kỳ kế toán đã khóa sổ đến hết ngày {cauHinhKeToan.NgayKhoaSo:dd/MM/yyyy}. Không thể lập chứng từ hạch toán ngày {butToan.NgayHachToan:dd/MM/yyyy}.", null);
        }

        // Kiểm tra tài khoản trong CSDL: cấm TK 911 và kiểm tra trạng thái hoạt động
        var taiKhoanIds = butToan.ChiTietButToans
            .SelectMany(c => new[] { c.TaiKhoanNoId, c.TaiKhoanCoId })
            .Distinct()
            .ToList();

        var taiKhoans = await _context.TaiKhoans
            .Where(t => taiKhoanIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id);

        foreach (var chiTiet in butToan.ChiTietButToans)
        {
            if (!taiKhoans.TryGetValue(chiTiet.TaiKhoanNoId, out var tkNo) || !tkNo.DangHoatDong)
            {
                return (false, $"Tài khoản Nợ (Id: {chiTiet.TaiKhoanNoId}) không tồn tại hoặc đã ngừng hoạt động.", null);
            }

            if (!taiKhoans.TryGetValue(chiTiet.TaiKhoanCoId, out var tkCo) || !tkCo.DangHoatDong)
            {
                return (false, $"Tài khoản Có (Id: {chiTiet.TaiKhoanCoId}) không tồn tại hoặc đã ngừng hoạt động.", null);
            }

            // Quy định TT99: TK 911 chỉ dùng cho chứng từ kết chuyển cuối kỳ (PKT-KC)
            if ((tkNo.MaTaiKhoan.StartsWith("911") || tkCo.MaTaiKhoan.StartsWith("911")) &&
                !butToan.SoChungTu.StartsWith("PKT-KC"))
            {
                _logger.LogWarning("Tài khoản 911 chỉ được sử dụng trong quy trình kết chuyển cuối kỳ: {SoChungTu}", butToan.SoChungTu);
                return (false, "Quy định TT99: Tài khoản 911 chỉ được sử dụng trong nghiệp vụ kết chuyển cuối kỳ.", null);
            }

            // Không cho phép hạch toán vào tài khoản tổng hợp nếu có tài khoản con
            if (tkNo.LaTaiKhoanSoCai && await _context.TaiKhoans.AnyAsync(t => t.TaiKhoanMeId == tkNo.Id))
            {
                return (false, $"Tài khoản {tkNo.MaTaiKhoan} là tài khoản tổng hợp, vui lòng chọn tài khoản chi tiết cấp con.", null);
            }

            if (tkCo.LaTaiKhoanSoCai && await _context.TaiKhoans.AnyAsync(t => t.TaiKhoanMeId == tkCo.Id))
            {
                return (false, $"Tài khoản {tkCo.MaTaiKhoan} là tài khoản tổng hợp, vui lòng chọn tài khoản chi tiết cấp con.", null);
            }
        }

        // Tự động sinh số chứng từ nếu chưa có
        if (string.IsNullOrWhiteSpace(butToan.SoChungTu))
        {
            var countToday = await _context.ButToans.CountAsync(b => b.NgayHachToan.Date == butToan.NgayHachToan.Date);
            butToan.SoChungTu = $"PKT{butToan.NgayHachToan:yyyyMMdd}-{(countToday + 1):D4}";
        }

        await _context.ButToans.AddAsync(butToan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Lưu thành công bút toán Id: {Id}, Số chứng từ: {SoChungTu}", butToan.Id, butToan.SoChungTu);
        return (true, null, butToan);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> GhiSoAsync(long id)
    {
        var butToan = await LayTheoIdAsync(id);
        if (butToan == null)
        {
            return (false, "Không tìm thấy chứng từ cần ghi sổ.");
        }

        if (butToan.TrangThai == TrangThaiButToan.DaGhiSo)
        {
            return (false, "Chứng từ đã ở trạng thái Ghi sổ.");
        }

        // Kiểm tra Khóa sổ kế toán (Phase 6)
        var cauHinhKeToan = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinhKeToan != null && !cauHinhKeToan.ChoPhepGhiSo(butToan.NgayHachToan))
        {
            return (false, $"Vi phạm khóa sổ kế toán: Kỳ kế toán đã khóa sổ đến hết ngày {cauHinhKeToan.NgayKhoaSo:dd/MM/yyyy}. Không thể ghi sổ chứng từ ngày {butToan.NgayHachToan:dd/MM/yyyy}.");
        }

        var (hopLe, loi) = KiemTraHopLe(butToan);
        if (!hopLe)
        {
            return (false, $"Không thể ghi sổ: {loi}");
        }

        butToan.TrangThai = TrangThaiButToan.DaGhiSo;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã ghi sổ chứng từ Id: {Id}, Số: {SoChungTu}", butToan.Id, butToan.SoChungTu);
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> BoGhiSoAsync(long id)
    {
        var butToan = await _context.ButToans.FindAsync(id);
        if (butToan == null)
        {
            return (false, "Không tìm thấy chứng từ cần bỏ ghi sổ.");
        }

        if (butToan.TrangThai != TrangThaiButToan.DaGhiSo)
        {
            return (false, "Chứng từ chưa được ghi sổ.");
        }

        // Kiểm tra Khóa sổ kế toán (Phase 6)
        var cauHinhKeToan = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinhKeToan != null && !cauHinhKeToan.ChoPhepGhiSo(butToan.NgayHachToan))
        {
            return (false, $"Vi phạm khóa sổ kế toán: Kỳ kế toán đã khóa sổ đến hết ngày {cauHinhKeToan.NgayKhoaSo:dd/MM/yyyy}. Không thể bỏ ghi sổ chứng từ ngày {butToan.NgayHachToan:dd/MM/yyyy}.");
        }

        butToan.TrangThai = TrangThaiButToan.ChuaGhiSo;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã bỏ ghi sổ chứng từ Id: {Id}, Số: {SoChungTu}", butToan.Id, butToan.SoChungTu);
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id)
    {
        var butToan = await _context.ButToans.FindAsync(id);
        if (butToan == null)
        {
            return (false, "Không tìm thấy chứng từ cần xóa.");
        }

        // Kiểm tra Khóa sổ kế toán (Phase 6)
        var cauHinhKeToan = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinhKeToan != null && !cauHinhKeToan.ChoPhepGhiSo(butToan.NgayHachToan))
        {
            return (false, $"Vi phạm khóa sổ kế toán: Kỳ kế toán đã khóa sổ đến hết ngày {cauHinhKeToan.NgayKhoaSo:dd/MM/yyyy}. Không thể xóa chứng từ ngày {butToan.NgayHachToan:dd/MM/yyyy}.");
        }

        if (butToan.TrangThai == TrangThaiButToan.DaGhiSo)
        {
            return (false, "Không thể xóa chứng từ đã ghi sổ. Vui lòng bỏ ghi sổ trước khi xóa.");
        }

        _context.ButToans.Remove(butToan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã xóa bút toán Id: {Id}", id);
        return (true, null);
    }

    public async Task<(List<TaiKhoan> TaiKhoans, List<DoiTuong> DoiTuongs)> LayDanhMucTaoButToanAsync()
    {
        var taiKhoans = await _context.TaiKhoans
            .Where(t => t.DangHoatDong && !t.MaTaiKhoan.StartsWith("911") && !t.LaTaiKhoanSoCai)
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();

        var doiTuongs = await _context.DoiTuongs
            .Where(d => d.DangHoatDong)
            .OrderBy(d => d.MaDoiTuong)
            .ToListAsync();

        return (taiKhoans, doiTuongs);
    }
}
