namespace ninjaTax.Models.Entities;

/// <summary>
/// Loại điều chỉnh thương mại (Hàng bán trả lại, Hàng mua trả lại, Chiết khấu thương mại, Giảm giá hàng bán)
/// </summary>
public enum LoaiDieuChinhThuongMai
{
    HangBanTraLai = 1,          // Nợ 5212, Nợ 33311 / Có 131 (hoặc 111/112) VÀ Nợ 1561 / Có 632
    HangMuaTraLai = 2,          // Nợ 331 / Có 1561, Có 1331
    ChietKhauThuongMaiBan = 3,  // Nợ 5211, Nợ 33311 / Có 131
    GiamGiaHangMua = 4          // Nợ 331 / Có 1561 (hoặc 632), Có 1331
}

/// <summary>
/// Phương thức xử lý thanh toán / công nợ cho điều chỉnh thương mại
/// </summary>
public enum HinhThucXuLyDieuChinh
{
    GiamTruCongNo = 1,
    TienMat = 2,
    TienGuiNganHang = 3
}

/// <summary>
/// Trạng thái chứng từ điều chỉnh thương mại
/// </summary>
public enum TrangThaiDieuChinhThuongMai
{
    TamTinh = 0,
    DaGhiSo = 1,
    DaHuy = 2
}

/// <summary>
/// Chứng từ điều chỉnh thương mại (Hàng bán/mua trả lại, chiết khấu thương mại đạt doanh số)
/// </summary>
public class ChungTuDieuChinhThuongMai
{
    public long Id { get; set; }

    public long ChiNhanhId { get; set; }
    public ChiNhanh? ChiNhanh { get; set; }

    public LoaiDieuChinhThuongMai LoaiDieuChinh { get; set; } = LoaiDieuChinhThuongMai.HangBanTraLai;

    public string SoChungTu { get; set; } = string.Empty;
    public DateTime NgayChungTu { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    public long DoiTuongId { get; set; }
    public DoiTuong? DoiTuong { get; set; }

    public long? HoaDonBanHangGocId { get; set; }
    public HoaDonBanHang? HoaDonBanHangGoc { get; set; }

    public long? HoaDonMuaHangGocId { get; set; }
    public HoaDonMuaHang? HoaDonMuaHangGoc { get; set; }

    public long? KhoId { get; set; }
    public Kho? Kho { get; set; }

    public string? LyDo { get; set; }
    public HinhThucXuLyDieuChinh HinhThucXuLy { get; set; } = HinhThucXuLyDieuChinh.GiamTruCongNo;
    public TrangThaiDieuChinhThuongMai TrangThai { get; set; } = TrangThaiDieuChinhThuongMai.TamTinh;

    public decimal TongTienHang { get; set; }
    public decimal TongTienThueVat { get; set; }
    public decimal TongThanhToan { get; set; }
    public decimal TongGiaTriNhapLaiKho { get; set; }

    public long? ButToanDoanhThuCongNoId { get; set; }
    public ButToan? ButToanDoanhThuCongNo { get; set; }

    public long? ButToanGiaVonKhoId { get; set; }
    public ButToan? ButToanGiaVonKho { get; set; }

    public ICollection<ChiTietDieuChinhThuongMai> ChiTietDieuChinhs { get; set; } = new List<ChiTietDieuChinhThuongMai>();
}

/// <summary>
/// Dòng chi tiết mặt hàng điều chỉnh thương mại
/// </summary>
public class ChiTietDieuChinhThuongMai
{
    public long Id { get; set; }

    public long ChungTuDieuChinhThuongMaiId { get; set; }
    public ChungTuDieuChinhThuongMai? ChungTuDieuChinhThuongMai { get; set; }

    public int DongSo { get; set; } = 1;

    public long? VatTuHangHoaId { get; set; }
    public VatTuHangHoa? VatTuHangHoa { get; set; }

    public string DonViTinh { get; set; } = string.Empty;
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public decimal ThueSuatVat { get; set; }
    public decimal TienThueVat { get; set; }

    public decimal DonGiaVonNhapLai { get; set; }
    public decimal TienGiaVonNhapLai { get; set; }

    public long? TaiKhoanNoId { get; set; }
    public TaiKhoan? TaiKhoanNo { get; set; }

    public long? TaiKhoanCoId { get; set; }
    public TaiKhoan? TaiKhoanCo { get; set; }
}
