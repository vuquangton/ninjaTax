using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum LoaiChungTuThuChi
{
    ThuTienMat = 1,      // Phiếu thu (TK 1111)
    ChiTienMat = 2,      // Phiếu chi (TK 1111)
    BaoCoNganHang = 3,   // Giấy báo Có ngân hàng (TK 1121)
    UyNhiemChi = 4       // Giấy báo Nợ / Ủy nhiệm chi (TK 1121)
}

public enum TrangThaiThuChi
{
    ChoGhiSo = 0,
    DaGhiSo = 1,
    DaHuy = 2
}

/// <summary>
/// Chứng từ Thu - Chi Tiền mặt & Tiền gửi Ngân hàng theo TT99/2025/TT-BTC.
/// </summary>
public class ChungTuThuChi
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SoChungTu { get; set; } = string.Empty;

    public LoaiChungTuThuChi LoaiChungTu { get; set; }

    public DateTime NgayChungTu { get; set; } = DateTime.Today;

    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    [StringLength(100)]
    public string SoChungTuGoc { get; set; } = string.Empty;

    public long? DoiTuongId { get; set; }
    public virtual DoiTuong? DoiTuong { get; set; }

    [StringLength(255)]
    public string? NguoiGiaoNopNhan { get; set; }

    [StringLength(500)]
    public string? DiaChi { get; set; }

    [StringLength(500)]
    public string? LyDo { get; set; }

    /// <summary>
    /// Tài khoản ngân hàng doanh nghiệp (bắt buộc đối với Báo Có và Ủy Nhiệm Chi)
    /// </summary>
    public long? TaiKhoanNganHangId { get; set; }
    public virtual TaiKhoanNganHang? TaiKhoanNganHang { get; set; }

    public decimal TongTien { get; set; } = 0;

    /// <summary>
    /// Cờ cảnh báo vi phạm quy định thanh toán không dùng tiền mặt đối với HĐ từ 20 triệu VNĐ (TT 219/2013 & TT 26/2015)
    /// </summary>
    public bool ViPhamQuyTac20Tr { get; set; } = false;

    public TrangThaiThuChi TrangThai { get; set; } = TrangThaiThuChi.ChoGhiSo;

    /// <summary>
    /// Liên kết Bút toán Sổ Cái Core GL (khi đã ghi sổ)
    /// </summary>
    public long? ButToanId { get; set; }
    public virtual ButToan? ButToan { get; set; }

    /// <summary>
    /// Tùy chọn liên kết trực tiếp Hóa đơn Bán hàng (đối trừ công nợ khách hàng 131)
    /// </summary>
    public long? HoaDonBanHangId { get; set; }
    public virtual HoaDonBanHang? HoaDonBanHang { get; set; }

    /// <summary>
    /// Tùy chọn liên kết trực tiếp Hóa đơn Mua hàng (thanh toán công nợ nhà cung cấp 331)
    /// </summary>
    public long? HoaDonMuaHangId { get; set; }
    public virtual HoaDonMuaHang? HoaDonMuaHang { get; set; }

    public virtual ICollection<ChiTietChungTuThuChi> ChiTietThuChis { get; set; } = new List<ChiTietChungTuThuChi>();

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }
}
