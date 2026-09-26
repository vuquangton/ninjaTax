using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum TrangThaiBangLuong
{
    ChoDuyet = 0,
    DaDuyet = 1,
    DaGhiSo = 2,
    DaChiTra = 3
}

/// <summary>
/// Bảng lương tổng hợp toàn công ty theo tháng (Payroll Header).
/// </summary>
public class BangLuongThang
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SoChungTu { get; set; } = string.Empty; // BL-YYYY-MM

    [Required]
    [StringLength(10)]
    public string KyKeToan { get; set; } = string.Empty; // YYYY-MM

    public DateTime NgayLap { get; set; } = DateTime.Today;

    public DateTime? NgayGhiSo { get; set; }

    public int SoNgayCongChuan { get; set; } = 22;

    // Tổng hợp tài chính
    public decimal TongQuyLuong { get; set; } = 0;       // Nợ 642 / Có 334
    public decimal TongBaoHiemDnGanh { get; set; } = 0;  // Nợ 642 / Có 338 (23.5%)
    public decimal TongBaoHiemNldGanh { get; set; } = 0; // Nợ 334 / Có 338 (10.5%)
    public decimal TongThueTncn { get; set; } = 0;       // Nợ 334 / Có 3335
    public decimal TongThucLinh { get; set; } = 0;       // Nợ 334 / Có 1111, 1121

    // Liên kết Bút toán Sổ Cái Core GL TT99
    public long? ButToanChiPhiLuongId { get; set; }
    public virtual ButToan? ButToanChiPhiLuong { get; set; }

    public long? ButToanBaoHiemDnId { get; set; }
    public virtual ButToan? ButToanBaoHiemDn { get; set; }

    public long? ButToanKhauTruLuongId { get; set; }
    public virtual ButToan? ButToanKhauTruLuong { get; set; }

    public TrangThaiBangLuong TrangThai { get; set; } = TrangThaiBangLuong.ChoDuyet;

    public virtual ICollection<ChiTietLuongNhanVien> ChiTiets { get; set; } = new List<ChiTietLuongNhanVien>();

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }
}

/// <summary>
/// Chi tiết bảng tính lương từng nhân viên (Gross to Net & Payslip).
/// </summary>
public class ChiTietLuongNhanVien
{
    public long Id { get; set; }

    public long BangLuongThangId { get; set; }
    public virtual BangLuongThang? BangLuongThang { get; set; }

    public long NhanVienId { get; set; }
    public virtual NhanVien? NhanVien { get; set; }

    // Thu nhập
    public decimal LuongThoiGian { get; set; } = 0;
    public decimal LuongLamThemGio { get; set; } = 0;
    public decimal LuongOtMienThue { get; set; } = 0; // Phần chênh lệch lương OT vượt 100% được miễn thuế TNCN
    public decimal PhuCapChiuThue { get; set; } = 0;
    public decimal PhuCapMienThue { get; set; } = 0; // Ăn trưa <= 730k, trang phục...
    public decimal TienThuong { get; set; } = 0;
    public decimal TongThuNhap { get; set; } = 0;

    // Bảo hiểm trích trừ vào lương NLĐ (10.5%)
    public decimal LuongDongBaoHiem { get; set; } = 0;
    public decimal BhxhNld { get; set; } = 0; // 8%
    public decimal BhytNld { get; set; } = 0; // 1.5%
    public decimal BhtnNld { get; set; } = 0; // 1%
    public decimal TongBaoHiemNld { get; set; } = 0;

    // Bảo hiểm & KPCĐ Doanh nghiệp gánh tính vào chi phí 642 (23.5%)
    public decimal BhxhDn { get; set; } = 0;  // 17.5%
    public decimal BhytDn { get; set; } = 0;  // 3.0%
    public decimal BhtnDn { get; set; } = 0;  // 1.0%
    public decimal KpcdDn { get; set; } = 0;  // 2.0%
    public decimal TongBaoHiemDn { get; set; } = 0;

    // Thuế TNCN
    public decimal GiamTruBanThan { get; set; } = 11000000m;
    public decimal GiamTruNguoiPhuThuoc { get; set; } = 0;
    public decimal ThuNhapTinhThue { get; set; } = 0;
    public decimal ThueTncnKhauTru { get; set; } = 0;

    // Giảm trừ khác & Thực lĩnh
    public decimal TamUng { get; set; } = 0;
    public decimal ThucLinh { get; set; } = 0;
}
