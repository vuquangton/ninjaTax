using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ninjaTax.Models.ViewModels;

public class KhoCreateEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Chi nhánh trực thuộc là bắt buộc")]
    [Display(Name = "Chi nhánh")]
    public long ChiNhanhId { get; set; }

    [Required(ErrorMessage = "Mã kho là bắt buộc")]
    [StringLength(50, ErrorMessage = "Mã kho không vượt quá 50 ký tự")]
    [Display(Name = "Mã kho hàng")]
    public string MaKho { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên kho là bắt buộc")]
    [StringLength(255, ErrorMessage = "Tên kho không vượt quá 255 ký tự")]
    [Display(Name = "Tên kho hàng")]
    public string TenKho { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Địa chỉ không vượt quá 500 ký tự")]
    [Display(Name = "Địa điểm kho")]
    public string? DiaChi { get; set; }

    [Display(Name = "Thủ kho phụ trách")]
    public long? ThuKhoId { get; set; }

    [Display(Name = "Tài khoản kho ngầm định")]
    public long? TaiKhoanKhoMacDinhId { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;

    [StringLength(500, ErrorMessage = "Ghi chú không vượt quá 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public bool DaPhatSinhPhieuKho { get; set; }

    public List<SelectListItem> BranchList { get; set; } = new();
    public List<SelectListItem> EmployeeList { get; set; } = new();
    public List<SelectListItem> AccountList { get; set; } = new();
}

public class KhoIndexViewModel
{
    public long? BranchId { get; set; }
    public List<KhoItemViewModel> Warehouses { get; set; } = new();
    public List<SelectListItem> BranchList { get; set; } = new();
}

public class KhoItemViewModel
{
    public long Id { get; set; }
    public string MaKho { get; set; } = string.Empty;
    public string TenKho { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
    public string TenChiNhanh { get; set; } = string.Empty;
    public string? TenThuKho { get; set; }
    public string? TaiKhoanKhoMa { get; set; }
    public bool DangHoatDong { get; set; }
    public bool DaPhatSinhPhieuKho { get; set; }
}
