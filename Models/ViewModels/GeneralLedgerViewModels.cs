namespace ninjaTax.Models.ViewModels;

/// <summary>
/// Dòng chi tiết Sổ Nhật ký chung (Mẫu S03a-DN theo Thông tư 99/2025/TT-BTC)
/// </summary>
public class DongSoNhatKyChungViewModel
{
    public long Id { get; set; }
    public DateTime NgayHachToan { get; set; }
    public DateTime NgayChungTu { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
    public string DienGiai { get; set; } = string.Empty;
    public string MaTaiKhoanNo { get; set; } = string.Empty;
    public string MaTaiKhoanCo { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string? DoiTuong { get; set; }
    public string? PhongBan { get; set; }
}

/// <summary>
/// Sổ Nhật ký chung (Mẫu S03a-DN)
/// </summary>
public class SoNhatKyChungViewModel
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public decimal TongPhatSinh { get; set; }
    public int TongSoDong { get; set; }
    public List<DongSoNhatKyChungViewModel> DongChiTiets { get; set; } = new();
}

/// <summary>
/// Dòng phát sinh Sổ Cái tài khoản (Mẫu S03b-DN)
/// </summary>
public class DongSoCaiViewModel
{
    public long ButToanId { get; set; }
    public DateTime NgayHachToan { get; set; }
    public DateTime NgayChungTu { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
    public string DienGiai { get; set; } = string.Empty;
    public string TaiKhoanDoiUng { get; set; } = string.Empty;
    public decimal PhatSinhNo { get; set; }
    public decimal PhatSinhCo { get; set; }
    public decimal SoDuLuyKe { get; set; }
}

/// <summary>
/// Sổ Cái một tài khoản kế toán (Mẫu S03b-DN theo Thông tư 99/2025/TT-BTC)
/// </summary>
public class SoCaiViewModel
{
    public string MaTaiKhoan { get; set; } = string.Empty;
    public string TenTaiKhoan { get; set; } = string.Empty;
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public decimal DuNoDauKy { get; set; }
    public decimal DuCoDauKy { get; set; }
    public decimal TongPhatSinhNo { get; set; }
    public decimal TongPhatSinhCo { get; set; }
    public decimal DuNoCuoiKy { get; set; }
    public decimal DuCoCuoiKy { get; set; }
    public List<DongSoCaiViewModel> DongChiTiets { get; set; } = new();
}

/// <summary>
/// Dòng Bảng Cân đối số phát sinh các tài khoản (Trial Balance 8 cột)
/// </summary>
public class DongBangCanDoiTaiKhoanViewModel
{
    public string MaTaiKhoan { get; set; } = string.Empty;
    public string TenTaiKhoan { get; set; } = string.Empty;
    public int BacTaiKhoan { get; set; } = 1;
    public bool LaTaiKhoanSoCai { get; set; }

    public decimal DuNoDauKy { get; set; }
    public decimal DuCoDauKy { get; set; }

    public decimal PhatSinhNoTrongKy { get; set; }
    public decimal PhatSinhCoTrongKy { get; set; }

    public decimal DuNoCuoiKy { get; set; }
    public decimal DuCoCuoiKy { get; set; }
}

/// <summary>
/// Bảng Cân đối tài khoản (Trial Balance 8 cột chuẩn TT99)
/// Bất biến: 
/// - Tổng Dư Nợ Đầu Kỳ == Tổng Dư Có Đầu Kỳ
/// - Tổng Phát Sinh Nợ Trong Kỳ == Tổng Phát Sinh Có Trong Kỳ
/// - Tổng Dư Nợ Cuối Kỳ == Tổng Dư Có Cuối Kỳ
/// </summary>
public class BangCanDoiTaiKhoanViewModel
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }

    public decimal TongDuNoDauKy { get; set; }
    public decimal TongDuCoDauKy { get; set; }

    public decimal TongPhatSinhNoTrongKy { get; set; }
    public decimal TongPhatSinhCoTrongKy { get; set; }

    public decimal TongDuNoCuoiKy { get; set; }
    public decimal TongDuCoCuoiKy { get; set; }

    public bool CanDoiDauKy => Math.Abs(TongDuNoDauKy - TongDuCoDauKy) < 0.0001m;
    public bool CanDoiPhatSinh => Math.Abs(TongPhatSinhNoTrongKy - TongPhatSinhCoTrongKy) < 0.0001m;
    public bool CanDoiCuoiKy => Math.Abs(TongDuNoCuoiKy - TongDuCoCuoiKy) < 0.0001m;
    public bool CanDoiHoanToan => CanDoiDauKy && CanDoiPhatSinh && CanDoiCuoiKy;

    public List<DongBangCanDoiTaiKhoanViewModel> DanhSachTaiKhoan { get; set; } = new();
}

/// <summary>
/// Kết quả chạy Kết chuyển tự động cuối kỳ (Period Closing)
/// </summary>
public class KetChuyenCuoiKyResult
{
    public bool ThanhCong { get; set; }
    public string? ThongBao { get; set; }
    public long? ButToanKetChuyenId { get; set; }
    public string? SoChungTuKetChuyen { get; set; }
    public decimal TongDoanhThuKetChuyen { get; set; }
    public decimal TongChiPhiKetChuyen { get; set; }
    public decimal LoiNhuanSauThueKetChuyen { get; set; }
    public List<string> NhatKyKetChuyen { get; set; } = new();
}

/// <summary>
/// Dòng đối soát Subledger (Sổ phụ) vs Sổ Cái (GL)
/// </summary>
public class DongDoiSoatSubledgerViewModel
{
    public string PhanHe { get; set; } = string.Empty; // Công nợ Phải thu / Công nợ Phải trả / Kho / TSCĐ
    public string MaTaiKhoan { get; set; } = string.Empty;
    public string TenTaiKhoan { get; set; } = string.Empty;
    public decimal SoDuSoPhuSubledger { get; set; }
    public decimal SoDuSoCaiGl { get; set; }
    public decimal ChenhLech => Math.Abs(SoDuSoPhuSubledger - SoDuSoCaiGl);
    public bool KhopSoLieu => ChenhLech < 0.01m;
    public string GhiChu { get; set; } = string.Empty;
}

/// <summary>
/// Báo cáo Kiểm tra Đối soát Tính Toàn vẹn Đa Phân hệ (Subledger vs GL Integrity Report)
/// </summary>
public class SubledgerReconciliationReportViewModel
{
    public DateTime MocThoiGian { get; set; }
    public bool ToanBoKhopSoLieu => DanhSachDoiSoat.All(d => d.KhopSoLieu);
    public int TongSoMucKiemTra => DanhSachDoiSoat.Count;
    public int SoMucLech => DanhSachDoiSoat.Count(d => !d.KhopSoLieu);
    public List<DongDoiSoatSubledgerViewModel> DanhSachDoiSoat { get; set; } = new();
}
