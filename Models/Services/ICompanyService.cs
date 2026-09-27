using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Giao diện dịch vụ Quản lý Doanh nghiệp, Đa chi nhánh và Khóa sổ Kế toán (Phase 6).
/// </summary>
public interface ICompanyService
{
    Task<ThongTinDoanhNghiep> GetCompanyProfileAsync();
    Task UpdateCompanyProfileAsync(ThongTinDoanhNghiep profile);

    Task<List<ChiNhanh>> GetBranchesAsync();
    Task<ChiNhanh?> GetBranchByIdAsync(long id);
    Task<ChiNhanh> SaveBranchAsync(ChiNhanh branch);
    Task<bool> DeleteBranchAsync(long id);

    Task<CauHinhKeToan> GetAccountingConfigAsync();
    Task UpdateAccountingConfigAsync(CauHinhKeToan config);

    Task LockBookToDateAsync(DateTime lockDate);
    Task UnlockBookAsync();

    /// <summary>
    /// Kiểm tra tính hợp lệ của ngày hạch toán đối chiếu với ngày khóa sổ.
    /// Ném InvalidOperationException nếu ngày hạch toán vi phạm ngày khóa sổ.
    /// </summary>
    Task<bool> ValidateCanPostTransactionAsync(DateTime transactionDate);
}

