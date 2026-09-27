using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ninjaTax.Models.Entities;

/// <summary>
/// Thực thể Hồ sơ Pháp nhân Doanh nghiệp (Company Profile).
/// Tuân thủ Luật Doanh nghiệp 2020 và Luật Kế toán 2015.
/// </summary>
public class ThongTinDoanhNghiep
{
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string MaDoanhNghiep { get; set; } = "DN01";

    [Required]
    [MaxLength(255)]
    public string TenDoanhNghiep { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? TenGiaoDich { get; set; }

    [MaxLength(255)]
    public string? TenTiengAnh { get; set; }

    /// <summary>
    /// Mã số thuế chuẩn quốc gia 10 chữ số (Mod 11 checksum).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string MaSoThue { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string DiaChiTruSo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TinhThanhPho { get; set; }

    [MaxLength(100)]
    public string? QuanHuyen { get; set; }

    [MaxLength(20)]
    public string? MaCoQuanThueQuanLy { get; set; }

    [MaxLength(255)]
    public string? TenCoQuanThueQuanLy { get; set; }

    [MaxLength(255)]
    public string? NguoiDaiDienPhapLuat { get; set; }

    [MaxLength(100)]
    public string? ChucDanhNguoiDaiDien { get; set; } = "Tổng Giám Đốc";

    [MaxLength(255)]
    public string? GiamDoc { get; set; }

    [MaxLength(255)]
    public string? KeToanTruong { get; set; }

    [MaxLength(255)]
    public string? NguoiLapBieu { get; set; }

    [MaxLength(255)]
    public string? ThuQuy { get; set; }

    [MaxLength(50)]
    public string? SoDienThoai { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? Website { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal VonDieuLe { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public DateTime NgayThanhLap { get; set; } = DateTime.Today;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChiNhanh> ChiNhanhs { get; set; } = new List<ChiNhanh>();
    public virtual CauHinhKeToan? CauHinhKeToan { get; set; }

    /// <summary>
    /// Kiểm tra tính hợp lệ của Mã số thuế 10 số theo thuật toán Modulo 11 của Tổng cục Thuế Việt Nam.
    /// Trọng số: 31, 29, 23, 19, 17, 13, 7, 5, 3
    /// </summary>
    public static bool KiemTraMstHopLe(string? mst)
    {
        if (string.IsNullOrWhiteSpace(mst)) return false;
        var cleanMst = mst.Trim().Replace(" ", "").Replace("-", "");

        // Chỉ xét MST doanh nghiệp chính (10 số)
        if (cleanMst.Length != 10 || !cleanMst.All(char.IsDigit)) return false;

        int[] weights = [31, 29, 23, 19, 17, 13, 7, 5, 3];
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (cleanMst[i] - '0') * weights[i];
        }

        int remainder = sum % 11;
        int checkDigit = 10 - remainder;

        // Nếu 10 - remainder == 10, chữ số kiểm tra là 0
        if (checkDigit == 10) checkDigit = 0;

        return (cleanMst[9] - '0') == checkDigit;
    }
}

