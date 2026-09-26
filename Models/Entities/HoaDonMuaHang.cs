namespace ninjaTax.Models.Entities;

/// <summary>
/// Trạng thái của chứng từ / Hóa đơn mua hàng
/// </summary>
public enum TrangThaiHoaDonMua
{
    ChoNhanHang = 0,
    DaNhanHangChuaHoaDon = 1,
    DaNhanHoaDon = 2,
    DaGhiSo = 3,
    DaHuy = 4
}

/// <summary>
/// Hóa đơn mua hàng / Chứng từ mua hàng hóa, dịch vụ đầu vào.
/// Tích hợp dữ liệu HĐĐT Nghị định 123/2020/NĐ-CP và TT99.
/// </summary>
public class HoaDonMuaHang
{
    public long Id { get; set; }

    /// <summary>
    /// Số chứng từ hệ thống (VD: MH-2026-00001)
    /// </summary>
    public string SoChungTu { get; set; } = string.Empty;

    public DateTime NgayChungTu { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    /// <summary>
    /// Ký hiệu mẫu số hóa đơn (VD: 1C26TAA)
    /// </summary>
    public string KHMauSoHoaDon { get; set; } = "1C26TAA";

    /// <summary>
    /// Ký hiệu hóa đơn (VD: C26T)
    /// </summary>
    public string KyHieuHoaDon { get; set; } = "C26T";

    /// <summary>
    /// Số hóa đơn điện tử đầu vào (8 chữ số, VD: 00001234)
    /// </summary>
    public string SoHoaDon { get; set; } = string.Empty;

    public DateTime NgayHoaDon { get; set; } = DateTime.Today;

    /// <summary>
    /// Mã tra cứu hóa đơn điện tử của nhà cung cấp
    /// </summary>
    public string? MaTraCuuHdt { get; set; }

    /// <summary>
    /// Khóa ngoại Nhà cung cấp
    /// </summary>
    public long NhaCungCapId { get; set; }
    public DoiTuong? NhaCungCap { get; set; }

    public string? MaSoThueNCC { get; set; }
    public string? TenNCC { get; set; }
    public string? DiaChiNCC { get; set; }

    public string DienGiai { get; set; } = string.Empty;

    /// <summary>
    /// Hạn thanh toán tiền hàng (phục vụ theo dõi tuổi nợ AP Aging)
    /// </summary>
    public DateTime HanThanhToan { get; set; } = DateTime.Today.AddDays(30);

    // Giá trị tiền tệ (Độ chính xác decimal 19, 4)
    public decimal TongTienHang { get; set; }
    public decimal TongTienChietKhau { get; set; }
    public decimal TongTienThueVat { get; set; }
    public decimal TongTienThue => TongTienThueVat;
    public decimal TongThanhToan { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ConPhaiTra => TongThanhToan - DaThanhToan;
    public bool DaGhiSo => ButToanId.HasValue || TrangThai == TrangThaiHoaDonMua.DaGhiSo;

    /// <summary>
    /// Cờ mua hàng kiêm nhập kho (true: tự động tăng tồn kho vật tư)
    /// </summary>
    public bool MuaHangKiemKho { get; set; } = true;

    public TrangThaiHoaDonMua TrangThai { get; set; } = TrangThaiHoaDonMua.DaNhanHoaDon;

    /// <summary>
    /// Khóa ngoại Bút toán Sổ Cái tự động sinh trong Core GL
    /// </summary>
    public long? ButToanId { get; set; }
    public ButToan? ButToan { get; set; }

    public ICollection<ChiTietHoaDonMua> ChiTietHangs { get; set; } = new List<ChiTietHoaDonMua>();
}

/// <summary>
/// Chi tiết dòng hàng hóa, dịch vụ trên hóa đơn mua
/// </summary>
public class ChiTietHoaDonMua
{
    public long Id { get; set; }
    public long HoaDonMuaHangId { get; set; }
    public HoaDonMuaHang? HoaDonMuaHang { get; set; }

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

    // Tài khoản định khoản chi tiết dòng theo TT99
    public long TaiKhoanNoId { get; set; }      // 152, 1561...
    public TaiKhoan? TaiKhoanNo { get; set; }

    public long TaiKhoanThueId { get; set; }    // 1331
    public TaiKhoan? TaiKhoanThue { get; set; }

    public long TaiKhoanCoId { get; set; }      // 331, 1111, 1121...
    public TaiKhoan? TaiKhoanCo { get; set; }
}
