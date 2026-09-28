namespace ninjaTax.Models.Entities;

/// <summary>
/// Loại tài khoản theo tính chất báo cáo tài chính
/// </summary>
public enum LoaiTaiKhoan
{
    TaiSan = 1,
    NoPhaiTra = 2,
    VonChuSoHuu = 3,
    DoanhThu = 4,
    ChiPhi = 5,
    ThuNhapKhac = 7,
    ChiPhiKhac = 8,
    XacDinhKetQuaKinhDoanh = 9
}

/// <summary>
/// Tính chất số dư tài khoản
/// </summary>
public enum TinhChatTaiKhoan
{
    DuNo = 1,
    DuCo = 2,
    LuongTinh = 3,
    KhongCoSoDu = 4
}

/// <summary>
/// Danh mục Hệ thống Tài khoản kế toán theo Thông tư TT99 (VAS).
/// Quy định nghiệp vụ TT99:
/// - Chuẩn hóa sử dụng Tài khoản 911 (Xác định kết quả kinh doanh) làm tài khoản trung gian kết chuyển cuối kỳ (số dư = 0).
/// - Khóa chính (Id) dùng kiểu long/bigint tối ưu hiệu năng B-Tree Index cho Sổ Cái lớn.
/// </summary>
public class TaiKhoan
{
    /// <summary>
    /// Khóa chính (bigint/long) duy nhất cho tài khoản
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Số hiệu tài khoản kế toán (VD: 111, 112, 131, 331, 421, 511, 642, ...)
    /// </summary>
    public string MaTaiKhoan { get; set; } = string.Empty;

    /// <summary>
    /// Tên tài khoản kế toán (VD: Tiền mặt, Tiền gửi ngân hàng, ...)
    /// </summary>
    public string TenTaiKhoan { get; set; } = string.Empty;

    /// <summary>
    /// Cấp bậc tài khoản (1: Cấp 1, 2: Cấp 2, 3: Cấp 3...)
    /// </summary>
    public int BacTaiKhoan { get; set; } = 1;

    /// <summary>
    /// Khóa ngoại tới tài khoản mẹ (nếu là tài khoản chi tiết cấp con)
    /// </summary>
    public long? TaiKhoanMeId { get; set; }

    /// <summary>
    /// Đối tượng tài khoản mẹ
    /// </summary>
    public TaiKhoan? TaiKhoanMe { get; set; }

    /// <summary>
    /// Danh sách các tài khoản con cấp dưới
    /// </summary>
    public ICollection<TaiKhoan> TaiKhoanCons { get; set; } = new List<TaiKhoan>();

    /// <summary>
    /// Phân loại nhóm tài khoản (Tài sản, Nợ phải trả, Vốn CSH, Doanh thu, Chi phí...)
    /// </summary>
    public LoaiTaiKhoan LoaiTaiKhoan { get; set; }

    /// <summary>
    /// Tính chất số dư tài khoản (Dư Nợ, Dư Có, Lưỡng tính)
    /// </summary>
    public TinhChatTaiKhoan TinhChat { get; set; }

    /// <summary>
    /// Cờ đánh dấu tài khoản tổng hợp (chỉ theo dõi sổ cái tổng, không cho phép hạch toán trực tiếp)
    /// </summary>
    public bool LaTaiKhoanSoCai { get; set; }

    /// <summary>
    /// Trạng thái hoạt động (true: đang sử dụng; false: ngừng sử dụng)
    /// </summary>
    public bool DangHoatDong { get; set; } = true;

    public string TenHienThiDropdown => $"{MaTaiKhoan} - {TenTaiKhoan}";
}
