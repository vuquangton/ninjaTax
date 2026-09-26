using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ xử lý hóa đơn mua hàng và tự động sinh bút toán định khoản chuẩn TT99:
/// - Nợ 152 / 1561: Giá mua chưa thuế (đã trừ chiết khấu)
/// - Nợ 1331: Thuế GTGT đầu vào
/// - Có 331 (hoặc 1111/1121): Tổng giá thanh toán
/// Ghi nhận nguồn gốc chứng từ bắt buộc (SoChungTuGoc, NgayChungTuGoc)
/// </summary>
public class HachToanMuaHangService : IHachToanMuaHangService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ILogger<HachToanMuaHangService> _logger;

    public HachToanMuaHangService(
        AppDbContext context,
        IButToanService butToanService,
        ILogger<HachToanMuaHangService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _logger = logger;
    }

    public async Task<List<HoaDonMuaHang>> LayDanhSachAsync()
    {
        return await _context.HoaDonMuaHangs
            .Include(h => h.NhaCungCap)
            .Include(h => h.ChiTietHangs)
                .ThenInclude(c => c.VatTuHangHoa)
            .OrderByDescending(h => h.NgayHachToan)
            .ThenByDescending(h => h.Id)
            .ToListAsync();
    }

    public async Task<HoaDonMuaHang?> LayTheoIdAsync(long id)
    {
        return await _context.HoaDonMuaHangs
            .Include(h => h.NhaCungCap)
            .Include(h => h.ButToan)
                .ThenInclude(b => b!.ChiTietButToans)
            .Include(h => h.ChiTietHangs)
                .ThenInclude(c => c.VatTuHangHoa)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<(bool ThanhCong, string? ThongBao, HoaDonMuaHang? HoaDon)> TaoMoiAsync(HoaDonMuaHang hoaDon)
    {
        if (hoaDon.ChiTietHangs == null || hoaDon.ChiTietHangs.Count == 0)
        {
            return (false, "Hóa đơn mua hàng phải có ít nhất 1 dòng mặt hàng.", null);
        }

        if (string.IsNullOrWhiteSpace(hoaDon.SoHoaDon))
        {
            return (false, "Theo quy định hóa đơn điện tử, Số hóa đơn không được để trống.", null);
        }

        // Tự động sinh mã chứng từ nội bộ
        if (string.IsNullOrWhiteSpace(hoaDon.SoChungTu))
        {
            var countToday = await _context.HoaDonMuaHangs.CountAsync(h => h.NgayHachToan.Date == hoaDon.NgayHachToan.Date);
            hoaDon.SoChungTu = $"MH{hoaDon.NgayHachToan:yyyyMMdd}-{(countToday + 1):D4}";
        }

        // Tính toán lại tổng tiền từ các dòng chi tiết
        decimal tongTienHang = 0m;
        decimal tongTienChietKhau = 0m;
        decimal tongTienThue = 0m;

        int dongSo = 1;
        foreach (var dong in hoaDon.ChiTietHangs)
        {
            dong.DongSo = dongSo++;
            dong.ThanhTien = dong.SoLuong * dong.DonGia;
            if (dong.TiLeChietKhau > 0 && dong.TienChietKhau == 0)
            {
                dong.TienChietKhau = dong.ThanhTien * dong.TiLeChietKhau / 100m;
            }

            if (dong.ThueSuatVat > 0 && dong.TienThueVat == 0)
            {
                dong.TienThueVat = (dong.ThanhTien - dong.TienChietKhau) * dong.ThueSuatVat / 100m;
            }

            tongTienHang += dong.ThanhTien;
            tongTienChietKhau += dong.TienChietKhau;
            tongTienThue += dong.TienThueVat;
        }

        hoaDon.TongTienHang = tongTienHang;
        hoaDon.TongTienChietKhau = tongTienChietKhau;
        hoaDon.TongTienThueVat = tongTienThue;
        hoaDon.TongThanhToan = tongTienHang - tongTienChietKhau + tongTienThue;
        hoaDon.TrangThai = TrangThaiHoaDonMua.DaNhanHoaDon;

        await _context.HoaDonMuaHangs.AddAsync(hoaDon);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã lưu hóa đơn mua hàng: {SoChungTu}, Số HĐĐT: {SoHoaDon}", hoaDon.SoChungTu, hoaDon.SoHoaDon);
        return (true, null, hoaDon);
    }

    public async Task<(bool ThanhCong, string? ThongBao, ButToan? ButToan)> GhiSoAsync(long hoaDonId)
    {
        var hoaDon = await LayTheoIdAsync(hoaDonId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn mua hàng.", null);
        }

        if (hoaDon.TrangThai == TrangThaiHoaDonMua.DaGhiSo)
        {
            return (false, "Hóa đơn mua hàng đã được ghi sổ trước đó.", null);
        }

        // Tự động xây dựng Bút toán Sổ Cái GL
        var butToan = new ButToan
        {
            SoChungTu = $"PKT-MH-{hoaDon.SoChungTu}",
            NgayHachToan = hoaDon.NgayHachToan,
            NgayChungTu = hoaDon.NgayChungTu,
            SoChungTuGoc = $"{hoaDon.KyHieuHoaDon}-{hoaDon.SoHoaDon}",
            NgayChungTuGoc = hoaDon.NgayHoaDon,
            DienGiai = string.IsNullOrWhiteSpace(hoaDon.DienGiai)
                ? $"Mua hàng theo HĐĐT {hoaDon.KyHieuHoaDon}-{hoaDon.SoHoaDon} từ {hoaDon.TenNCC}"
                : hoaDon.DienGiai,
            TrangThai = TrangThaiButToan.DaGhiSo
        };

        var chiTietButToans = new List<ChiTietButToan>();
        int dongSo = 1;

        // 1. Các dòng tiền hàng (Nợ TK Kho 152/156, Có TK 331)
        foreach (var hang in hoaDon.ChiTietHangs)
        {
            decimal tienSauChietKhau = hang.ThanhTien - hang.TienChietKhau;
            if (tienSauChietKhau > 0)
            {
                chiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = dongSo++,
                    TaiKhoanNoId = hang.TaiKhoanNoId,
                    TaiKhoanCoId = hang.TaiKhoanCoId,
                    SoTien = tienSauChietKhau,
                    DoiTuongId = hoaDon.NhaCungCapId,
                    DienGiai = $"Tiền hàng: {hang.VatTuHangHoa?.TenVatTu ?? "Hàng mua"}"
                });
            }

            // 2. Dòng thuế GTGT (Nợ TK 1331, Có TK 331)
            if (hang.TienThueVat > 0)
            {
                chiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = dongSo++,
                    TaiKhoanNoId = hang.TaiKhoanThueId,
                    TaiKhoanCoId = hang.TaiKhoanCoId,
                    SoTien = hang.TienThueVat,
                    DoiTuongId = hoaDon.NhaCungCapId,
                    DienGiai = $"Thuế GTGT {hang.ThueSuatVat}% theo HĐ {hoaDon.SoHoaDon}"
                });
            }
        }

        butToan.ChiTietButToans = chiTietButToans;

        var (thanhCong, loi, ketQuaButToan) = await _butToanService.TaoMoiAsync(butToan);
        if (!thanhCong || ketQuaButToan == null)
        {
            _logger.LogError("Lỗi khi tự động sinh bút toán mua hàng: {Loi}", loi);
            return (false, $"Không thể tự động sinh bút toán: {loi}", null);
        }

        // Liên kết Bút toán với Hóa đơn mua
        hoaDon.ButToanId = ketQuaButToan.Id;
        hoaDon.TrangThai = TrangThaiHoaDonMua.DaGhiSo;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tự động ghi sổ thành công HĐ mua {SoHoaDon} -> Bút toán {SoChungTu}", hoaDon.SoHoaDon, ketQuaButToan.SoChungTu);
        return (true, null, ketQuaButToan);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> BoGhiSoAsync(long hoaDonId)
    {
        var hoaDon = await LayTheoIdAsync(hoaDonId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn.");
        }

        if (hoaDon.TrangThai != TrangThaiHoaDonMua.DaGhiSo)
        {
            return (false, "Hóa đơn chưa ở trạng thái ghi sổ.");
        }

        if (hoaDon.ButToanId.HasValue)
        {
            await _butToanService.BoGhiSoAsync(hoaDon.ButToanId.Value);
        }

        hoaDon.TrangThai = TrangThaiHoaDonMua.DaNhanHoaDon;
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long hoaDonId)
    {
        var hoaDon = await LayTheoIdAsync(hoaDonId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn cần xóa.");
        }

        if (hoaDon.TrangThai == TrangThaiHoaDonMua.DaGhiSo)
        {
            return (false, "Không thể xóa hóa đơn đã ghi sổ. Vui lòng bỏ ghi sổ trước.");
        }

        if (hoaDon.ButToanId.HasValue)
        {
            await _butToanService.XoaAsync(hoaDon.ButToanId.Value);
        }

        _context.HoaDonMuaHangs.Remove(hoaDon);
        await _context.SaveChangesAsync();

        return (true, null);
    }
}
