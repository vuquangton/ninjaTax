using System.ComponentModel.DataAnnotations;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class DoiTuongItemViewModel
{
    public long Id { get; set; }
    public string MaDoiTuong { get; set; } = string.Empty;
    public string TenDoiTuong { get; set; } = string.Empty;
    public LoaiDoiTuong Loai { get; set; }
    public string? MaSoThue { get; set; }
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? NguoiLienHe { get; set; }
    public bool DangHoatDong { get; set; }
    public bool DaPhatSinhGiaoDich { get; set; }
    public decimal DuNoHienTai { get; set; }
    public decimal DuCoHienTai { get; set; }
}

public class DoiTuongCreateEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Mã đối tượng là bắt buộc")]
    [StringLength(50, ErrorMessage = "Mã đối tượng không vượt quá 50 ký tự")]
    [Display(Name = "Mã đối tượng")]
    public string MaDoiTuong { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên đối tượng là bắt buộc")]
    [StringLength(255, ErrorMessage = "Tên đối tượng không vượt quá 255 ký tự")]
    [Display(Name = "Tên đối tượng / Tên công ty")]
    public string TenDoiTuong { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại đối tượng là bắt buộc")]
    [Display(Name = "Loại đối tượng")]
    public LoaiDoiTuong Loai { get; set; } = LoaiDoiTuong.KhachHang;

    [StringLength(20, ErrorMessage = "Mã số thuế không vượt quá 20 ký tự")]
    [Display(Name = "Mã số thuế / CCCD")]
    public string? MaSoThue { get; set; }

    [StringLength(500, ErrorMessage = "Địa chỉ không vượt quá 500 ký tự")]
    [Display(Name = "Địa chỉ trụ sở / Nơi cư trú")]
    public string? DiaChi { get; set; }

    [StringLength(50, ErrorMessage = "Số điện thoại không vượt quá 50 ký tự")]
    [Display(Name = "Số điện thoại liên lạc")]
    public string? SoDienThoai { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100, ErrorMessage = "Email không vượt quá 100 ký tự")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(255, ErrorMessage = "Tên người liên hệ không vượt quá 255 ký tự")]
    [Display(Name = "Người liên hệ")]
    public string? NguoiLienHe { get; set; }

    [StringLength(50, ErrorMessage = "Số tài khoản ngân hàng không vượt quá 50 ký tự")]
    [Display(Name = "Số tài khoản ngân hàng")]
    public string? SoTaiKhoanNganHang { get; set; }

    [StringLength(255, ErrorMessage = "Tên ngân hàng không vượt quá 255 ký tự")]
    [Display(Name = "Ngân hàng thụ hưởng")]
    public string? TenNganHang { get; set; }

    [Display(Name = "Đang giao dịch")]
    public bool DangHoatDong { get; set; } = true;
}

public class DoiTuongIndexViewModel
{
    public string? TimKiem { get; set; }
    public LoaiDoiTuong? LoaiFilter { get; set; }
    public List<DoiTuongItemViewModel> DanhSach { get; set; } = new();
}
