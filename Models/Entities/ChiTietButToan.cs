namespace ninjaTax.Models.Entities;

/// <summary>
/// Dòng định khoản chi tiết của bút toán (Chứng từ Nhật ký chung).
/// Tuân thủ nguyên tắc định khoản kép:
/// - Mỗi dòng chỉ rõ Tài khoản Nợ (TaiKhoanNo) và Tài khoản Có (TaiKhoanCo).
/// - Số tiền phát sinh (SoTien) với độ chính xác số học kế toán (19, 4).
/// - Đối tượng theo dõi công nợ chi tiết (Khách hàng / Nhà cung cấp).
/// </summary>
public class ChiTietButToan
{
    /// <summary>
    /// Khóa chính (bigint/long) duy nhất cho dòng chi tiết
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới chứng từ bút toán mẹ
    /// </summary>
    public long ButToanId { get; set; }

    /// <summary>
    /// Thực thể chứng từ bút toán mẹ
    /// </summary>
    public ButToan? ButToan { get; set; }

    /// <summary>
    /// Thứ tự dòng trong chứng từ (1, 2, 3...)
    /// </summary>
    public int DongSo { get; set; } = 1;

    /// <summary>
    /// Khóa ngoại tài khoản ghi Nợ (Không được là TK 911)
    /// </summary>
    public long TaiKhoanNoId { get; set; }

    /// <summary>
    /// Thực thể tài khoản ghi Nợ
    /// </summary>
    public TaiKhoan? TaiKhoanNo { get; set; }

    /// <summary>
    /// Khóa ngoại tài khoản ghi Có (Không được là TK 911)
    /// </summary>
    public long TaiKhoanCoId { get; set; }

    /// <summary>
    /// Thực thể tài khoản ghi Có
    /// </summary>
    public TaiKhoan? TaiKhoanCo { get; set; }

    /// <summary>
    /// Số tiền phát sinh hạch toán (decimal precision 19, 4)
    /// </summary>
    public decimal SoTien { get; set; }

    /// <summary>
    /// Diễn giải chi tiết cho dòng nghiệp vụ
    /// </summary>
    public string? DienGiai { get; set; }

    /// <summary>
    /// Khóa ngoại đối tượng công nợ (Khách hàng/NCC/Nhân viên) nếu tài khoản có tính chất theo dõi đối tượng
    /// </summary>
    public long? DoiTuongId { get; set; }

    /// <summary>
    /// Thực thể đối tượng công nợ
    /// </summary>
    public DoiTuong? DoiTuong { get; set; }

    /// <summary>
    /// Khóa ngoại bộ phận / phòng ban / trung tâm chi phí (Cost Center) phục vụ báo cáo quản trị và TT99
    /// </summary>
    public long? PhongBanId { get; set; }

    /// <summary>
    /// Thực thể phòng ban / trung tâm chi phí
    /// </summary>
    public virtual PhongBan? PhongBan { get; set; }
}
