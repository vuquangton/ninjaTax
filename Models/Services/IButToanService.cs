using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ Bút toán kế toán / Sổ Nhật ký chung.
/// Quản lý logic kiểm tra cân đối kép (TongNo == TongCo) và tuân thủ Thông tư TT99.
/// </summary>
public interface IButToanService
{
    /// <summary>
    /// Lấy danh sách tất cả các bút toán theo thứ tự ngày hạch toán mới nhất
    /// </summary>
    Task<List<ButToan>> LayDanhSachAsync();

    /// <summary>
    /// Lấy thông tin chi tiết một bút toán kèm các dòng định khoản và đối tượng
    /// </summary>
    Task<ButToan?> LayTheoIdAsync(long id);

    /// <summary>
    /// Tạo mới một bút toán kèm xác thực cân đối Nợ/Có và các nguyên tắc TT99
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao, ButToan? ButToan)> TaoMoiAsync(ButToan butToan);

    /// <summary>
    /// Ghi sổ bút toán (chuyển trạng thái sang Đã ghi sổ nếu thỏa mãn cân đối kép)
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao)> GhiSoAsync(long id);

    /// <summary>
    /// Bỏ ghi sổ bút toán (chuyển trạng thái về Chưa ghi sổ để chỉnh sửa)
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao)> BoGhiSoAsync(long id);

    /// <summary>
    /// Xóa bút toán (chỉ cho phép xóa khi chưa ghi sổ)
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id);

    /// <summary>
    /// Kiểm tra tính hợp lệ và cân đối của bút toán theo chuẩn TT99
    /// </summary>
    (bool HopLe, string? Loi) KiemTraHopLe(ButToan butToan);

    /// <summary>
    /// Lấy danh mục tài khoản (loại trừ 911) và danh mục đối tượng để phục vụ lập chứng từ
    /// </summary>
    Task<(List<TaiKhoan> TaiKhoans, List<DoiTuong> DoiTuongs)> LayDanhMucTaoButToanAsync();
}
