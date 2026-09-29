using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IDoiTuongService
{
    Task<List<DoiTuongItemViewModel>> LayDanhSachAsync(string? timKiem = null, LoaiDoiTuong? loai = null);
    Task<DoiTuong?> LayTheoIdAsync(long id);
    Task<DoiTuong?> LayTheoMaAsync(string maDoiTuong);
    Task<(bool ThanhCong, string? ThongBao, long? DoiTuongId)> TaoMoiAsync(DoiTuongCreateEditViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, DoiTuongCreateEditViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id);
    Task<bool> KiemTraDaPhatSinhGiaoDichAsync(long id);
    Task<(decimal DuNo, decimal DuCo)> LaySoDuCongNoDoiTuongAsync(long id);
}
