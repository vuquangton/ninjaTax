namespace ninjaTax.Models.Services;

public interface IUnitConversionService
{
    /// <summary>
    /// Quy đổi số lượng từ đơn vị tính giao dịch (phụ) sang đơn vị tính cơ bản
    /// </summary>
    decimal QuyDoiVeDonViCoBan(decimal soLuongGiaoDich, decimal tyLeQuyDoi, Entities.PhepTinhQuyDoi phepTinh = Entities.PhepTinhQuyDoi.Nhan);

    /// <summary>
    /// Quy đổi số lượng từ đơn vị tính cơ bản sang đơn vị tính giao dịch (phụ)
    /// </summary>
    decimal QuyDoiTuDonViCoBan(decimal soLuongCoBan, decimal tyLeQuyDoi, Entities.PhepTinhQuyDoi phepTinh = Entities.PhepTinhQuyDoi.Nhan);

    /// <summary>
    /// Lấy danh sách đơn vị tính quy đổi của một vật tư hàng hóa
    /// </summary>
    Task<List<Entities.DonViTinhQuyDoi>> GetConversionUnitsAsync(long vatTuId);

    /// <summary>
    /// Thêm hoặc cập nhật đơn vị tính quy đổi
    /// </summary>
    Task<Entities.DonViTinhQuyDoi> SaveConversionUnitAsync(Entities.DonViTinhQuyDoi unit);

    /// <summary>
    /// Xóa đơn vị tính quy đổi
    /// </summary>
    Task<bool> DeleteConversionUnitAsync(long id);
}
