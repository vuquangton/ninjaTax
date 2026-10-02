using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public interface ICommercialAdjustmentService
{
    Task<ChungTuDieuChinhThuongMai> TaoChungTuAsync(ChungTuDieuChinhThuongMai chungTu);
    Task<bool> GhiSoAsync(long id);
    Task<bool> HuyGhiSoAsync(long id);
    Task<List<ChungTuDieuChinhThuongMai>> LayDanhSachAsync(LoaiDieuChinhThuongMai? loai = null, DateTime? tuNgay = null, DateTime? denNgay = null, long? branchId = null);
    Task<ChungTuDieuChinhThuongMai?> LayChiTietAsync(long id);
}
