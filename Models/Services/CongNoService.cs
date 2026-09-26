using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ quản lý công nợ, đối trừ chứng từ (Settlement Engine) và phân tích tuổi nợ (Aging)
/// </summary>
public class CongNoService : ICongNoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CongNoService> _logger;

    public CongNoService(AppDbContext context, ILogger<CongNoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool ThanhCong, string? ThongBao)> DoiTruHoaDonBanAsync(
        long hoaDonBanHangId,
        decimal soTien,
        long? butToanId = null,
        string? ghiChu = null)
    {
        var hoaDon = await _context.HoaDonBanHangs.FindAsync(hoaDonBanHangId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn bán hàng.");
        }

        if (soTien <= 0)
        {
            return (false, "Số tiền đối trừ phải lớn hơn 0.");
        }

        if (soTien > hoaDon.ConPhaiThu)
        {
            return (false, $"Số tiền đối trừ ({soTien:N0}) vượt quá số tiền còn nợ của hóa đơn ({hoaDon.ConPhaiThu:N0}).");
        }

        hoaDon.DaThuTien += soTien;

        var doiTru = new DoiTruCongNo
        {
            Loai = LoaiCongNo.PhaiThuKhachHang,
            DoiTuongId = hoaDon.KhachHangId,
            NgayDoiTru = DateTime.Today,
            HoaDonBanHangId = hoaDon.Id,
            ButToanId = butToanId,
            SoTienDoiTru = soTien,
            GhiChu = ghiChu ?? $"Thanh toán cho hóa đơn {hoaDon.SoHoaDon}"
        };

        await _context.DoiTruCongNos.AddAsync(doiTru);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đối trừ công nợ HĐ bán {SoHoaDon}: {SoTien:N0} VNĐ. Còn nợ: {ConNo:N0} VNĐ", hoaDon.SoHoaDon, soTien, hoaDon.ConPhaiThu);
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> DoiTruHoaDonMuaAsync(
        long hoaDonMuaHangId,
        decimal soTien,
        long? butToanId = null,
        string? ghiChu = null)
    {
        var hoaDon = await _context.HoaDonMuaHangs.FindAsync(hoaDonMuaHangId);
        if (hoaDon == null)
        {
            return (false, "Không tìm thấy hóa đơn mua hàng.");
        }

        if (soTien <= 0)
        {
            return (false, "Số tiền đối trừ phải lớn hơn 0.");
        }

        if (soTien > hoaDon.ConPhaiTra)
        {
            return (false, $"Số tiền đối trừ ({soTien:N0}) vượt quá số tiền còn phải trả ({hoaDon.ConPhaiTra:N0}).");
        }

        hoaDon.DaThanhToan += soTien;

        var doiTru = new DoiTruCongNo
        {
            Loai = LoaiCongNo.PhaiTraNhaCungCap,
            DoiTuongId = hoaDon.NhaCungCapId,
            NgayDoiTru = DateTime.Today,
            HoaDonMuaHangId = hoaDon.Id,
            ButToanId = butToanId,
            SoTienDoiTru = soTien,
            GhiChu = ghiChu ?? $"Thanh toán tiền hàng cho hóa đơn {hoaDon.SoHoaDon}"
        };

        await _context.DoiTruCongNos.AddAsync(doiTru);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đối trừ công nợ HĐ mua {SoHoaDon}: {SoTien:N0} VNĐ. Còn nợ: {ConNo:N0} VNĐ", hoaDon.SoHoaDon, soTien, hoaDon.ConPhaiTra);
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao, decimal DaDoiTru)> DoiTruFifoKhachHangAsync(
        long khachHangId,
        decimal tongTienThu,
        long? butToanId = null)
    {
        if (tongTienThu <= 0)
        {
            return (false, "Số tiền thu phải lớn hơn 0.", 0m);
        }

        var hoaDons = await _context.HoaDonBanHangs
            .Where(h => h.KhachHangId == khachHangId && (h.TongThanhToan - h.DaThuTien) > 0)
            .OrderBy(h => h.NgayHoaDon)
            .ThenBy(h => h.Id)
            .ToListAsync();

        if (hoaDons.Count == 0)
        {
            return (false, "Khách hàng không có hóa đơn nào còn nợ tiền.", 0m);
        }

        decimal conLai = tongTienThu;
        decimal tongDaDoiTru = 0m;

        foreach (var hd in hoaDons)
        {
            if (conLai <= 0) break;

            decimal soTienTru = Math.Min(conLai, hd.ConPhaiThu);
            hd.DaThuTien += soTienTru;
            conLai -= soTienTru;
            tongDaDoiTru += soTienTru;

            _context.DoiTruCongNos.Add(new DoiTruCongNo
            {
                Loai = LoaiCongNo.PhaiThuKhachHang,
                DoiTuongId = khachHangId,
                NgayDoiTru = DateTime.Today,
                HoaDonBanHangId = hd.Id,
                ButToanId = butToanId,
                SoTienDoiTru = soTienTru,
                GhiChu = $"Đối trừ FIFO tự động theo HĐ {hd.SoHoaDon}"
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Hoàn tất đối trừ FIFO khách hàng {KhId}. Tổng tiền đã đối trừ: {DaDoiTru:N0} VNĐ", khachHangId, tongDaDoiTru);

        return (true, null, tongDaDoiTru);
    }

    public async Task<List<AgingReportItem>> BaoCaoTuoiNoPhaiThuAsync(DateTime? mocThoiGian = null)
    {
        var moc = (mocThoiGian ?? DateTime.Today).Date;

        var hoaDons = await _context.HoaDonBanHangs
            .Include(h => h.KhachHang)
            .Where(h => (h.TongThanhToan - h.DaThuTien) > 0)
            .ToListAsync();

        var nhomTheoKhach = hoaDons.GroupBy(h => h.KhachHangId);
        var ketQua = new List<AgingReportItem>();

        foreach (var group in nhomTheoKhach)
        {
            var khach = group.First().KhachHang;
            var item = new AgingReportItem
            {
                DoiTuongId = group.Key,
                MaDoiTuong = khach?.MaDoiTuong ?? "KH-UNKNOWN",
                TenDoiTuong = khach?.TenDoiTuong ?? "Khách hàng vãng lai"
            };

            foreach (var hd in group)
            {
                var conNo = hd.ConPhaiThu;
                int soNgayQuaHan = (int)(moc - hd.HanThanhToan.Date).TotalDays;

                if (soNgayQuaHan <= 0)
                {
                    item.TrongHan += conNo;
                }
                else if (soNgayQuaHan <= 30)
                {
                    item.Tu1Den30Ngay += conNo;
                }
                else if (soNgayQuaHan <= 60)
                {
                    item.Tu31Den60Ngay += conNo;
                }
                else if (soNgayQuaHan <= 90)
                {
                    item.Tu61Den90Ngay += conNo;
                }
                else
                {
                    item.Tren90Ngay += conNo;
                }
            }

            ketQua.Add(item);
        }

        return ketQua.OrderByDescending(k => k.TongNo).ToList();
    }

    public async Task<List<AgingReportItem>> BaoCaoTuoiNoPhaiTraAsync(DateTime? mocThoiGian = null)
    {
        var moc = (mocThoiGian ?? DateTime.Today).Date;

        var hoaDons = await _context.HoaDonMuaHangs
            .Include(h => h.NhaCungCap)
            .Where(h => (h.TongThanhToan - h.DaThanhToan) > 0)
            .ToListAsync();

        var nhomTheoNcc = hoaDons.GroupBy(h => h.NhaCungCapId);
        var ketQua = new List<AgingReportItem>();

        foreach (var group in nhomTheoNcc)
        {
            var ncc = group.First().NhaCungCap;
            var item = new AgingReportItem
            {
                DoiTuongId = group.Key,
                MaDoiTuong = ncc?.MaDoiTuong ?? "NCC-UNKNOWN",
                TenDoiTuong = ncc?.TenDoiTuong ?? "Nhà cung cấp"
            };

            foreach (var hd in group)
            {
                var conNo = hd.ConPhaiTra;
                int soNgayQuaHan = (int)(moc - hd.HanThanhToan.Date).TotalDays;

                if (soNgayQuaHan <= 0)
                {
                    item.TrongHan += conNo;
                }
                else if (soNgayQuaHan <= 30)
                {
                    item.Tu1Den30Ngay += conNo;
                }
                else if (soNgayQuaHan <= 60)
                {
                    item.Tu31Den60Ngay += conNo;
                }
                else if (soNgayQuaHan <= 90)
                {
                    item.Tu61Den90Ngay += conNo;
                }
                else
                {
                    item.Tren90Ngay += conNo;
                }
            }

            ketQua.Add(item);
        }

        return ketQua.OrderByDescending(k => k.TongNo).ToList();
    }
}
