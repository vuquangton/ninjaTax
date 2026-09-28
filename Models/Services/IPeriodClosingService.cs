using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ kết chuyển tự động doanh thu chi phí cuối kỳ chuẩn TT99 (Strictly NO TK 911).
/// </summary>
public interface IPeriodClosingService
{
    /// <summary>
    /// Thực hiện kết chuyển toàn bộ doanh thu (5xx, 7xx) và chi phí (6xx, 8xx) trực tiếp vào TK 4212.
    /// Bất biến: Tuyệt đối không sinh bất kỳ dòng hạch toán nào qua TK 911.
    /// </summary>
    Task<KetChuyenCuoiKyResult> TaoButToanKetChuyenAsync(int nam, int? thang = null);

    /// <summary>
    /// Hủy kết chuyển kỳ (Xóa bút toán kết chuyển để tính toán lại nếu kỳ chưa bị khóa sổ).
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao)> HuyKetChuyenAsync(long butToanKetChuyenId);
}
