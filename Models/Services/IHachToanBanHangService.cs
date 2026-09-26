using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ nghiệp vụ Hóa đơn bán hàng, phát hành HĐĐT và tự động sinh bút toán Doanh thu / Giá vốn TT99
/// </summary>
public interface IHachToanBanHangService
{
    Task<List<HoaDonBanHang>> LayDanhSachAsync();
    Task<HoaDonBanHang?> LayTheoIdAsync(long id);
    Task<(bool ThanhCong, string? ThongBao, HoaDonBanHang? HoaDon)> TaoMoiAsync(HoaDonBanHang hoaDon);
    Task<(bool ThanhCong, string? ThongBao)> PhatHanhHddtAsync(long hoaDonId);
    Task<(bool ThanhCong, string? ThongBao, ButToan? DoanhThu, ButToan? GiaVon)> GhiSoAsync(long hoaDonId);
    Task<(bool ThanhCong, string? ThongBao)> BoGhiSoAsync(long hoaDonId);
    Task<(bool ThanhCong, string? ThongBao)> HuyHoaDonAsync(long hoaDonId, string lyDo);
}
