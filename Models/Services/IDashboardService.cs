using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetKpisAsync();
    Task<List<MonthlyCashFlowDto>> GetCashFlowAsync(int months = 12);
    Task<List<DebtorDto>> GetTopDebtorsAsync(int count = 10);
}
