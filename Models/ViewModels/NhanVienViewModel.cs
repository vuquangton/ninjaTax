using System.ComponentModel.DataAnnotations;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class NhanVienListViewModel
{
    public string? TuKhoa { get; set; }
    public string? PhongBan { get; set; }
    public LoaiHopDongLaoDong? LoaiHopDong { get; set; }
    public bool? DangLamViec { get; set; }
    public List<NhanVienItemViewModel> DanhSach { get; set; } = new();
}

public class NhanVienIndexViewModel
{
    public List<NhanVienItemViewModel> DanhSachNhanVien { get; set; } = new();
    public List<string> PhongBans { get; set; } = new();
    public string? PhongBan { get; set; }
    public LoaiHopDongLaoDong? LoaiHopDong { get; set; }
    public string? TuKhoa { get; set; }
}

public class NhanVienItemViewModel
{
    public long Id { get; set; }
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? SoCccd { get; set; }
    public string? MaSoThue { get; set; }
    public string? PhongBan { get; set; }
    public string? ChucVu { get; set; }
    public LoaiHopDongLaoDong LoaiHopDong { get; set; }
    public decimal LuongCoBan { get; set; }
    public decimal LuongDongBaoHiem { get; set; }
    public int SoNguoiPhuThuoc { get; set; }
    public bool CoCamKet08 { get; set; }
    public bool DongBaoHiem { get; set; }
    public bool DangLamViec { get; set; }
}

public class NhanVienCreateEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Mã nhân viên bắt buộc nhập")]
    [StringLength(50)]
    public string MaNhanVien { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên nhân viên bắt buộc nhập")]
    [StringLength(255)]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(20)]
    public string? SoCccd { get; set; }

    [StringLength(20)]
    public string? MaSoThue { get; set; }

    [StringLength(20)]
    public string? SoSoBhxh { get; set; }

    [StringLength(100)]
    public string? PhongBan { get; set; }

    [StringLength(100)]
    public string? ChucVu { get; set; }

    public LoaiHopDongLaoDong LoaiHopDong { get; set; } = LoaiHopDongLaoDong.HopDongDaiHan;

    [Required]
    public DateTime NgayVaoLam { get; set; } = DateTime.Today;

    public DateTime? NgayKetThucHd { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản không hợp lệ")]
    public decimal LuongCoBan { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Lương đóng bảo hiểm không hợp lệ")]
    public decimal LuongDongBaoHiem { get; set; }

    public decimal PhuCapAnTrua { get; set; } = 730000m; // Mặc định mức tối đa miễn thuế
    public decimal PhuCapTrachNhiem { get; set; }
    public decimal PhuCapDienThoai { get; set; }
    public decimal PhuCapTrangPhuc { get; set; }

    [Range(0, 20, ErrorMessage = "Số người phụ thuộc từ 0 đến 20")]
    public int SoNguoiPhuThuoc { get; set; } = 0;

    public bool CoCamKet08 { get; set; } = false;
    public bool DongBaoHiem { get; set; } = true;
    public bool LaDoanVienCongDoan { get; set; } = false;

    [StringLength(50)]
    public string? SoTaiKhoanNganHang { get; set; }

    [StringLength(100)]
    public string? TenNganHang { get; set; }

    public bool DangLamViec { get; set; } = true;
}
