namespace ninjaTax.Models.Entities;

/// <summary>
/// Phân loại tính chất vật tư hàng hóa
/// </summary>
public enum LoaiVatTuHangHoa
{
    VatTu = 1,          // Nguyên vật liệu (TK 152)
    CongCuDungCu = 2,   // Công cụ dụng cụ (TK 153)
    ThanhPham = 3,      // Thành phẩm sản xuất (TK 155)
    HangHoa = 4,        // Hàng hóa thương mại (TK 156)
    DichVu = 5          // Dịch vụ tiêu dùng / Vận chuyển (Không tính tồn kho)
}

/// <summary>
/// Danh mục Vật tư hàng hóa và dịch vụ chuẩn hóa ERP kế toán VAS/TT99.
/// Khóa chính bigint (long).
/// </summary>
public class VatTuHangHoa
{
    /// <summary>
    /// Khóa chính bigint
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Mã vật tư hàng hóa (VD: HH-CISCO-C9200, VT-THEP-PHI10)
    /// </summary>
    public string MaVatTu { get; set; } = string.Empty;

    /// <summary>
    /// Tên vật tư hàng hóa dịch vụ
    /// </summary>
    public string TenVatTu { get; set; } = string.Empty;

    /// <summary>
    /// Đơn vị tính cơ bản (Cái, Chiếc, Mét, Kg, Hộp...)
    /// </summary>
    public string DonViTinh { get; set; } = string.Empty;

    /// <summary>
    /// Phân loại nhóm vật tư hàng hóa
    /// </summary>
    public LoaiVatTuHangHoa LoaiVatTu { get; set; } = LoaiVatTuHangHoa.HangHoa;

    /// <summary>
    /// Khóa ngoại Tài khoản Kho ngầm định (TK 152, TK 1561...)
    /// </summary>
    public long? TaiKhoanKhoId { get; set; }
    public TaiKhoan? TaiKhoanKho { get; set; }

    /// <summary>
    /// Khóa ngoại Tài khoản Doanh thu ngầm định (TK 5111, 5112, 5113...)
    /// </summary>
    public long? TaiKhoanDoanhThuId { get; set; }
    public TaiKhoan? TaiKhoanDoanhThu { get; set; }

    /// <summary>
    /// Khóa ngoại Tài khoản Giá vốn ngầm định (TK 632)
    /// </summary>
    public long? TaiKhoanGiaVonId { get; set; }
    public TaiKhoan? TaiKhoanGiaVon { get; set; }

    /// <summary>
    /// Thuế suất VAT ngầm định (0, 5, 8, 10)
    /// </summary>
    public decimal ThueSuatVatMacDinh { get; set; } = 10m;

    /// <summary>
    /// Đơn giá mua gần nhất (decimal 19, 4)
    /// </summary>
    public decimal DonGiaMuaGanNhat { get; set; }

    /// <summary>
    /// Đơn giá bán tiêu chuẩn chưa VAT (decimal 19, 4)
    /// </summary>
    public decimal DonGiaBanTieuChuan { get; set; }

    /// <summary>
    /// Cờ theo dõi số lượng tồn kho (Dịch vụ = false)
    /// </summary>
    public bool DangTheoDoiTonKho { get; set; } = true;

    /// <summary>
    /// Trạng thái hoạt động (true: đang kinh doanh; false: ngừng kinh doanh)
    /// </summary>
    public bool DangHoatDong { get; set; } = true;
}
