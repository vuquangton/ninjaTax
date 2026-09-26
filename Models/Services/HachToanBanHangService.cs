using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ xử lý Hóa đơn bán hàng, phát hành HĐĐT (QĐ 1450) và tự động sinh bút toán kép chuẩn TT99:
/// - Bút toán 1: Doanh thu (Nợ 131 / Có 511, Có 33311)
/// - Bút toán 2: Giá vốn kiêm xuất kho (Nợ 632 / Có 1561)
/// </summary>
public class HachToanBanHangService : IHachToanBanHangService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ILogger<HachToanBanHangService> _logger;

    public HachToanBanHangService(
        AppDbContext context,
        IButToanService butToanService,
        ILogger<HachToanBanHangService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _logger = logger;
    }

    public async Task<List<HoaDonBanHang>> LayDanhSachAsync()
    {
        return await _context.HoaDonBanHangs
            .Include(h => h.KhachHang)
            .Include(h => h.ChiTietBans)
                .ThenInclude(c => c.VatTuHangHoa)
            .OrderByDescending(h => h.NgayHachToan)
            .ThenByDescending(h => h.Id)
            .ToListAsync();
    }

    public async Task<HoaDonBanHang?> LayTheoIdAsync(long id)
    {
        return await _context.HoaDonBanHangs
            .Include(h => h.KhachHang)
            .Include(h => h.ButToanDoanhThu)
                .ThenInclude(b => b!.ChiTietButToans)
            .Include(h => h.ButToanGiaVon)
                .ThenInclude(b => b!.ChiTietButToans)
            .Include(h => h.ChiTietBans)
                .ThenInclude(c => c.VatTuHangHoa)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<(bool ThanhCong, string? ThongBao, HoaDonBanHang? HoaDon)> TaoMoiAsync(HoaDonBanHang hoaDon)
    {
        if (hoaDon.ChiTietBans == null || hoaDon.ChiTietBans.Count == 0)
        {
            return (false, "Hóa đơn bán hàng phải có ít nhất 1 dòng mặt hàng xuất bán.", null);
        }

        // Tự động sinh mã chứng từ nội bộ
        if (string.IsNullOrWhiteSpace(hoaDon.SoChungTu))
        {
            var countToday = await _context.HoaDonBanHangs.CountAsync(h => h.NgayHachToan.Date == hoaDon.NgayHachToan.Date);
            hoaDon.SoChungTu = $"BH{hoaDon.NgayHachToan:yyyyMMdd}-{(countToday + 1):D4}";
        }

        // Tự động cấp số hóa đơn nếu để trống
        if (string.IsNullOrWhiteSpace(hoaDon.SoHoaDon))
        {
            var maxSoHd = await _context.HoaDonBanHangs
                .Where(h => h.KyHieu == hoaDon.KyHieu)
                .CountAsync();
            hoaDon.SoHoaDon = (maxSoHd + 1).ToString("D8");
        }

        decimal tongTienHang = 0m;
        decimal tongTienChietKhau = 0m;
        decimal tongTienThue = 0m;

        int dongSo = 1;
        foreach (var dong in hoaDon.ChiTietBans)
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
        hoaDon.TrangThai = TrangThaiHddt.MoiTao;

        await _context.HoaDonBanHangs.AddAsync(hoaDon);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã lưu hóa đơn bán hàng: {SoChungTu}, Số HĐ: {SoHoaDon}", hoaDon.SoChungTu, hoaDon.SoHoaDon);
        return (true, null, hoaDon);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> PhatHanhHddtAsync(long hoaDonId)
    {
        var hoaDon = await LayTheoIdAsync(hoaDonId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn cần phát hành.");
        }

        if (hoaDon.TrangThai == TrangThaiHddt.CoQuanThueCapMa)
        {
            return (false, "Hóa đơn đã được cơ quan thuế cấp mã, không thể phát hành lại.");
        }

        // Mô phỏng quy trình ký số điện tử X.509
        var payload = $"{hoaDon.KHMauSo}|{hoaDon.KyHieu}|{hoaDon.SoHoaDon}|{hoaDon.TongThanhToan}|{DateTime.UtcNow:O}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        hoaDon.ChuKySo = Convert.ToBase64String(hashBytes);

        // Mô phỏng mã xác thực của Cơ quan Thuế cấp (24 ký tự hex)
        hoaDon.MaCoQuanThue = "CQT-" + Convert.ToHexString(hashBytes)[..20];
        hoaDon.TrangThai = TrangThaiHddt.CoQuanThueCapMa;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Phát hành HĐĐT thành công. Số HĐ: {SoHoaDon}, Mã CQT: {MaCqt}", hoaDon.SoHoaDon, hoaDon.MaCoQuanThue);
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao, ButToan? DoanhThu, ButToan? GiaVon)> GhiSoAsync(long hoaDonId)
    {
        var hoaDon = await LayTheoIdAsync(hoaDonId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn.", null, null);
        }

        if (hoaDon.ButToanDoanhThuId.HasValue)
        {
            return (false, "Hóa đơn này đã được ghi sổ kế toán.", null, null);
        }

        // 1. Sinh Bút toán Doanh thu
        var butToanDoanhThu = new ButToan
        {
            SoChungTu = $"PKT-BH-{hoaDon.SoChungTu}",
            NgayHachToan = hoaDon.NgayHachToan,
            NgayChungTu = hoaDon.NgayChungTu,
            SoChungTuGoc = $"{hoaDon.KyHieu}-{hoaDon.SoHoaDon}",
            NgayChungTuGoc = hoaDon.NgayHoaDon,
            DienGiai = string.IsNullOrWhiteSpace(hoaDon.DienGiai)
                ? $"Doanh thu bán hàng HĐĐT {hoaDon.KyHieu}-{hoaDon.SoHoaDon} cho {hoaDon.TenKhachHang}"
                : hoaDon.DienGiai,
            TrangThai = TrangThaiButToan.DaGhiSo
        };

        var chiTietDoanhThu = new List<ChiTietButToan>();
        int dongDt = 1;

        foreach (var dong in hoaDon.ChiTietBans)
        {
            decimal tienHang = dong.ThanhTien - dong.TienChietKhau;
            if (tienHang > 0)
            {
                chiTietDoanhThu.Add(new ChiTietButToan
                {
                    DongSo = dongDt++,
                    TaiKhoanNoId = dong.TaiKhoanNoId,
                    TaiKhoanCoId = dong.TaiKhoanDoanhThuId,
                    SoTien = tienHang,
                    DoiTuongId = hoaDon.KhachHangId,
                    DienGiai = $"Doanh thu: {dong.VatTuHangHoa?.TenVatTu ?? "Hàng hóa"}"
                });
            }

            if (dong.TienThueVat > 0)
            {
                chiTietDoanhThu.Add(new ChiTietButToan
                {
                    DongSo = dongDt++,
                    TaiKhoanNoId = dong.TaiKhoanNoId,
                    TaiKhoanCoId = dong.TaiKhoanThueId,
                    SoTien = dong.TienThueVat,
                    DoiTuongId = hoaDon.KhachHangId,
                    DienGiai = $"Thuế GTGT đầu ra {dong.ThueSuatVat}% theo HĐ {hoaDon.SoHoaDon}"
                });
            }
        }

        butToanDoanhThu.ChiTietButToans = chiTietDoanhThu;
        var (tcDt, loiDt, ketQuaDt) = await _butToanService.TaoMoiAsync(butToanDoanhThu);
        if (!tcDt || ketQuaDt == null)
        {
            return (false, $"Lỗi ghi sổ doanh thu: {loiDt}", null, null);
        }

        hoaDon.ButToanDoanhThuId = ketQuaDt.Id;

        // 2. Sinh Bút toán Giá vốn (nếu kiêm xuất kho)
        ButToan? ketQuaGiaVon = null;
        if (hoaDon.BanHangKiemXuatKho)
        {
            var chiTietGiaVon = new List<ChiTietButToan>();
            int dongGv = 1;

            foreach (var dong in hoaDon.ChiTietBans)
            {
                if (dong.TienGiaVon > 0 && dong.TaiKhoanGiaVonId.HasValue && dong.TaiKhoanKhoId.HasValue)
                {
                    chiTietGiaVon.Add(new ChiTietButToan
                    {
                        DongSo = dongGv++,
                        TaiKhoanNoId = dong.TaiKhoanGiaVonId.Value,
                        TaiKhoanCoId = dong.TaiKhoanKhoId.Value,
                        SoTien = dong.TienGiaVon,
                        DoiTuongId = hoaDon.KhachHangId,
                        DienGiai = $"Giá vốn xuất kho: {dong.VatTuHangHoa?.TenVatTu ?? "Hàng bán"}"
                    });
                }
            }

            if (chiTietGiaVon.Count > 0)
            {
                var butToanGiaVon = new ButToan
                {
                    SoChungTu = $"PXK-{hoaDon.SoChungTu}",
                    NgayHachToan = hoaDon.NgayHachToan,
                    NgayChungTu = hoaDon.NgayChungTu,
                    SoChungTuGoc = $"{hoaDon.KyHieu}-{hoaDon.SoHoaDon}",
                    NgayChungTuGoc = hoaDon.NgayHoaDon,
                    DienGiai = $"Giá vốn xuất kho theo HĐĐT {hoaDon.KyHieu}-{hoaDon.SoHoaDon}",
                    TrangThai = TrangThaiButToan.DaGhiSo,
                    ChiTietButToans = chiTietGiaVon
                };

                var (tcGv, loiGv, kqGv) = await _butToanService.TaoMoiAsync(butToanGiaVon);
                if (tcGv && kqGv != null)
                {
                    ketQuaGiaVon = kqGv;
                    hoaDon.ButToanGiaVonId = kqGv.Id;
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Ghi sổ thành công HĐ bán {SoHoaDon} -> Doanh thu {DtId}, Giá vốn {GvId}", hoaDon.SoHoaDon, hoaDon.ButToanDoanhThuId, hoaDon.ButToanGiaVonId);

        return (true, null, ketQuaDt, ketQuaGiaVon);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> BoGhiSoAsync(long hoaDonId)
    {
        var hoaDon = await LayTheoIdAsync(hoaDonId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn.");
        }

        if (hoaDon.ButToanDoanhThuId.HasValue)
        {
            await _butToanService.BoGhiSoAsync(hoaDon.ButToanDoanhThuId.Value);
        }

        if (hoaDon.ButToanGiaVonId.HasValue)
        {
            await _butToanService.BoGhiSoAsync(hoaDon.ButToanGiaVonId.Value);
        }

        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> HuyHoaDonAsync(long hoaDonId, string lyDo)
    {
        var hoaDon = await LayTheoIdAsync(hoaDonId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn.");
        }

        if (hoaDon.TrangThai == TrangThaiHddt.DaHuy)
        {
            return (false, "Hóa đơn này đã được hủy trước đó.");
        }

        // Bỏ ghi sổ nếu đã ghi sổ
        await BoGhiSoAsync(hoaDonId);

        hoaDon.TrangThai = TrangThaiHddt.DaHuy;
        hoaDon.DienGiai += $" [ĐÃ HỦY: {lyDo}]";
        await _context.SaveChangesAsync();

        _logger.LogWarning("Đã hủy hóa đơn điện tử {SoHoaDon}. Lý do: {LyDo}", hoaDon.SoHoaDon, lyDo);
        return (true, null);
    }
}
