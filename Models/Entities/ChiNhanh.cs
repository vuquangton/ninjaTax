using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ninjaTax.Models.Entities;

public enum LoaiChiNhanh
{
    TruSoChinh = 1,
    PhuThuocCungTinh = 2,
    PhuThuocKhacTinh = 3,
    DocLap = 4
}

/// <summary>
/// Thực thể Đơn vị cơ sở / Chi nhánh (Branch / Organizational Unit).
/// Phục vụ phân bổ nghĩa vụ thuế và hạch toán đa chi nhánh theo NĐ 126/2020 & TT 80/2021.
/// </summary>
public class ChiNhanh
{
    public long Id { get; set; }

    public long DoanhNghiepId { get; set; }
    public virtual ThongTinDoanhNghiep? DoanhNghiep { get; set; }

    [Required]
    [MaxLength(50)]
    public string MaChiNhanh { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string TenChiNhanh { get; set; } = string.Empty;

    /// <summary>
    /// Mã số thuế chi nhánh (13 số định dạng XXXXXXXXXX-YYY hoặc 10 số đối với trụ sở chính).
    /// </summary>
    [MaxLength(20)]
    public string? MaSoThueChiNhanh { get; set; }

    public LoaiChiNhanh LoaiChiNhanh { get; set; } = LoaiChiNhanh.TruSoChinh;

    public bool KeKhaiThueGtgtRieng { get; set; } = false;
    public bool KeKhaiThueTncnRieng { get; set; } = false;

    [MaxLength(500)]
    public string? DiaChi { get; set; }

    [MaxLength(100)]
    public string? TinhThanhPho { get; set; }

    [MaxLength(20)]
    public string? MaCoQuanThueQuanLyRieng { get; set; }

    [MaxLength(255)]
    public string? TenCoQuanThueQuanLyRieng { get; set; }

    [MaxLength(255)]
    public string? NguoiDungDau { get; set; }

    [MaxLength(50)]
    public string? SoDienThoai { get; set; }

    public bool DangHoatDong { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual CauHinhHoaDonDienTu? CauHinhHddt { get; set; }

    /// <summary>
    /// Kiểm tra tính hợp lệ của Mã số thuế Chi nhánh 13 số: 10 số công ty mẹ + dấu gạch nối + 3 số (001-999).
    /// Ví dụ: 0109998888-001
    /// </summary>
    public static bool KiemTraMstChiNhanhHopLe(string? mstChiNhanh, string? mstCongTyMe)
    {
        if (string.IsNullOrWhiteSpace(mstChiNhanh)) return false;
        var trimmed = mstChiNhanh.Trim();

        // Nếu là chi nhánh 13 số dạng XXXXXXXXXX-YYY
        var match = Regex.Match(trimmed, @"^(\d{10})-(\d{3})$");
        if (!match.Success) return false;

        var prefixMst = match.Groups[1].Value;
        var suffixBranch = match.Groups[2].Value;

        // Nếu có MST công ty mẹ đối chiếu, 10 số đầu phải trùng khớp
        if (!string.IsNullOrWhiteSpace(mstCongTyMe))
        {
            var cleanMe = mstCongTyMe.Trim().Replace(" ", "").Replace("-", "");
            if (prefixMst != cleanMe) return false;
        }

        // Kiểm tra 10 số đầu hợp lệ theo Modulo 11
        if (!ThongTinDoanhNghiep.KiemTraMstHopLe(prefixMst)) return false;

        // Suffix chi nhánh từ 001 đến 999
        if (!int.TryParse(suffixBranch, out int branchNum) || branchNum < 1 || branchNum > 999) return false;

        return true;
    }
}

