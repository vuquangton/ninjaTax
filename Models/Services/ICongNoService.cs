using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dòng báo cáo tuổi nợ công nợ chi tiết (Aging Report)
/// </summary>
public class AgingReportItem
{
    public long DoiTuongId { get; set; }
    public string MaDoiTuong { get; set; } = string.Empty;
    public string TenDoiTuong { get; set; } = string.Empty;
    public decimal TrongHan { get; set; }
    public decimal Tu1Den30Ngay { get; set; }
    public decimal Tu31Den60Ngay { get; set; }
    public decimal Tu61Den90Ngay { get; set; }
    public decimal Tu91Den180Ngay { get; set; }
    public decimal Tu181Den360Ngay { get; set; }
    public decimal Tu1Den2Nam { get; set; }
    public decimal Tu2Den3Nam { get; set; }
    public decimal Tren3Nam { get; set; }
    public decimal Tren90Ngay => Tu91Den180Ngay + Tu181Den360Ngay + Tu1Den2Nam + Tu2Den3Nam + Tren3Nam;
    public decimal TongNo => TrongHan + Tu1Den30Ngay + Tu31Den60Ngay + Tu61Den90Ngay + Tu91Den180Ngay + Tu181Den360Ngay + Tu1Den2Nam + Tu2Den3Nam + Tren3Nam;
}

/// <summary>
/// Dịch vụ quản trị Công nợ chi tiết (AR/AP Subledger) và Đối trừ hóa đơn
/// </summary>
public interface ICongNoService
{
    Task<(bool ThanhCong, string? ThongBao)> DoiTruHoaDonBanAsync(long hoaDonBanHangId, decimal soTien, long? butToanId = null, string? ghiChu = null);
    Task<(bool ThanhCong, string? ThongBao)> DoiTruHoaDonMuaAsync(long hoaDonMuaHangId, decimal soTien, long? butToanId = null, string? ghiChu = null);
    Task<(bool ThanhCong, string? ThongBao, decimal DaDoiTru)> DoiTruFifoKhachHangAsync(long khachHangId, decimal tongTienThu, long? butToanId = null);
    Task<List<AgingReportItem>> BaoCaoTuoiNoPhaiThuAsync(DateTime? mocThoiGian = null);
    Task<List<AgingReportItem>> BaoCaoTuoiNoPhaiTraAsync(DateTime? mocThoiGian = null);
    Task<(bool ThanhCong, string? ThongBao, ButToan? ButToan)> BuTruCongNoHaiChieuAsync(long doiTuongId, decimal soTien, DateTime ngayHachToan, string? ghiChu = null);
}
