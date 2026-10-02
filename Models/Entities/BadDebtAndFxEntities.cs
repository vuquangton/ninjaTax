namespace ninjaTax.Models.Entities;

public enum TrangThaiBangTrichLap
{
    TamTinh = 0,
    DaGhiSo = 1,
    DaHuy = 2
}

/// <summary>
/// Bảng trích lập dự phòng nợ phải thu khó đòi theo Thông tư 48/2019/TT-BTC
/// </summary>
public class BangTrichLapDuPhongNoPhaiThu
{
    public long Id { get; set; }

    public long ChiNhanhId { get; set; }
    public ChiNhanh? ChiNhanh { get; set; }

    public string SoChungTu { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    public decimal TongNoQuaHan { get; set; }
    public decimal TongSoDuPhongPhaiTrich { get; set; }
    public decimal SoDuDuPhongHienTai2293 { get; set; }
    public decimal SoTienTrichThem { get; set; }
    public decimal SoTienHoanNhap { get; set; }

    public TrangThaiBangTrichLap TrangThai { get; set; } = TrangThaiBangTrichLap.TamTinh;

    public long? ButToanId { get; set; }
    public ButToan? ButToan { get; set; }

    public string? GhiChu { get; set; }

    public ICollection<ChiTietTrichLapDuPhong> ChiTietTrichLaps { get; set; } = new List<ChiTietTrichLapDuPhong>();
}

/// <summary>
/// Chi tiết trích lập dự phòng từng khoản nợ phải thu quá hạn
/// </summary>
public class ChiTietTrichLapDuPhong
{
    public long Id { get; set; }

    public long BangTrichLapDuPhongNoPhaiThuId { get; set; }
    public BangTrichLapDuPhongNoPhaiThu? BangTrichLapDuPhong { get; set; }

    public long KhachHangId { get; set; }
    public DoiTuong? KhachHang { get; set; }

    public long? HoaDonBanHangId { get; set; }
    public HoaDonBanHang? HoaDonBanHang { get; set; }

    public string SoHoaDon { get; set; } = string.Empty;
    public DateTime NgayHoaDon { get; set; }
    public DateTime HanThanhToan { get; set; }
    public decimal SoTienConNo { get; set; }
    public int SoNgayQuaHan { get; set; }
    public decimal TyLeTrichLap { get; set; } // 30, 50, 70, 100
    public decimal SoTienDuPhong { get; set; }
    public string? LyDoDacBiet { get; set; }
}

/// <summary>
/// Đánh giá lại chênh lệch tỷ giá ngoại tệ cuối kỳ theo VAS 10 và TK 413
/// </summary>
public class DanhGiaLaiNgoaiTe
{
    public long Id { get; set; }

    public long ChiNhanhId { get; set; }
    public ChiNhanh? ChiNhanh { get; set; }

    public string SoChungTu { get; set; } = string.Empty;
    public DateTime NgayChungTu { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    public string LoaiTien { get; set; } = "USD";
    public decimal TyGiaMua { get; set; }
    public decimal TyGiaBan { get; set; }

    public decimal TongLaiTyGia { get; set; }
    public decimal TongLoTyGia { get; set; }
    public decimal ChenhLechThuan => TongLaiTyGia - TongLoTyGia;

    public long? ButToanDanhGiaLaiId { get; set; }
    public ButToan? ButToanDanhGiaLai { get; set; }

    public long? ButToanKetChuyen413Id { get; set; }
    public ButToan? ButToanKetChuyen413 { get; set; }

    public bool DaGhiSo => ButToanDanhGiaLaiId.HasValue;

    public ICollection<ChiTietDanhGiaLaiNgoaiTe> ChiTietDanhGiaLais { get; set; } = new List<ChiTietDanhGiaLaiNgoaiTe>();
}

public class ChiTietDanhGiaLaiNgoaiTe
{
    public long Id { get; set; }

    public long DanhGiaLaiNgoaiTeId { get; set; }
    public DanhGiaLaiNgoaiTe? DanhGiaLaiNgoaiTe { get; set; }

    public long TaiKhoanId { get; set; }
    public TaiKhoan? TaiKhoan { get; set; }

    public long? DoiTuongId { get; set; }
    public DoiTuong? DoiTuong { get; set; }

    public decimal SoDuNgoaiTe { get; set; }
    public decimal TyGiaGhiSo { get; set; }
    public decimal GiaTriGhiSoVnd { get; set; }
    public decimal TyGiaDanhGiaLai { get; set; }
    public decimal GiaTriDanhGiaLaiVnd { get; set; }
    public decimal ChenhLechVnd { get; set; } // Dương: Lãi, Âm: Lỗ
    public bool LaLai => ChenhLechVnd > 0;
}
