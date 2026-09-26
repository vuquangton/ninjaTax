namespace ninjaTax.Models.Entities;

public enum LoaiCongNo
{
    PhaiThuKhachHang = 1, // TK 131
    PhaiTraNhaCungCap = 2  // TK 331
}

/// <summary>
/// Thực thể ghi nhận lịch sử đối trừ công nợ giữa Chứng từ thanh toán và Hóa đơn.
/// Phục vụ theo dõi tuổi nợ (Aging) và chi tiết công nợ hóa đơn.
/// </summary>
public class DoiTruCongNo
{
    public long Id { get; set; }

    public LoaiCongNo Loai { get; set; }

    public long DoiTuongId { get; set; }
    public DoiTuong? DoiTuong { get; set; }

    public DateTime NgayDoiTru { get; set; } = DateTime.Today;

    /// <summary>
    /// Hóa đơn bán hàng được đối trừ (nếu là công nợ phải thu)
    /// </summary>
    public long? HoaDonBanHangId { get; set; }
    public HoaDonBanHang? HoaDonBanHang { get; set; }

    /// <summary>
    /// Hóa đơn mua hàng được đối trừ (nếu là công nợ phải trả)
    /// </summary>
    public long? HoaDonMuaHangId { get; set; }
    public HoaDonMuaHang? HoaDonMuaHang { get; set; }

    /// <summary>
    /// Khóa ngoại Bút toán thanh toán (Phiếu thu, Phiếu chi, Giấy báo Có, Giấy báo Nợ)
    /// </summary>
    public long? ButToanId { get; set; }
    public ButToan? ButToan { get; set; }

    /// <summary>
    /// Số tiền đã được bù trừ thanh toán cho hóa đơn (decimal 19, 4)
    /// </summary>
    public decimal SoTienDoiTru { get; set; }

    public string? GhiChu { get; set; }
}
