using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public interface ILandedCostService
{
    Task<ChungTuChiPhiMuaHang?> GetByIdAsync(long id);
    Task<List<ChungTuChiPhiMuaHang>> GetAllAsync(long? chiNhanhId = null);
    
    /// <summary>
    /// Tạo mới chứng từ chi phí mua hàng (chưa phân bổ)
    /// </summary>
    Task<ChungTuChiPhiMuaHang> CreateAsync(ChungTuChiPhiMuaHang chungTu);

    /// <summary>
    /// Thực hiện phân bổ chi phí mua hàng vào danh sách các dòng ChiTietNhapKho đã chọn (VAS 02)
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> AllocateCostAsync(
        long chungTuChiPhiId, 
        List<long> chiTietNhapKhoIds, 
        PhuongThucPhanBoChiPhi phuongThuc,
        long taiKhoanChiPhiId,
        long taiKhoanDoiUngId);

    /// <summary>
    /// Hủy phân bổ chi phí mua hàng và hoàn tác nguyên giá nhập kho
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> CancelAllocationAsync(long chungTuChiPhiId);
}
