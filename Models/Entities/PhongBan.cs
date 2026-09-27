using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum LoaiPhongBan
{
    QuanLy = 1,          // Khối Văn phòng, Ban Giám Đốc, Kế toán, Nhân sự (TK chi phí: 6422)
    BanHang = 2,         // Khối Kinh doanh, Marketing, Bán hàng (TK chi phí: 6421)
    SanXuat = 3,         // Phân xưởng sản xuất, Vận hành, Kỹ thuật (TK chi phí: 154)
    KhoaChuyenMon = 4,   // Khoa đào tạo, Bộ môn viện nghiên cứu, Phòng khám y tế (TK chi phí: 154)
    Khac = 5             // Khác (TK chi phí: 6422)
}

public static class LoaiPhongBanExtensions
{
    public static string LayMaTaiKhoanChiPhiMacDinh(this LoaiPhongBan loai)
    {
        return loai switch
        {
            LoaiPhongBan.BanHang => "6421",
            LoaiPhongBan.SanXuat => "154",
            LoaiPhongBan.KhoaChuyenMon => "154",
            _ => "6422"
        };
    }
}

/// <summary>
/// Thực thể Phòng Ban, Khoa chuyên môn & Trung tâm chi phí (Cost Center / Profit Center).
/// Phục vụ phân bổ chi phí lương (TT99) và Báo cáo tài chính bộ phận (IFRS 8 / VAS 28).
/// </summary>
public class PhongBan
{
    public long Id { get; set; }

    public long ChiNhanhId { get; set; }
    public virtual ChiNhanh? ChiNhanh { get; set; }

    public long? PhongBanChaId { get; set; }
    public virtual PhongBan? PhongBanCha { get; set; }
    public virtual ICollection<PhongBan> PhongBanCons { get; set; } = new List<PhongBan>();

    [Required(ErrorMessage = "Mã phòng ban là bắt buộc")]
    [StringLength(50)]
    public string MaPhongBan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
    [StringLength(255)]
    public string TenPhongBan { get; set; } = string.Empty;

    [StringLength(255)]
    public string? TenTiengAnh { get; set; }

    public LoaiPhongBan LoaiPhongBan { get; set; } = LoaiPhongBan.QuanLy;

    /// <summary>
    /// Mã tài khoản chi phí định tuyến mặc định (vd: 6421, 6422, 154)
    /// </summary>
    [StringLength(20)]
    public string? MaTaiKhoanChiPhi { get; set; }

    public long? TruongPhongId { get; set; }
    public virtual NhanVien? TruongPhong { get; set; }

    /// <summary>
    /// true: Trung tâm lợi nhuận (Profit Center - phát sinh cả Doanh thu 511 và Chi phí 6xx).
    /// false: Trung tâm chi phí thuần túy (Cost Center).
    /// </summary>
    public bool LaTrungTamLoiNhuan { get; set; } = false;

    public bool DangHoatDong { get; set; } = true;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
    public virtual ICollection<ChiTietButToan> ChiTietButToans { get; set; } = new List<ChiTietButToan>();

    /// <summary>
    /// Kiểm tra thuật toán chống chu trình phân cấp cây phòng ban (Anti-cyclic tree validation).
    /// Trả về true nếu gán proposedParentId cho currentId sẽ tạo thành chu trình khép kín.
    /// </summary>
    public static bool KiemTraChuTrinhDeQuy(long currentId, long? proposedParentId, Func<long, long?> getParentIdFunc)
    {
        if (currentId <= 0 || !proposedParentId.HasValue)
        {
            return false;
        }

        // Tự trỏ chính nó
        if (proposedParentId.Value == currentId)
        {
            return true;
        }

        var visited = new HashSet<long> { currentId };
        long? currentAncestorId = proposedParentId.Value;

        while (currentAncestorId.HasValue && currentAncestorId.Value > 0)
        {
            if (currentAncestorId.Value == currentId)
            {
                return true; // Phát hiện chu trình khép kín
            }

            if (!visited.Add(currentAncestorId.Value))
            {
                // Chu trình vòng lặp ngoài cây hiện tại
                return true;
            }

            currentAncestorId = getParentIdFunc(currentAncestorId.Value);
        }

        return false;
    }

    /// <summary>
    /// Xác thực gán phòng ban cha, ném InvalidOperationException nếu phát hiện chu trình.
    /// </summary>
    public static void XacThucPhongBanCha(long currentId, long? proposedParentId, Func<long, long?> getParentIdFunc)
    {
        if (KiemTraChuTrinhDeQuy(currentId, proposedParentId, getParentIdFunc))
        {
            throw new InvalidOperationException("Không thể chọn phòng ban cha do tạo thành chu trình khép kín (Cyclic dependency).");
        }
    }
}
