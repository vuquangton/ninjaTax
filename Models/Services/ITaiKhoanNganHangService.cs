using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface ITaiKhoanNganHangService
{
    Task<List<TaiKhoanNganHangItemViewModel>> LayDanhSachAsync(string? timKiem = null);
    Task<TaiKhoanNganHang?> LayTheoIdAsync(long id);
    Task<(bool ThanhCong, string? ThongBao, long? BankAccountId)> TaoMoiAsync(TaiKhoanNganHangCreateEditViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, TaiKhoanNganHangCreateEditViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id);
    Task<bool> KiemTraDaPhatSinhGiaoDichAsync(long id);
}
