using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IThuChiService
{
    Task<string> SinhSoChungTuAsync(LoaiChungTuThuChi loaiChungTu, int nam);
    Task<decimal> TinhTonQuyKhaDungAsync(DateTime denNgay, long? taiKhoanNganHangId = null);
    Task<ChungTuThuChi> TaoChungTuThuChiAsync(ThuChiCreateViewModel model);
    Task<ChungTuThuChi> GhiSoChungTuAsync(long chungTuId);
    Task<ChungTuThuChi> HuyChungTuAsync(long chungTuId, string lyDoHuy);
    Task<ThuChiFilterViewModel> TimKiemChungTuAsync(LoaiChungTuThuChi? loaiChungTu, DateTime? tuNgay, DateTime? denNgay, string? tuKhoa);
    Task<ChungTuThuChi?> LayChiTietChungTuAsync(long id);
    Task<SoQuyBaoCaoViewModel> LayBaoCaoSoQuyAsync(string loaiSo, DateTime tuNgay, DateTime denNgay, long? taiKhoanNganHangId = null);
    Task<List<TaiKhoanNganHang>> LayDanhSachTaiKhoanNganHangAsync();
    Task<TaiKhoanNganHang> TaoTaiKhoanNganHangAsync(TaiKhoanNganHang taiKhoan);
}
