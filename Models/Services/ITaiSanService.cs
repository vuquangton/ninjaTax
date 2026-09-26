using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface ITaiSanService
{
    Task<TaiSanCoDinh> KhaiBaoTaiSanAsync(TaiSanCreateViewModel model);
    Task<TaiSanListViewModel> TimKiemTaiSanAsync(LoaiTaiSan? loai, TrangThaiTaiSan? trangThai, string? tuKhoa);
    Task<TaiSanCoDinh?> LayChiTietTaiSanAsync(long id);
    Task<BangKhauHaoKyViewModel> XemBangKhauHaoKyAsync(string kyKeToan);
    Task<List<BangTinhKhauHao>> ChayVaGhiSoKhauHaoKyAsync(string kyKeToan);
}
