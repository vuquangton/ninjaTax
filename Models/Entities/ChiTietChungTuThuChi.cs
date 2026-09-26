using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

/// <summary>
/// Chi tiết hạch toán dòng nghiệp vụ Thu - Chi tiền mặt, tiền gửi (Định khoản Nợ / Có).
/// </summary>
public class ChiTietChungTuThuChi
{
    public long Id { get; set; }

    public long ChungTuThuChiId { get; set; }
    public virtual ChungTuThuChi? ChungTuThuChi { get; set; }

    [StringLength(500)]
    public string DienGiai { get; set; } = string.Empty;

    public long TaiKhoanNoId { get; set; }
    public virtual TaiKhoan? TaiKhoanNo { get; set; }

    public long TaiKhoanCoId { get; set; }
    public virtual TaiKhoan? TaiKhoanCo { get; set; }

    public decimal SoTien { get; set; } = 0;

    /// <summary>
    /// Đối tượng chi tiết theo dõi công nợ (131, 331, 141)
    /// </summary>
    public long? DoiTuongId { get; set; }
    public virtual DoiTuong? DoiTuong { get; set; }
}
