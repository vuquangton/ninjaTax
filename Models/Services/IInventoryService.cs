using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IInventoryService
{
    /// <summary>
    /// Lấy số lượng tồn kho tức thời hoặc tại một mốc thời gian của vật tư tại một kho cụ thể.
    /// Chỉ tính các chứng từ đã ghi sổ (TrangThai == DaGhiSo).
    /// </summary>
    Task<decimal> GetStockBalanceAsync(long khoId, long vatTuId, DateTime? asOfDate = null);

    /// <summary>
    /// Lấy tổng số lượng tồn kho của vật tư trên toàn bộ hệ thống các kho (hoặc theo chi nhánh).
    /// </summary>
    Task<decimal> GetTotalStockBalanceAcrossWarehousesAsync(long vatTuId, long? branchId = null, DateTime? asOfDate = null);

    /// <summary>
    /// Kiểm tra tính khả dụng của hàng tồn kho trước khi xuất (Anti-Negative Stock Invariant).
    /// Ném InvalidOperationException nếu số lượng xuất vượt quá tồn kho khả dụng tại ngày xuất.
    /// </summary>
    Task ValidateStockAvailabilityAsync(long khoId, long vatTuId, decimal soLuongXuat, DateTime ngayXuat, long? excludeXuatKhoId = null);

    /// <summary>
    /// Lấy danh sách kho hàng đang hoạt động (lọc theo chi nhánh nếu có).
    /// </summary>
    Task<List<Kho>> GetActiveWarehousesAsync(long? branchId = null);

    /// <summary>
    /// Lấy toàn bộ danh sách kho hàng (kèm chi nhánh, thủ kho, tài khoản).
    /// </summary>
    Task<List<Kho>> GetAllWarehousesAsync(long? branchId = null);

    /// <summary>
    /// Lấy thông tin kho theo Id kèm chi nhánh và thủ kho.
    /// </summary>
    Task<Kho?> GetWarehouseByIdAsync(long id);

    /// <summary>
    /// Lưu hoặc cập nhật thông tin kho hàng.
    /// </summary>
    Task<Kho> SaveWarehouseAsync(Kho kho);

    /// <summary>
    /// Kiểm tra kho hàng đã phát sinh phiếu kho hoặc tồn kho hay chưa.
    /// </summary>
    Task<bool> CanDeleteWarehouseAsync(long khoId);

    /// <summary>
    /// Xóa kho hàng nếu chưa phát sinh chứng từ.
    /// </summary>
    Task<(bool Success, string? Message)> DeleteWarehouseAsync(long khoId);

    /// <summary>
    /// Tính đơn giá xuất kho bình quân gia quyền tại thời điểm xuất kho (VAS 02 / TT 200).
    /// </summary>
    Task<decimal> CalculateWeightedAverageCostAsync(long khoId, long vatTuId, DateTime asOfDate);

    /// <summary>
    /// Lưu phiếu nhập kho (Tạo mới hoặc cập nhật thông tin ở trạng thái TamTinh).
    /// </summary>
    Task<PhieuNhapKho> SavePhieuNhapKhoAsync(PhieuNhapKho phieuNhap);

    /// <summary>
    /// Ghi sổ phiếu nhập kho: cập nhật trạng thái DaGhiSo và tự động hạch toán bút toán Sổ cái TT99.
    /// </summary>
    Task<PhieuNhapKho> GhiSoPhieuNhapKhoAsync(long phieuNhapKhoId);

    /// <summary>
    /// Hủy ghi sổ phiếu nhập kho (chỉ cho phép nếu không gây âm kho).
    /// </summary>
    Task<bool> HuyGhiSoPhieuNhapKhoAsync(long phieuNhapKhoId);

    /// <summary>
    /// Lấy chi tiết phiếu nhập kho theo Id.
    /// </summary>
    Task<PhieuNhapKho?> GetPhieuNhapByIdAsync(long id);

    /// <summary>
    /// Lưu phiếu xuất kho (Tạo mới hoặc cập nhật thông tin ở trạng thái TamTinh).
    /// </summary>
    Task<PhieuXuatKho> SavePhieuXuatKhoAsync(PhieuXuatKho phieuXuat);

    /// <summary>
    /// Ghi sổ phiếu xuất kho: kiểm tra chống xuất âm kho, tự động tính giá vốn BQGQ và sinh bút toán Nợ 632 / Có 1561.
    /// </summary>
    Task<PhieuXuatKho> GhiSoPhieuXuatKhoAsync(long phieuXuatKhoId);

    /// <summary>
    /// Hủy ghi sổ phiếu xuất kho.
    /// </summary>
    Task<bool> HuyGhiSoPhieuXuatKhoAsync(long phieuXuatKhoId);

    /// <summary>
    /// Lấy chi tiết phiếu xuất kho theo Id.
    /// </summary>
    Task<PhieuXuatKho?> GetPhieuXuatByIdAsync(long id);

    /// <summary>
    /// Lập Báo Cáo Nhập - Xuất - Tồn (Mẫu S10-DN theo TT 200/2014/TT-BTC & TT 99/2025/TT-BTC)
    /// và tự động đối soát với Sổ Cái TK Kho (152, 1561).
    /// </summary>
    Task<BaoCaoNhapXuatTonViewModel> LapBaoCaoNhapXuatTonAsync(DateTime tuNgay, DateTime denNgay, long? khoId = null, long? branchId = null);

    /// <summary>
    /// Động cơ tính lại giá xuất kho theo phương pháp Bình quân gia quyền cả kỳ (VAS 02).
    /// Quét và cập nhật lại đơn giá vốn trên toàn bộ các dòng phiếu xuất kho trong kỳ và sinh bút toán điều chỉnh giá vốn (TK 632 / 1561).
    /// </summary>
    Task<(bool Success, string? Message, int SoDongCapNhat, decimal TongChenhLech)> RecalculatePeriodWeightedAverageCostAsync(
        DateTime tuNgay, 
        DateTime denNgay, 
        long? khoId = null, 
        long? branchId = null);
}
