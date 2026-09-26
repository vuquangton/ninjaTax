namespace ninjaTax.Models.Entities;

/// <summary>
/// Trạng thái của Hóa đơn điện tử theo Nghị định 123/2020/NĐ-CP
/// </summary>
public enum TrangThaiHddt
{
    MoiTao = 0,             // Bản nháp chưa ký
    DaKySo = 1,             // Đã ký số điện tử
    DaGuiCoQuanThue = 2,    // Đang chờ cơ quan thuế cấp mã
    CoQuanThueCapMa = 3,    // Hợp lệ, đã được cấp mã CQT
    CoQuanThueTuChoi = 4,   // Bị cơ quan thuế từ chối
    DaHuy = 5,              // Đã lập biên bản hủy
    BiDieuChinh = 6,        // Đã bị hóa đơn khác điều chỉnh
    BiThayThe = 7           // Đã bị hóa đơn khác thay thế
}

/// <summary>
/// Hóa đơn bán hàng / Hóa đơn điện tử bán ra theo chuẩn QĐ 1450 & TT99.
/// Tự động sinh cặp Bút toán: Doanh thu (131/511/3331) và Giá vốn (632/156) nếu kiêm xuất kho.
/// </summary>
public class HoaDonBanHang
{
    public long Id { get; set; }

    /// <summary>
    /// Số chứng từ hệ thống (VD: BH-2026-00001)
    /// </summary>
    public string SoChungTu { get; set; } = string.Empty;

    public DateTime NgayChungTu { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    /// <summary>
    /// Ký hiệu mẫu số hóa đơn điện tử (VD: 1C26TBB)
    /// </summary>
    public string KHMauSo { get; set; } = "1C26TBB";

    /// <summary>
    /// Ký hiệu hóa đơn điện tử (VD: C26T)
    /// </summary>
    public string KyHieu { get; set; } = "C26T";

    /// <summary>
    /// Số hóa đơn điện tử chính thức (8 chữ số, VD: 00000001)
    /// </summary>
    public string SoHoaDon { get; set; } = string.Empty;

    public DateTime NgayHoaDon { get; set; } = DateTime.Today;

    /// <summary>
    /// Mã của cơ quan thuế cấp (cho hóa đơn có mã của CQT)
    /// </summary>
    public string? MaCoQuanThue { get; set; }

    /// <summary>
    /// Chuỗi chữ ký số điện tử (X.509 Certificate Hash)
    /// </summary>
    public string? ChuKySo { get; set; }

    public TrangThaiHddt TrangThai { get; set; } = TrangThaiHddt.MoiTao;

    /// <summary>
    /// Khóa ngoại Khách hàng
    /// </summary>
    public long KhachHangId { get; set; }
    public DoiTuong? KhachHang { get; set; }

    public string? MaSoThueKH { get; set; }
    public string? TenKhachHang { get; set; }
    public string? DiaChiKH { get; set; }

    public string DienGiai { get; set; } = string.Empty;

    /// <summary>
    /// Hạn thanh toán (phục vụ theo dõi tuổi nợ AR Aging)
    /// </summary>
    public DateTime HanThanhToan { get; set; } = DateTime.Today.AddDays(15);

    // Tiền tệ (Độ chính xác decimal 19, 4)
    public decimal TongTienHang { get; set; }
    public decimal TongTienChietKhau { get; set; }
    public decimal TongTienThueVat { get; set; }
    public decimal TongTienThue => TongTienThueVat;
    public decimal TongThanhToan { get; set; }
    public decimal DaThuTien { get; set; }
    public decimal ConPhaiThu => TongThanhToan - DaThuTien;
    public bool DaGhiSo => ButToanDoanhThuId.HasValue;

    /// <summary>
    /// Cờ bán hàng kiêm xuất kho (true: tự động sinh phiếu xuất kho và hạch toán giá vốn 632/156)
    /// </summary>
    public bool BanHangKiemXuatKho { get; set; } = true;

    /// <summary>
    /// Bút toán ghi nhận Doanh thu & Thuế VAT đầu ra
    /// </summary>
    public long? ButToanDoanhThuId { get; set; }
    public ButToan? ButToanDoanhThu { get; set; }

    /// <summary>
    /// Bút toán ghi nhận Giá vốn hàng bán (Nợ 632 / Có 156)
    /// </summary>
    public long? ButToanGiaVonId { get; set; }
    public ButToan? ButToanGiaVon { get; set; }

    public ICollection<ChiTietHoaDonBan> ChiTietBans { get; set; } = new List<ChiTietHoaDonBan>();
}

/// <summary>
/// Chi tiết dòng hàng hóa, dịch vụ xuất bán trên hóa đơn
/// </summary>
public class ChiTietHoaDonBan
{
    public long Id { get; set; }
    public long HoaDonBanHangId { get; set; }
    public HoaDonBanHang? HoaDonBanHang { get; set; }

    public int DongSo { get; set; } = 1;

    public long VatTuHangHoaId { get; set; }
    public VatTuHangHoa? VatTuHangHoa { get; set; }

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public decimal TiLeChietKhau { get; set; }
    public decimal TienChietKhau { get; set; }

    public decimal ThueSuatVat { get; set; }
    public decimal TienThueVat { get; set; }

    // Hạch toán Doanh thu
    public long TaiKhoanNoId { get; set; }       // 131, 111, 112
    public TaiKhoan? TaiKhoanNo { get; set; }

    public long TaiKhoanDoanhThuId { get; set; } // 5111, 5112
    public TaiKhoan? TaiKhoanDoanhThu { get; set; }

    public long TaiKhoanThueId { get; set; }     // 33311
    public TaiKhoan? TaiKhoanThue { get; set; }

    // Hạch toán Giá vốn (nếu kiêm xuất kho)
    public decimal DonGiaVon { get; set; }
    public decimal TienGiaVon => SoLuong * DonGiaVon;

    public long? TaiKhoanGiaVonId { get; set; }  // 632
    public TaiKhoan? TaiKhoanGiaVon { get; set; }

    public long? TaiKhoanKhoId { get; set; }     // 1561, 155
    public TaiKhoan? TaiKhoanKho { get; set; }
}
