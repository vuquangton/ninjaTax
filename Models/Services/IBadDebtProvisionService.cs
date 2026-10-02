using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public class ChiTietTrichLapDuPhongViewModel
{
    public long KhachHangId { get; set; }
    public string MaKhachHang { get; set; } = string.Empty;
    public string TenKhachHang { get; set; } = string.Empty;
    public long? HoaDonBanHangId { get; set; }
    public string SoHoaDon { get; set; } = string.Empty;
    public DateTime NgayHoaDon { get; set; }
    public DateTime HanThanhToan { get; set; }
    public decimal SoTienConNo { get; set; }
    public int SoNgayQuaHan { get; set; }
    public decimal TyLeTrichLap { get; set; }
    public decimal SoTienDuPhong { get; set; }
    public string? LyDoDacBiet { get; set; }
}

public interface IBadDebtProvisionService
{
    Task<BangTrichLapDuPhongNoPhaiThu> TaoBangTrichLapDuPhongAsync(DateTime ngayHachToan, string? ghiChu = null);
    Task<(bool ThanhCong, string? ThongBao)> GhiSoBangTrichLapAsync(long bangTrichLapId);
    Task<(bool ThanhCong, string? ThongBao)> HuyBangTrichLapAsync(long bangTrichLapId);
    Task<List<ChiTietTrichLapDuPhongViewModel>> LayDanhSachNoQuaHanTt48Async(DateTime mocThoiGian);
}
