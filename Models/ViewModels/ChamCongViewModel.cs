using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class BangChamCongViewModel
{
    public long Id { get; set; }
    public string KyKeToan { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public int SoNgayCongChuan { get; set; } = 22;
    public TrangThaiChamCong TrangThai { get; set; }
    public List<DongChamCongViewModel> DongChamCongs { get; set; } = new();
}

public class DongChamCongViewModel
{
    public long NhanVienId { get; set; }
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? PhongBan { get; set; }
    public decimal SoNgayDiLam { get; set; }
    public decimal SoNgayNghiPhep { get; set; }
    public decimal SoNgayNghiLe { get; set; }
    public decimal SoNgayNghiKhongLuong { get; set; }
    public decimal SoNgayNghiOmBhxh { get; set; }
    public decimal SoNgayNghiThaiSan { get; set; }
    public decimal GioLamThemNgayThuong { get; set; }
    public decimal GioLamThemNgayNghi { get; set; }
    public decimal GioLamThemNgayLe { get; set; }
    public decimal TongCongTinhLuong { get; set; }
}
