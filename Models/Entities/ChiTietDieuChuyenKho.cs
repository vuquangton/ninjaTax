namespace ninjaTax.Models.Entities;

/// <summary>
/// Chi tiết mặt hàng trên Phiếu điều chuyển kho nội bộ.
/// </summary>
public class ChiTietDieuChuyenKho
{
    public long Id { get; set; }

    public long PhieuDieuChuyenKhoId { get; set; }
    public PhieuDieuChuyenKho? PhieuDieuChuyenKho { get; set; }

    public long VatTuHangHoaId { get; set; }
    public VatTuHangHoa? VatTuHangHoa { get; set; }

    public string DonViTinh { get; set; } = string.Empty;

    public decimal SoLuong { get; set; }
    public decimal DonGiaVon { get; set; }
    public decimal ThanhTien { get; set; }

    public long? TaiKhoanXuatId { get; set; }
    public TaiKhoan? TaiKhoanXuat { get; set; }

    public long? TaiKhoanNhapId { get; set; }
    public TaiKhoan? TaiKhoanNhap { get; set; }
}
