using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class BangLuongIndexViewModel
{
    public string KyKeToan { get; set; } = string.Empty;
    public List<BangLuongThangItemViewModel> DanhSachBangLuong { get; set; } = new();
}

public class BangLuongThangItemViewModel
{
    public long Id { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
    public string KyKeToan { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; }
    public decimal TongQuyLuong { get; set; }
    public decimal TongBaoHiemDnGanh { get; set; }
    public decimal TongBaoHiemNldGanh { get; set; }
    public decimal TongThueTncn { get; set; }
    public decimal TongThucLinh { get; set; }
    public TrangThaiBangLuong TrangThai { get; set; }
    public int SoNhanVien { get; set; }
}

public class BangLuongChiTietViewModel
{
    public long BangLuongId { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
    public string KyKeToan { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; }
    public int SoNgayCongChuan { get; set; }
    public TrangThaiBangLuong TrangThai { get; set; }

    public decimal TongQuyLuong { get; set; }
    public decimal TongBaoHiemDnGanh { get; set; }
    public decimal TongBaoHiemNldGanh { get; set; }
    public decimal TongThueTncn { get; set; }
    public decimal TongThucLinh { get; set; }

    public string? SoButToanLuong { get; set; }
    public string? SoButToanBhDn { get; set; }
    public string? SoButToanKhauTru { get; set; }

    public List<DongLuongNhanVienViewModel> ChiTiets { get; set; } = new();
}

public class DongLuongNhanVienViewModel
{
    public long NhanVienId { get; set; }
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? PhongBan { get; set; }
    public string? ChucVu { get; set; }
    public LoaiHopDongLaoDong LoaiHopDong { get; set; }

    public decimal LuongCoBan { get; set; }
    public decimal LuongThoiGian { get; set; }
    public decimal LuongLamThemGio { get; set; }
    public decimal LuongOtMienThue { get; set; }
    public decimal PhuCapChiuThue { get; set; }
    public decimal PhuCapMienThue { get; set; }
    public decimal TienThuong { get; set; }
    public decimal TongThuNhap { get; set; }

    public decimal LuongDongBaoHiem { get; set; }
    public decimal BhxhNld { get; set; }
    public decimal BhytNld { get; set; }
    public decimal BhtnNld { get; set; }
    public decimal TongBaoHiemNld { get; set; }

    public decimal BhxhDn { get; set; }
    public decimal BhytDn { get; set; }
    public decimal BhtnDn { get; set; }
    public decimal KpcdDn { get; set; }
    public decimal TongBaoHiemDn { get; set; }

    public decimal GiamTruBanThan { get; set; }
    public decimal GiamTruNguoiPhuThuoc { get; set; }
    public decimal ThuNhapTinhThue { get; set; }
    public decimal ThueTncnKhauTru { get; set; }

    public decimal TamUng { get; set; }
    public decimal ThucLinh { get; set; }
}

public class PayslipViewModel
{
    public string KyKeToan { get; set; } = string.Empty;
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? PhongBan { get; set; }
    public string? ChucVu { get; set; }
    public int SoNguoiPhuThuoc { get; set; }
    public decimal SoNgayCongThucTe { get; set; }
    public int SoNgayCongChuan { get; set; }

    public decimal LuongCoBan { get; set; }
    public decimal LuongThoiGian { get; set; }
    public decimal LuongLamThemGio { get; set; }
    public decimal LuongOtMienThue { get; set; }
    public decimal PhuCapAnTrua { get; set; }
    public decimal PhuCapKhac { get; set; }
    public decimal TienThuong { get; set; }
    public decimal TongThuNhap { get; set; }

    public decimal BhxhNld { get; set; }
    public decimal BhytNld { get; set; }
    public decimal BhtnNld { get; set; }
    public decimal TongBaoHiemNld { get; set; }

    public decimal ThueTncnKhauTru { get; set; }
    public decimal TamUng { get; set; }
    public decimal ThucLinh { get; set; }

    public string? SoTaiKhoanNganHang { get; set; }
    public string? TenNganHang { get; set; }
}
