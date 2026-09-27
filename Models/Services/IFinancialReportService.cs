using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IFinancialReportService
{
    Task<BctcDashboardViewModel> LayDashboardAsync(int namTaiChinh);
    Task<BaoCaoTinhHinhTaiChinhViewModel> LapBaoCaoB01Async(int namTaiChinh);
    Task<BaoCaoKetQuaKinhDoanhViewModel> LapBaoCaoB02Async(int namTaiChinh);
    Task<BaoCaoLuuChuyenTienTeViewModel> LapBaoCaoB03Async(int namTaiChinh);
    Task<ThuyetMinhBctcViewModel> LapThuyetMinhB09Async(int namTaiChinh);
    Task<BaoCaoBoPhanViewModel> LapBaoCaoBoPhanAsync(int namTaiChinh, long? branchId = null);
    Task KhoaSoBctcNamAsync(int namTaiChinh);
}
