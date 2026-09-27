using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface ITaxAuditShieldService
{
    Task<TaxAuditShieldReportViewModel> QuetToanBoBayThueAsync(int namTaiChinh);
    Task<TaxAuditShieldReportViewModel> LuuBaoCaoTaxShieldAsync(int namTaiChinh);
}
