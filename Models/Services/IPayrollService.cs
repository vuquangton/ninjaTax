using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IPayrollService
{
    Task<BangLuongChiTietViewModel> TinhLuongThangAsync(string kyKeToan);
    Task<BangLuongThang> GhiSoBangLuongAsync(long bangLuongId);
    Task<PayslipViewModel> LayPhieuLuongNhanVienAsync(long bangLuongId, long nhanVienId);
    Task<List<BangLuongThangItemViewModel>> LayDanhSachBangLuongAsync();

    // Thuật toán kiểm tra trần/sàn và trích theo luật
    (decimal BhxhDn, decimal BhytDn, decimal BhtnDn, decimal KpcdDn) TinhBaoHiemDoanhNghiep(decimal luongDongBh, int vung = 1);
    (decimal BhxhNld, decimal BhytNld, decimal BhtnNld) TinhBaoHiemNguoiLaoDong(decimal luongDongBh, int vung = 1);
    decimal TinhThueTncn(NhanVien nv, decimal tongThuNhap, decimal baoHiemNld, decimal otMienThue, decimal phuCapMienThue);
}
