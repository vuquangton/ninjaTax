using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum LoaiXuatKho
{
    BanHang = 1,         // Xuất kho bán lẻ / bán buôn (Nợ 632 / Có 1561)
    SanXuat = 2,         // Xuất kho nguyên vật liệu để gia công, sản xuất (Nợ 154 / Có 152)
    SuDungNoiBo = 3,     // Xuất tiêu dùng nội bộ văn phòng, bán hàng (Nợ 6421, 6422 / Có 152, 153, 156)
    XuatKhac = 4,        // Xuất kiểm kê thiếu, hao hụt (Nợ 1381 / Có 152, 156)
    TraHangNhaCungCap = 5 // Xuất kho trả lại hàng mua cho nhà cung cấp (Nợ 331 / Có 1561)
}

/// <summary>
/// Chứng từ Phiếu Xuất Kho (Mẫu số 02-VT ban hành theo TT 200/2014/TT-BTC & TT 99/2025/TT-BTC).
/// </summary>
public class PhieuXuatKho
{
    public long Id { get; set; }

    [Required]
    public long ChiNhanhId { get; set; }
    public virtual ChiNhanh? ChiNhanh { get; set; }

    [Required]
    public long KhoId { get; set; }
    public virtual Kho? Kho { get; set; }

    [Required]
    [StringLength(50)]
    public string SoPhieu { get; set; } = string.Empty;

    public DateTime NgayXuat { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    public LoaiXuatKho LoaiXuatKho { get; set; } = LoaiXuatKho.BanHang;

    /// <summary>
    /// Liên kết hóa đơn bán hàng (nếu xuất kho theo HĐĐT)
    /// </summary>
    public long? HoaDonBanHangId { get; set; }
    public virtual HoaDonBanHang? HoaDonBanHang { get; set; }

    public long? KhachHangId { get; set; }
    public virtual DoiTuong? KhachHang { get; set; }

    /// <summary>
    /// Gắn Phòng Ban để phân bổ chi phí giá vốn/hoạt động theo phân khúc (Phase 7 - IFRS 8 / VAS 28)
    /// </summary>
    public long? PhongBanId { get; set; }
    public virtual PhongBan? PhongBan { get; set; }

    [StringLength(500)]
    public string DienGiai { get; set; } = string.Empty;

    public decimal TongSoLuong { get; set; }
    public decimal TongTienGiaVon { get; set; }

    public TrangThaiPhieuKho TrangThai { get; set; } = TrangThaiPhieuKho.TamTinh;

    public long? ButToanId { get; set; }
    public virtual ButToan? ButToan { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChiTietXuatKho> ChiTietXuatKhos { get; set; } = new List<ChiTietXuatKho>();
}

/// <summary>
/// Dòng chi tiết mặt hàng trên Phiếu Xuất Kho
/// </summary>
public class ChiTietXuatKho
{
    public long Id { get; set; }

    public long PhieuXuatKhoId { get; set; }
    public virtual PhieuXuatKho? PhieuXuatKho { get; set; }

    public long VatTuHangHoaId { get; set; }
    public virtual VatTuHangHoa? VatTuHangHoa { get; set; }

    public decimal SoLuong { get; set; }

    /// <summary>
    /// Đơn giá vốn xuất kho (Tính theo Bình quân gia quyền hoặc gán trực tiếp)
    /// </summary>
    public decimal DonGiaVon { get; set; }

    /// <summary>
    /// Tiền giá vốn xuất kho = SoLuong * DonGiaVon
    /// </summary>
    public decimal TienGiaVon { get; set; }

    public long TaiKhoanNoId { get; set; }
    public virtual TaiKhoan? TaiKhoanNo { get; set; }

    public long TaiKhoanCoId { get; set; }
    public virtual TaiKhoan? TaiKhoanCo { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    /// <summary>
    /// Đơn giá vốn bình quân gia quyền tính lại cuối kỳ (VAS 02)
    /// </summary>
    public decimal? DonGiaVonCuoiKy { get; set; }

    /// <summary>
    /// Chênh lệch giá vốn sau khi tính lại = (DonGiaVonCuoiKy - DonGiaVon) * SoLuong
    /// </summary>
    public decimal? ChenhLechGiaVon { get; set; }
}
