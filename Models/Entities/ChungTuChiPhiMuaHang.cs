using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ninjaTax.Models.Entities;

/// <summary>
/// Phương thức phân bổ chi phí mua hàng
/// </summary>
public enum PhuongThucPhanBoChiPhi
{
    TheoGiaTri = 1,  // Phân bổ theo tỷ lệ thành tiền hàng (VAS 02)
    TheoSoLuong = 2  // Phân bổ theo tỷ lệ số lượng hàng
}

/// <summary>
/// Chứng từ Chi phí Mua hàng (Landed Cost Voucher).
/// Dùng để tập hợp các chi phí vận chuyển, bốc dỡ, lưu kho, bảo hiểm phát sinh khi mua hàng
/// và phân bổ trực tiếp vào nguyên giá nhập kho (TK 1561/152) theo Chuẩn mực VAS 02.
/// </summary>
public class ChungTuChiPhiMuaHang
{
    public long Id { get; set; }

    [Required]
    public long ChiNhanhId { get; set; }
    public virtual ChiNhanh? ChiNhanh { get; set; }

    [Required]
    [StringLength(50)]
    public string SoChungTu { get; set; } = string.Empty;

    public DateTime NgayChungTu { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    /// <summary>
    /// Nhà cung cấp dịch vụ vận chuyển/bốc xếp
    /// </summary>
    public long? NhaCungCapDichVuId { get; set; }
    public virtual DoiTuong? NhaCungCapDichVu { get; set; }

    [StringLength(500)]
    public string DienGiai { get; set; } = string.Empty;

    /// <summary>
    /// Tổng chi phí mua hàng cần phân bổ (chưa VAT)
    /// </summary>
    [Column(TypeName = "decimal(19, 4)")]
    public decimal TongChiPhi { get; set; }

    /// <summary>
    /// Thuế suất GTGT chi phí vận chuyển/dịch vụ
    /// </summary>
    [Column(TypeName = "decimal(19, 4)")]
    public decimal ThueSuatVat { get; set; } = 10m;

    [Column(TypeName = "decimal(19, 4)")]
    public decimal TienThueVat { get; set; }

    [Column(TypeName = "decimal(19, 4)")]
    public decimal TongThanhToan { get; set; }

    public PhuongThucPhanBoChiPhi PhuongThucPhanBo { get; set; } = PhuongThucPhanBoChiPhi.TheoGiaTri;

    public bool DaPhanBo { get; set; } = false;

    public long? ButToanId { get; set; }
    public virtual ButToan? ButToan { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChiPhiMuaHangPhanBo> ChiTietPhanBos { get; set; } = new List<ChiPhiMuaHangPhanBo>();
}

/// <summary>
/// Chi tiết dòng phân bổ chi phí mua hàng cho từng dòng ChiTietNhapKho
/// </summary>
public class ChiPhiMuaHangPhanBo
{
    public long Id { get; set; }

    [Required]
    public long ChungTuChiPhiMuaHangId { get; set; }
    public virtual ChungTuChiPhiMuaHang? ChungTuChiPhiMuaHang { get; set; }

    [Required]
    public long ChiTietNhapKhoId { get; set; }
    public virtual ChiTietNhapKho? ChiTietNhapKho { get; set; }

    /// <summary>
    /// Số tiền chi phí mua hàng phân bổ vào dòng nhập này (decimal 19,4)
    /// </summary>
    [Column(TypeName = "decimal(19, 4)")]
    public decimal SoTienPhanBo { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }
}
