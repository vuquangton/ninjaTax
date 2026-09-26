using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum TrangThaiChamCong
{
    DangCham = 1,
    DaChot = 2,
    DaKhoa = 3
}

/// <summary>
/// Bảng chấm công tổng hợp theo tháng (Timesheet).
/// </summary>
public class BangChamCongThang
{
    public long Id { get; set; }

    /// <summary>
    /// Kỳ kế toán (Định dạng YYYY-MM, ví dụ: 2026-09)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string KyKeToan { get; set; } = string.Empty;

    public int Nam { get; set; }

    public int Thang { get; set; }

    /// <summary>
    /// Số ngày công chuẩn của tháng (Thường là 22 ngày đối với nghỉ T7+CN, hoặc 26 ngày nếu chỉ nghỉ CN)
    /// </summary>
    public int SoNgayCongChuan { get; set; } = 22;

    public TrangThaiChamCong TrangThai { get; set; } = TrangThaiChamCong.DangCham;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietChamCong> ChiTiets { get; set; } = new List<ChiTietChamCong>();

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }
}

/// <summary>
/// Chi tiết chấm công từng nhân sự trong tháng theo Bộ luật Lao động 2019.
/// </summary>
public class ChiTietChamCong
{
    public long Id { get; set; }

    public long BangChamCongThangId { get; set; }
    public virtual BangChamCongThang? BangChamCongThang { get; set; }

    public long NhanVienId { get; set; }
    public virtual NhanVien? NhanVien { get; set; }

    /// <summary>
    /// Số ngày đi làm thực tế
    /// </summary>
    public decimal SoNgayDiLam { get; set; } = 0;

    /// <summary>
    /// Nghỉ phép năm có hưởng 100% lương theo Điều 113 BLLĐ 2019
    /// </summary>
    public decimal SoNgayNghiPhep { get; set; } = 0;

    /// <summary>
    /// Nghỉ lễ, tết hưởng 100% lương theo Điều 112 BLLĐ 2019
    /// </summary>
    public decimal SoNgayNghiLe { get; set; } = 0;

    public decimal SoNgayNghiKhongLuong { get; set; } = 0;

    /// <summary>
    /// Nghỉ ốm đau hưởng chế độ BHXH (không tính vào chi phí lương DN)
    /// </summary>
    public decimal SoNgayNghiOmBhxh { get; set; } = 0;

    /// <summary>
    /// Nghỉ thai sản hưởng chế độ BHXH
    /// </summary>
    public decimal SoNgayNghiThaiSan { get; set; } = 0;

    // Làm thêm giờ (Overtime - OT)
    public decimal GioLamThemNgayThuong { get; set; } = 0; // x150%
    public decimal GioLamThemNgayNghi { get; set; } = 0;   // x200%
    public decimal GioLamThemNgayLe { get; set; } = 0;     // x300%

    /// <summary>
    /// Tổng ngày công tính lương = SoNgayDiLam + SoNgayNghiPhep + SoNgayNghiLe
    /// </summary>
    public decimal TongCongTinhLuong { get; set; } = 0;
}
