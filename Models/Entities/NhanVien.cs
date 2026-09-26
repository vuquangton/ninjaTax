using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum LoaiHopDongLaoDong
{
    HopDongDaiHan = 1,   // HĐLĐ từ 3 tháng trở lên: Tính thuế TNCN theo biểu lũy tiến 7 bậc, bắt buộc đóng BHXH
    ThuViecThoiVu = 2    // HĐLĐ dưới 3 tháng / Thử việc / CTV: Khấu trừ 10% tại nguồn (hoặc Cam kết 08)
}

/// <summary>
/// Quản lý hồ sơ nhân sự, hợp đồng lao động, mức lương đóng bảo hiểm và thông tin giảm trừ thuế TNCN.
/// </summary>
public class NhanVien
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string MaNhanVien { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(20)]
    public string? SoCccd { get; set; }

    [StringLength(20)]
    public string? MaSoThue { get; set; }

    [StringLength(20)]
    public string? SoSoBhxh { get; set; }

    [StringLength(100)]
    public string? PhongBan { get; set; }

    [StringLength(100)]
    public string? ChucVu { get; set; }

    public LoaiHopDongLaoDong LoaiHopDong { get; set; } = LoaiHopDongLaoDong.HopDongDaiHan;

    public DateTime NgayVaoLam { get; set; } = DateTime.Today;

    public DateTime? NgayKetThucHd { get; set; }

    // Mức lương & Chế độ phụ cấp
    public decimal LuongCoBan { get; set; } = 0;

    /// <summary>
    /// Mức lương làm căn cứ đóng BHXH, BHYT, BHTN (thỏa mãn Sàn lương vùng & Trần 20 lần MLCS)
    /// </summary>
    public decimal LuongDongBaoHiem { get; set; } = 0;

    /// <summary>
    /// Phụ cấp ăn trưa / giữa ca (Miễn thuế tối đa 730.000 VNĐ/tháng theo TT 26/2016)
    /// </summary>
    public decimal PhuCapAnTrua { get; set; } = 0;

    /// <summary>
    /// Phụ cấp trách nhiệm / chức vụ (Đóng BHXH và chịu thuế TNCN 100%)
    /// </summary>
    public decimal PhuCapTrachNhiem { get; set; } = 0;

    /// <summary>
    /// Phụ cấp điện thoại (Miễn thuế TNCN theo quy chế tài chính doanh nghiệp)
    /// </summary>
    public decimal PhuCapDienThoai { get; set; } = 0;

    /// <summary>
    /// Phụ cấp trang phục (Miễn thuế TNCN tối đa 5.000.000 VNĐ/năm)
    /// </summary>
    public decimal PhuCapTrangPhuc { get; set; } = 0;

    /// <summary>
    /// Số người phụ thuộc đã đăng ký MST người phụ thuộc hợp lệ (mỗi người giảm trừ 4.400.000 VNĐ/tháng)
    /// </summary>
    public int SoNguoiPhuThuoc { get; set; } = 0;

    /// <summary>
    /// Cờ áp dụng Cam kết Mẫu 08/CK-TNCN theo Thông tư 80/2021 (cho lao động thời vụ < 3 tháng tạm thời không khấu trừ 10%)
    /// </summary>
    public bool CoCamKet08 { get; set; } = false;

    public bool DongBaoHiem { get; set; } = true;

    public bool LaDoanVienCongDoan { get; set; } = false;

    [StringLength(50)]
    public string? SoTaiKhoanNganHang { get; set; }

    [StringLength(100)]
    public string? TenNganHang { get; set; }

    public bool DangLamViec { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
