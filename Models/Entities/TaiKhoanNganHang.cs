using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

/// <summary>
/// Quản lý Tài khoản Ngân hàng Doanh nghiệp phục vụ phân hệ Tiền gửi (Báo Có, Ủy Nhiệm Chi).
/// </summary>
public class TaiKhoanNganHang
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SoTaiKhoan { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string TenNganHang { get; set; } = string.Empty;

    [StringLength(255)]
    public string? ChiNhanh { get; set; }

    [StringLength(255)]
    public string? ChuTaiKhoan { get; set; }

    public decimal SoDuBanDau { get; set; } = 0;

    public bool DangHoatDong { get; set; } = true;

    /// <summary>
    /// Liên kết tiểu khoản kế toán TK 1121 tương ứng trong Bảng hệ thống tài khoản
    /// </summary>
    public long? TaiKhoanKeToanId { get; set; }
    public virtual TaiKhoan? TaiKhoanKeToan { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
