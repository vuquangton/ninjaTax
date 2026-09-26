using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface ITimesheetService
{
    Task<BangChamCongViewModel> LayHoacTaoBangChamCongAsync(string kyKeToan, int soNgayCongChuan = 22);
    Task CapNhatChiTietChamCongAsync(long bangChamCongId, List<DongChamCongViewModel> danhSach);
    Task<BangChamCongThang> ChotBangChamCongAsync(long bangChamCongId);
}
