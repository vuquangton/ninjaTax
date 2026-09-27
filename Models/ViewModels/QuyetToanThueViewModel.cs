using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class QuyetToanTndnViewModel
{
    public long Id { get; set; }
    public int NamQuyetToan { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; } = DateTime.Today;
    public TrangThaiQuyetToan TrangThai { get; set; }

    public decimal ChiTieuA1_LoiNhuanKeToan { get; set; }
    public decimal ChiTieuB4_ChiPhiKhongDuocTru { get; set; }
    public decimal ChiTieuB7_ThuNhapMienThue { get; set; }
    public decimal ChiTieuB14_ThuNhapChiuThue { get; set; }
    public decimal ChiTieuC1_ThuNhapTinhThue { get; set; }
    public decimal ChiTieuC4_LoKetChuyen { get; set; }
    public decimal ThueSuat { get; set; } = 20.0m;
    public decimal ChiTieuC7_ThueTndnPhaiNop { get; set; }

    public decimal TamNopQ1 { get; set; }
    public decimal TamNopQ2 { get; set; }
    public decimal TamNopQ3 { get; set; }
    public decimal TamNopQ4 { get; set; }
    public decimal TongTamNop4Quy { get; set; }
    public decimal Nguong80PhanTram { get; set; }
    public decimal TyLeTamNop { get; set; }
    public bool ViPham80PhanTram { get; set; }
    public decimal SoTienNopThieu { get; set; }
    public int SoNgayChamNop { get; set; }
    public decimal TienPhatChamNopDuKien { get; set; }

    public List<DongChiPhiB4ViewModel> DanhSachChiPhiB4 { get; set; } = new();
}

public class DongChiPhiB4ViewModel
{
    public LoaiViPhamB4 LoaiViPham { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string? SoChungTuLienQuan { get; set; }
    public DateTime? NgayChungTu { get; set; }
    public string CanCuPhapLy { get; set; } = string.Empty;
}

public class QuyetToanTncnViewModel
{
    public long Id { get; set; }
    public int NamQuyetToan { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; } = DateTime.Today;
    public TrangThaiQuyetToan TrangThai { get; set; }

    public int TongSoNhanVienQuyetToan { get; set; }
    public int SoNhanVienUyQuyen { get; set; }

    public decimal TongThuNhapChiuThue { get; set; }
    public decimal TongThuNhapMienThue { get; set; }
    public decimal TongGiamTruGiaCanh { get; set; }
    public decimal TongBaoHiemBatBuoc { get; set; }
    public decimal TongThuNhapTinhThue { get; set; }
    public decimal TongThueDaKhauTru { get; set; }
    public decimal TongThuePhaiNopSauQtt { get; set; }
    public decimal TongThueNopThua { get; set; }
    public decimal TongThueConPhaiNopThem { get; set; }

    public List<Dong051ViewModel> BangKe051 { get; set; } = new();
    public List<Dong052ViewModel> BangKe052 { get; set; } = new();
}

public class Dong051ViewModel
{
    public long NhanVienId { get; set; }
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? MaSoThue { get; set; }
    public string? SoCccd { get; set; }
    public bool CaNhanUyQuyenQuyetToan { get; set; }

    public decimal TongThuNhapChiuThue { get; set; }
    public decimal ThuNhapMienThue { get; set; }
    public decimal GiamTruBanThan { get; set; }
    public int SoNguoiPhuThuoc { get; set; }
    public decimal GiamTruNguoiPhuThuoc { get; set; }
    public decimal BaoHiemBatBuoc { get; set; }

    public decimal ThuNhapTinhThue { get; set; }
    public decimal ThueDaKhauTru { get; set; }
    public decimal ThueDaKhauTruTrongNam => ThueDaKhauTru;
    public decimal ThuePhaiNopSauQuyetToan { get; set; }
    public decimal ThueNopThua { get; set; }
    public decimal ThueConPhaiNop { get; set; }
}

public class Dong052ViewModel
{
    public long NhanVienId { get; set; }
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? MaSoThue { get; set; }
    public string? SoCccd { get; set; }
    public bool CoCamKet08 { get; set; }

    public decimal TongThuNhapChiuThue { get; set; }
    public decimal ThueTncnDaKhauTru10 { get; set; }
}
