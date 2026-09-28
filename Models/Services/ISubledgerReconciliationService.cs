using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ đối soát tính toàn vẹn giữa các Sổ Phụ (Subledger) và Sổ Cái (General Ledger).
/// </summary>
public interface ISubledgerReconciliationService
{
    /// <summary>
    /// Thực hiện đối soát số dư AR, AP, Kho, TSCĐ đối chiếu với TK 131, 331, 1561, 211, 214 tại mốc thời gian.
    /// </summary>
    Task<SubledgerReconciliationReportViewModel> KiemTraDoiSoatToanHeThongAsync(DateTime mocThoiGian);
}
