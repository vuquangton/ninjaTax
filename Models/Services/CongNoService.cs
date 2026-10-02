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
                else if (soNgayQuaHan <= 180)
                {
                    item.Tu91Den180Ngay += conNo;
                }
                else if (soNgayQuaHan <= 360)
                {
                    item.Tu181Den360Ngay += conNo;
                }
                else if (soNgayQuaHan <= 720)
                {
                    item.Tu1Den2Nam += conNo;
                }
                else if (soNgayQuaHan <= 1080)
                {
                    item.Tu2Den3Nam += conNo;
                }
                else
                {
                    item.Tren3Nam += conNo;
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
                else if (soNgayQuaHan <= 180)
                {
                    item.Tu91Den180Ngay += conNo;
                }
                else if (soNgayQuaHan <= 360)
                {
                    item.Tu181Den360Ngay += conNo;
                }
                else if (soNgayQuaHan <= 720)
                {
                    item.Tu1Den2Nam += conNo;
                }
                else if (soNgayQuaHan <= 1080)
                {
                    item.Tu2Den3Nam += conNo;
                }
                else
                {
                    item.Tren3Nam += conNo;
                }
            }

            ketQua.Add(item);
        }

        return ketQua.OrderByDescending(k => k.TongNo).ToList();
    }

    public async Task<(bool ThanhCong, string? ThongBao, ButToan? ButToan)> BuTruCongNoHaiChieuAsync(
        long doiTuongId,
        decimal soTien,
        DateTime ngayHachToan,
        string? ghiChu = null)
    {
        if (soTien <= 0)
        {
            return (false, "Số tiền bù trừ công nợ phải lớn hơn 0.", null);
        }

        var doiTuong = await _context.DoiTuongs.FindAsync(doiTuongId);
        if (doiTuong == null)
        {
            return (false, "Không tìm thấy đối tượng công nợ.", null);
        }

        // Kiểm tra khóa sổ kế toán
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh?.NgayKhoaSo.HasValue == true && ngayHachToan.Date <= cauHinh.NgayKhoaSo.Value.Date)
        {
            return (false, $"Kỳ kế toán đã khóa sổ đến ngày {cauHinh.NgayKhoaSo:dd/MM/yyyy}. Không thể bù trừ.", null);
        }

        // Lấy tài khoản 331 (Nợ) và 131 (Có)
        var tk331 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "331");
        var tk131 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "131");
        if (tk331 == null || tk131 == null)
        {
            return (false, "Hệ thống thiếu tài khoản 331 hoặc 131 trong danh mục.", null);
        }

        var chiNhanh = await _context.ChiNhanhs.FirstOrDefaultAsync(c => c.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh) 
                       ?? await _context.ChiNhanhs.FirstOrDefaultAsync();
        if (chiNhanh == null)
        {
            return (false, "Hệ thống chưa thiết lập chi nhánh mặc định.", null);
        }

        // Bù trừ FIFO Hóa đơn bán (131)
        var (okBan, msgBan, daDoiTruBan) = await DoiTruFifoKhachHangAsync(doiTuongId, soTien);
        if (!okBan || daDoiTruBan < soTien)
        {
            return (false, msgBan ?? $"Công nợ phải thu của {doiTuong.TenDoiTuong} không đủ {soTien:N0} để bù trừ.", null);
        }

        // Bù trừ FIFO Hóa đơn mua (331)
        var hoaDonMuas = await _context.HoaDonMuaHangs
            .Where(h => h.NhaCungCapId == doiTuongId && (h.TongThanhToan - h.DaThanhToan) > 0)
            .OrderBy(h => h.NgayHoaDon)
            .ThenBy(h => h.Id)
            .ToListAsync();

        decimal conLaiMua = soTien;
        foreach (var hdm in hoaDonMuas)
        {
            if (conLaiMua <= 0) break;
            decimal tru = Math.Min(conLaiMua, hdm.ConPhaiTra);
            hdm.DaThanhToan += tru;
            conLaiMua -= tru;

            _context.DoiTruCongNos.Add(new DoiTruCongNo
            {
                Loai = LoaiCongNo.PhaiTraNhaCungCap,
                DoiTuongId = doiTuongId,
                NgayDoiTru = ngayHachToan.Date,
                HoaDonMuaHangId = hdm.Id,
                SoTienDoiTru = tru,
                GhiChu = ghiChu ?? $"Bù trừ hai chiều AR/AP đối tượng {doiTuong.MaDoiTuong}"
            });
        }

        if (conLaiMua > 0)
        {
            return (false, $"Công nợ phải trả của {doiTuong.TenDoiTuong} không đủ {soTien:N0} để bù trừ.", null);
        }

        // Tạo bút toán hạch toán bù trừ Nợ 331 / Có 131
        var soChungTu = $"BTCN-{ngayHachToan:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        var butToan = new ButToan
        {
            SoChungTu = soChungTu,
            NgayHachToan = ngayHachToan.Date,
            NgayChungTu = ngayHachToan.Date,
            SoChungTuGoc = soChungTu,
            NgayChungTuGoc = ngayHachToan.Date,
            DienGiai = ghiChu ?? $"Bù trừ công nợ hai chiều Nợ 331 / Có 131 đối tượng {doiTuong.MaDoiTuong} - {doiTuong.TenDoiTuong}",
            TongTien = soTien,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DongSo = 1,
                    TaiKhoanNoId = tk331.Id,
                    TaiKhoanCoId = tk131.Id,
                    SoTien = soTien,
                    DienGiai = $"Bù trừ công nợ hai chiều đối tượng {doiTuong.MaDoiTuong}",
                    DoiTuongId = doiTuongId
                }
            }
        };

        await _context.ButToans.AddAsync(butToan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã bù trừ công nợ hai chiều Nợ 331 / Có 131 đối tượng {DoiTuong}: {SoTien:N0} VNĐ", doiTuong.MaDoiTuong, soTien);
        return (true, null, butToan);
    }
}
