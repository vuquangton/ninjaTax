using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ nghiệp vụ Hóa đơn mua hàng và tự động sinh bút toán Sổ cái GL theo TT99
/// </summary>
public interface IHachToanMuaHangService
{
    Task<List<HoaDonMuaHang>> LayDanhSachAsync();
    Task<HoaDonMuaHang?> LayTheoIdAsync(long id);
    Task<(bool ThanhCong, string? ThongBao, HoaDonMuaHang? HoaDon)> TaoMoiAsync(HoaDonMuaHang hoaDon);
    Task<(bool ThanhCong, string? ThongBao, ButToan? ButToan)> GhiSoAsync(long hoaDonId);
    Task<(bool ThanhCong, string? ThongBao)> BoGhiSoAsync(long hoaDonId);
    Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long hoaDonId);
}
