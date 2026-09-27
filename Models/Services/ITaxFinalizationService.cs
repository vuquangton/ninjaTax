using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface ITaxFinalizationService
{
    Task<QuyetToanTndnViewModel> LapQuyetToanTndnAsync(int namTaiChinh);
    Task<QuyetToanTncnViewModel> LapQuyetToanTncnAsync(int namTaiChinh);
    Task LuuQuyetToanTndnAsync(QuyetToanTndnViewModel model);
    Task LuuQuyetToanTncnAsync(QuyetToanTncnViewModel model);
}
