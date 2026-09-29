using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ninjaTax.Models.ViewModels;

public class TaiKhoanNganHangCreateEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Số tài khoản ngân hàng là bắt buộc")]
    [StringLength(50, ErrorMessage = "Số tài khoản không vượt quá 50 ký tự")]
    [Display(Name = "Số tài khoản ngân hàng")]
    public string SoTaiKhoan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên ngân hàng là bắt buộc")]
    [StringLength(255, ErrorMessage = "Tên ngân hàng không vượt quá 255 ký tự")]
    [Display(Name = "Ngân hàng")]
    public string TenNganHang { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Chi nhánh không vượt quá 255 ký tự")]
    [Display(Name = "Chi nhánh ngân hàng")]
    public string? ChiNhanh { get; set; }

    [StringLength(255, ErrorMessage = "Chủ tài khoản không vượt quá 255 ký tự")]
    [Display(Name = "Chủ tài khoản")]
    public string? ChuTaiKhoan { get; set; }

    [Display(Name = "Số dư ban đầu")]
    public decimal SoDuBanDau { get; set; }

    [Display(Name = "Tiểu khoản Sổ Cái (TK 1121)")]
    public long? TaiKhoanKeToanId { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;

    [StringLength(500, ErrorMessage = "Ghi chú không vượt quá 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public bool DaPhatSinhGiaoDich { get; set; }

    public List<SelectListItem> AccountList { get; set; } = new();
}

public class TaiKhoanNganHangIndexViewModel
{
    public string? TimKiem { get; set; }
    public List<TaiKhoanNganHangItemViewModel> DanhSach { get; set; } = new();
}

public class TaiKhoanNganHangItemViewModel
{
    public long Id { get; set; }
    public string SoTaiKhoan { get; set; } = string.Empty;
    public string TenNganHang { get; set; } = string.Empty;
    public string? ChiNhanh { get; set; }
    public string? ChuTaiKhoan { get; set; }
    public decimal SoDuBanDau { get; set; }
    public string? TaiKhoanKeToanMa { get; set; }
    public bool DangHoatDong { get; set; }
    public bool DaPhatSinhGiaoDich { get; set; }
}
