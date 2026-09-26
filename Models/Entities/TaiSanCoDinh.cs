using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum LoaiTaiSan
{
    TaiSanCoDinh = 1,    // TK 211: Nguyên giá >= 30.000.000 VNĐ, TGSD > 12 tháng (TT 45/2013/TT-BTC)
    CongCuDungCu = 2     // TK 242: Chi phí trả trước / CCDC phân bổ <= 36 tháng (TT 78/2014 & TT 96/2015)
}

public enum TrangThaiTaiSan
{
    DangSuDung = 1,
    DaKhauHaoHet = 2,
    GiamTaiSan = 3
}

/// <summary>
/// Quản lý Tài sản cố định (TK 211) và Công cụ dụng cụ (TK 242) theo chuẩn TT 45/2013 và TT99/2025.
/// </summary>
public class TaiSanCoDinh
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string MaTaiSan { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string TenTaiSan { get; set; } = string.Empty;

    public LoaiTaiSan LoaiTaiSan { get; set; } = LoaiTaiSan.TaiSanCoDinh;

    public DateTime NgayGhiTang { get; set; } = DateTime.Today;

    public DateTime NgayBatDauKhauHao { get; set; } = DateTime.Today;

    public decimal NguyenGia { get; set; } = 0;

    /// <summary>
    /// Thời gian trích khấu hao / phân bổ (tính theo tháng).
    /// Đối với CCDC (TK 242): Bắt buộc <= 36 tháng.
    /// Đối với TSCĐ (TK 211): Bắt buộc > 12 tháng.
    /// </summary>
    public int ThoiGianSuDungThang { get; set; } = 12;

    public decimal GiaTriDaKhauHao { get; set; } = 0;

    public decimal GiaTriConLai { get; set; } = 0;

    /// <summary>
    /// Mức trích khấu hao tiêu chuẩn mỗi tháng (NguyenGia / ThoiGianSuDungThang)
    /// </summary>
    public decimal MucKhauHaoThang { get; set; } = 0;

    /// <summary>
    /// TK Nguyên giá: 211x (nếu TSCĐ) hoặc 242 (nếu CCDC)
    /// </summary>
    public long TaiKhoanNguyenGiaId { get; set; }
    public virtual TaiKhoan? TaiKhoanNguyenGia { get; set; }

    /// <summary>
    /// TK Khấu hao lũy kế: 2141 (đối với TSCĐ) hoặc chính TK 242 (đối với CCDC phân bổ dần)
    /// </summary>
    public long? TaiKhoanKhauHaoId { get; set; }
    public virtual TaiKhoan? TaiKhoanKhauHao { get; set; }

    /// <summary>
    /// TK Chi phí tính khấu hao: 642 (Chi phí quản lý doanh nghiệp theo TT99)
    /// </summary>
    public long TaiKhoanChiPhiId { get; set; }
    public virtual TaiKhoan? TaiKhoanChiPhi { get; set; }

    [StringLength(255)]
    public string? BoPhanSuDung { get; set; }

    public TrangThaiTaiSan TrangThai { get; set; } = TrangThaiTaiSan.DangSuDung;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    public virtual ICollection<BangTinhKhauHao> BangTinhKhauHaos { get; set; } = new List<BangTinhKhauHao>();

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }
}
