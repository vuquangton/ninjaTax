using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

/// <summary>
/// Thực thể Kho hàng (Warehouse).
/// Quản lý vị trí lưu trữ vật tư, hàng hóa, công cụ dụng cụ, thành phẩm phân bổ theo từng chi nhánh.
/// Chuẩn hóa theo Thông tư 200/2014/TT-BTC và Thông tư 99/2025/TT-BTC.
/// </summary>
public class Kho
{
    public long Id { get; set; }

    [Required]
    public long ChiNhanhId { get; set; }
    public virtual ChiNhanh? ChiNhanh { get; set; }

    [Required(ErrorMessage = "Mã kho là bắt buộc")]
    [StringLength(50)]
    public string MaKho { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên kho là bắt buộc")]
    [StringLength(255)]
    public string TenKho { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DiaChi { get; set; }

    public long? ThuKhoId { get; set; }
    public virtual NhanVien? ThuKho { get; set; }

    /// <summary>
    /// Tài khoản kho ngầm định (VD: 152 - NVL, 1561 - Hàng hóa, 155 - Thành phẩm)
    /// </summary>
    public long? TaiKhoanKhoMacDinhId { get; set; }
    public virtual TaiKhoan? TaiKhoanKhoMacDinh { get; set; }

    public bool DangHoatDong { get; set; } = true;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; } = new List<PhieuNhapKho>();
    public virtual ICollection<PhieuXuatKho> PhieuXuatKhos { get; set; } = new List<PhieuXuatKho>();
}
