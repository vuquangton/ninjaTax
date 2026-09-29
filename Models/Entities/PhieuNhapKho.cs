using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum LoaiNhapKho
{
    MuaNgoai = 1,        // Nhập kho do mua ngoài (Nợ 152, 1561 / Có 331, 111, 112)
    TuSanXuat = 2,       // Nhập kho thành phẩm hoàn thành tự sản xuất (Nợ 155, 156 / Có 154)
    ThuHoi = 3,          // Thu hồi vật tư thừa từ công trình/phân xưởng (Nợ 152 / Có 154, 642)
    NhapKhac = 4         // Nhập kiểm kê thừa, quà biếu tặng (Nợ 152, 156 / Có 3381, 711)
}

public enum TrangThaiPhieuKho
{
    TamTinh = 0,         // Lưu nháp (Chưa ghi sổ kho, chưa sinh bút toán)
    DaGhiSo = 1,         // Đã ghi sổ (Đã cập nhật tồn kho và sinh bút toán trên Sổ cái)
    DaHuy = 2            // Đã hủy chứng từ
}

/// <summary>
/// Chứng từ Phiếu Nhập Kho (Mẫu số 01-VT ban hành theo TT 200/2014/TT-BTC & TT 99/2025/TT-BTC).
/// </summary>
public class PhieuNhapKho
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

    public DateTime NgayNhap { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    public LoaiNhapKho LoaiNhapKho { get; set; } = LoaiNhapKho.MuaNgoai;

    /// <summary>
    /// Liên kết hóa đơn mua hàng (nếu lập từ HĐĐT mua vào)
    /// </summary>
    public long? HoaDonMuaHangId { get; set; }
    public virtual HoaDonMuaHang? HoaDonMuaHang { get; set; }

    public long? NhaCungCapId { get; set; }
    public virtual DoiTuong? NhaCungCap { get; set; }

    [StringLength(500)]
    public string DienGiai { get; set; } = string.Empty;

    public decimal TongSoLuong { get; set; }
    public decimal TongTienHang { get; set; }

    public TrangThaiPhieuKho TrangThai { get; set; } = TrangThaiPhieuKho.TamTinh;

    public long? ButToanId { get; set; }
    public virtual ButToan? ButToan { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();
}

/// <summary>
/// Dòng chi tiết mặt hàng trên Phiếu Nhập Kho
/// </summary>
public class ChiTietNhapKho
{
    public long Id { get; set; }

    public long PhieuNhapKhoId { get; set; }
    public virtual PhieuNhapKho? PhieuNhapKho { get; set; }

    public long VatTuHangHoaId { get; set; }
    public virtual VatTuHangHoa? VatTuHangHoa { get; set; }

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public long TaiKhoanNoId { get; set; }
    public virtual TaiKhoan? TaiKhoanNo { get; set; }

    public long TaiKhoanCoId { get; set; }
    public virtual TaiKhoan? TaiKhoanCo { get; set; }

    [StringLength(50)]
    public string? SoLo { get; set; }

    public DateTime? HanSuDung { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    /// <summary>
    /// Tổng chi phí mua hàng được phân bổ vào dòng này (VAS 02)
    /// </summary>
    public decimal ChiPhiMuaHangPhanBo { get; set; } = 0m;

    /// <summary>
    /// Đơn giá sau phân bổ chi phí = (ThanhTien + ChiPhiMuaHangPhanBo) / SoLuong
    /// </summary>
    public decimal DonGiaSauPhanBo => SoLuong > 0 ? Math.Round((ThanhTien + ChiPhiMuaHangPhanBo) / SoLuong, 4) : DonGia;

    public virtual ICollection<ChiPhiMuaHangPhanBo> ChiPhiPhanBos { get; set; } = new List<ChiPhiMuaHangPhanBo>();
}
