namespace ninjaTax.Models.Entities;

/// <summary>
/// Trạng thái ghi sổ của chứng từ kế toán
/// </summary>
public enum TrangThaiButToan
{
    ChuaGhiSo = 0,
    DaGhiSo = 1,
    DaHuy = 2
}

/// <summary>
/// Bút toán kế toán / Chứng từ Nhật ký chung (Header).
/// Tuân thủ quy định chuẩn Thông tư TT99:
/// - Bắt buộc theo dõi nguồn gốc: Số chứng từ gốc (SoChungTuGoc) và Ngày chứng từ gốc (NgayChungTuGoc).
/// - Nguyên tắc cân đối bút toán kép: Tổng phát sinh Nợ phải bằng Tổng phát sinh Có (TongNo == TongCo).
/// - Khóa chính (Id) dùng kiểu long/bigint.
/// </summary>
public class ButToan
{
    /// <summary>
    /// Khóa chính (bigint/long) duy nhất cho chứng từ bút toán
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Số chứng từ hệ thống (VD: PKT-2026-00001, NKC-00012)
    /// </summary>
    public string SoChungTu { get; set; } = string.Empty;

    /// <summary>
    /// Ngày hạch toán (ngày ghi vào sổ sách kế toán)
    /// </summary>
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    /// <summary>
    /// Ngày lập chứng từ
    /// </summary>
    public DateTime NgayChungTu { get; set; } = DateTime.Today;

    /// <summary>
    /// Số chứng từ gốc đính kèm (Quy định bắt buộc theo TT99 để kiểm toán nguồn gốc)
    /// </summary>
    public string SoChungTuGoc { get; set; } = string.Empty;

    /// <summary>
    /// Ngày lập của chứng từ gốc đính kèm (Bắt buộc theo TT99)
    /// </summary>
    public DateTime NgayChungTuGoc { get; set; } = DateTime.Today;

    /// <summary>
    /// Diễn giải chung nội dung nghiệp vụ kinh tế phát sinh
    /// </summary>
    public string DienGiai { get; set; } = string.Empty;

    /// <summary>
    /// Tổng tiền giao dịch của chứng từ (Định dạng tiền tệ decimal 19, 4)
    /// </summary>
    public decimal TongTien { get; set; }

    /// <summary>
    /// Tổng phát sinh Nợ (Bắt buộc TongNo == TongCo khi ghi sổ)
    /// </summary>
    public decimal TongNo { get; set; }

    /// <summary>
    /// Tổng phát sinh Có (Bắt buộc TongCo == TongNo khi ghi sổ)
    /// </summary>
    public decimal TongCo { get; set; }

    /// <summary>
    /// Trạng thái chứng từ (Chưa ghi sổ, Đã ghi sổ, Đã hủy)
    /// </summary>
    public TrangThaiButToan TrangThai { get; set; } = TrangThaiButToan.ChuaGhiSo;

    /// <summary>
    /// Tên tài khoản hoặc người dùng tạo chứng từ
    /// </summary>
    public string? NguoiTao { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo bản ghi trong hệ thống
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Danh sách các dòng định khoản chi tiết (Nợ/Có) của chứng từ
    /// </summary>
    public ICollection<ChiTietButToan> ChiTietButToans { get; set; } = new List<ChiTietButToan>();
}
