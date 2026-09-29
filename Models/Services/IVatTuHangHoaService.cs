using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IVatTuHangHoaService
{
    Task<List<VatTuHangHoaItemViewModel>> LayDanhSachAsync(string? timKiem = null, LoaiVatTuHangHoa? loai = null);
    Task<VatTuHangHoa?> LayTheoIdAsync(long id);
    Task<VatTuHangHoa?> LayTheoMaAsync(string maVatTu);
    Task<(bool ThanhCong, string? ThongBao, long? VatTuId)> TaoMoiAsync(VatTuHangHoaCreateViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, VatTuHangHoaEditViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id);
    Task<bool> KiemTraDaPhatSinhGiaoDichAsync(long id);
}
