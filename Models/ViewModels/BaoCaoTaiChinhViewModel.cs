using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class BctcDashboardViewModel
{
    public int NamTaiChinh { get; set; }
    public TrangThaiBaoCaoTaiChinh TrangThai { get; set; } = TrangThaiBaoCaoTaiChinh.DangLap;

    // B01-DN
    public decimal TongTaiSan { get; set; }
    public decimal TongNguonVon { get; set; }
    public bool B01CanDoi => TongTaiSan == TongNguonVon;

    // B02-DN
    public decimal DoanhThuThuan { get; set; }
    public decimal LoiNhuanGop { get; set; }
    public decimal LoiNhuanTruocThue { get; set; }
    public decimal ThueTndnHienHanh { get; set; }
    public decimal LoiNhuanSauThue { get; set; }

    // B03-DN
    public decimal LuuChuyenHdkd { get; set; }
    public decimal LuuChuyenHddt { get; set; }
    public decimal LuuChuyenHdtc { get; set; }
    public decimal TienDauKy { get; set; }
    public decimal TienCuoiKy { get; set; }

    // Quyết toán thuế tóm tắt
    public decimal ThueTndnPhaiNop { get; set; }
    public decimal ThueTndnTamNop4Quy { get; set; }
    public decimal TyLeTamNopTndn { get; set; }
    public bool ViPham80PhanTramTndn { get; set; }

    public int TongNhanVienQttTncn { get; set; }
    public decimal TongThueTncnQuyetToan { get; set; }

    // Risk Shield tóm tắt
    public int SoCanhBaoDo { get; set; }
    public int SoCanhBaoVang { get; set; }
    public decimal TongTienRuiRo { get; set; }
}

public class DongChiTieuB01ViewModel
{
    public string MaSo { get; set; } = string.Empty;
    public string ChiTieu { get; set; } = string.Empty;
    public string? ThuyetMinh { get; set; }
    public decimal SoDauNam { get; set; }
    public decimal SoCuoiNam { get; set; }
    public bool InDam { get; set; }
    public bool InNghieng { get; set; }
    public int CapDo { get; set; } = 1;
}

public class BaoCaoTinhHinhTaiChinhViewModel
{
    public int NamTaiChinh { get; set; }
    public DateTime NgayLap { get; set; } = DateTime.Today;
    public string SoChungTu { get; set; } = string.Empty;
    public TrangThaiBaoCaoTaiChinh TrangThai { get; set; }
    public decimal TongTaiSanDauNam { get; set; }
    public decimal TongTaiSanCuoiNam { get; set; }
    public decimal TongNguonVonDauNam { get; set; }
    public decimal TongNguonVonCuoiNam { get; set; }
    public bool IsCanDoi => TongTaiSanCuoiNam == TongNguonVonCuoiNam;
    public List<DongChiTieuB01ViewModel> ChiTiets { get; set; } = new();
}

public class DongChiTieuB02ViewModel
{
    public string MaSo { get; set; } = string.Empty;
    public string ChiTieu { get; set; } = string.Empty;
    public string? ThuyetMinh { get; set; }
    public decimal NamTruoc { get; set; }
    public decimal NamNay { get; set; }
    public bool InDam { get; set; }
    public bool InNghieng { get; set; }
}

public class BaoCaoKetQuaKinhDoanhViewModel
{
    public int NamTaiChinh { get; set; }
    public DateTime NgayLap { get; set; } = DateTime.Today;
    public string SoChungTu { get; set; } = string.Empty;
    public decimal DoanhThuThuan { get; set; }
    public decimal LoiNhuanGop { get; set; }
    public decimal LoiNhuanTruocThue { get; set; }
    public decimal ThueTndnHienHanh { get; set; }
    public decimal LoiNhuanSauThue { get; set; }
    public decimal ChenhLechTk4212 { get; set; }
    public bool IsKhopSoSổCai => Math.Abs(ChenhLechTk4212) < 1;
    public List<DongChiTieuB02ViewModel> ChiTiets { get; set; } = new();
}

public class DongChiTieuB03ViewModel
{
    public string MaSo { get; set; } = string.Empty;
    public string ChiTieu { get; set; } = string.Empty;
    public string? ThuyetMinh { get; set; }
    public decimal NamTruoc { get; set; }
    public decimal NamNay { get; set; }
    public bool InDam { get; set; }
    public bool InNghieng { get; set; }
}

public class BaoCaoLuuChuyenTienTeViewModel
{
    public int NamTaiChinh { get; set; }
    public DateTime NgayLap { get; set; } = DateTime.Today;
    public string SoChungTu { get; set; } = string.Empty;
    public decimal LuuChuyenHdkd { get; set; }
    public decimal LuuChuyenHddt { get; set; }
    public decimal LuuChuyenHdtc { get; set; }
    public decimal LuuChuyenThuanTrongKy { get; set; }
    public decimal TienDauKy { get; set; }
    public decimal TienCuoiKy { get; set; }
    public decimal DuNoTk111Va112 { get; set; }
    public bool IsKhopSoCai => TienCuoiKy == DuNoTk111Va112;
    public bool IsKhopTienMat => IsKhopSoCai;
    public List<DongChiTieuB03ViewModel> ChiTiets { get; set; } = new();
}

public class ThuyetMinhBctcViewModel
{
    public int NamTaiChinh { get; set; }
    public string TenDoanhNghiep { get; set; } = "CÔNG TY TNHH NINJATAX VIỆT NAM";
    public string MaSoThue { get; set; } = "0109998888";
    public string DiaChi { get; set; } = "Hà Nội, Việt Nam";
    public string HinhThucSo { get; set; } = "Nhật ký chung";
    public string CheDoKeToan { get; set; } = "Thông tư 99/2025/TT-BTC";
    public string DonViTienTe { get; set; } = "VND";
    public decimal NguyenGiaTscd { get; set; }
    public decimal HaoMonLuyKeTscd { get; set; }
    public decimal GiaTriConLaiTscd { get; set; }
    public decimal TongChiPhiCcDcPhanBo { get; set; }
    public decimal TongPhaiThuKhachHang { get; set; }
    public decimal TongPhaiTraNguoiBan { get; set; }
    public decimal TongQuyLuongTrongNam { get; set; }
    public decimal TongBaoHiemDaTrichNop { get; set; }
    public decimal TongThueDaNopTrongNam { get; set; }
}
