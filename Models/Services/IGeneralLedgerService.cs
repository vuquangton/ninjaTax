using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ trích xuất Sổ Cái và Báo cáo Tổng hợp (General Ledger Service) theo TT99/2025/TT-BTC.
/// </summary>
public interface IGeneralLedgerService
{
    /// <summary>
    /// Lập Sổ Nhật ký chung (Mẫu S03a-DN)
    /// </summary>
    Task<SoNhatKyChungViewModel> LaySoNhatKyChungAsync(DateTime tuNgay, DateTime denNgay);

    /// <summary>
    /// Lập Sổ Cái một tài khoản kế toán chi tiết hoặc tổng hợp (Mẫu S03b-DN)
    /// </summary>
    Task<SoCaiViewModel> LaySoCaiAsync(string maTaiKhoan, DateTime tuNgay, DateTime denNgay);

    /// <summary>
    /// Lập Bảng Cân đối số phát sinh các tài khoản (Trial Balance 8 cột)
    /// </summary>
    Task<BangCanDoiTaiKhoanViewModel> LayBangCanDoiTaiKhoanAsync(DateTime tuNgay, DateTime denNgay);
}
