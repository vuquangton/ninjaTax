using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

/// <summary>
/// Bảng tính khấu hao TSCĐ & phân bổ CCDC từng kỳ kế toán (tháng).
/// </summary>
public class BangTinhKhauHao
{
    public long Id { get; set; }

    public long TaiSanCoDinhId { get; set; }
    public virtual TaiSanCoDinh? TaiSanCoDinh { get; set; }

    /// <summary>
    /// Kỳ kế toán (Định dạng YYYY-MM, ví dụ: 2026-09)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string KyKeToan { get; set; } = string.Empty;

    public DateTime TuNgay { get; set; }

    public DateTime DenNgay { get; set; }

    public decimal NguyenGia { get; set; } = 0;

    /// <summary>
    /// Số tiền trích khấu hao / phân bổ trong kỳ này
    /// </summary>
    public decimal SoTienKhauHao { get; set; } = 0;

    /// <summary>
    /// Lũy kế khấu hao đến hết kỳ này
    /// </summary>
    public decimal LuyKeKhauHao { get; set; } = 0;

    /// <summary>
    /// Giá trị còn lại sau khi trích kỳ này
    /// </summary>
    public decimal GiaTriConLai { get; set; } = 0;

    /// <summary>
    /// Bút toán ghi sổ vào Sổ cái Core GL (Nợ 642 / Có 2141 hoặc Có 242)
    /// </summary>
    public long? ButToanId { get; set; }
    public virtual ButToan? ButToan { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
