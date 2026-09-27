using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class DepartmentIndexViewModel
{
    public long? SelectedBranchId { get; set; }
    public List<SelectListItem> BranchList { get; set; } = new();
    public List<DepartmentListItemViewModel> Departments { get; set; } = new();
    public int TotalDepartments => Departments.Count;
    public int TotalEmployees => Departments.Sum(d => d.SoNhanVien);
}

public class DepartmentListItemViewModel
{
    public long Id { get; set; }
    public long ChiNhanhId { get; set; }
    public string TenChiNhanh { get; set; } = string.Empty;
    public long? PhongBanChaId { get; set; }
    public string? TenPhongBanCha { get; set; }
    public string MaPhongBan { get; set; } = string.Empty;
    public string TenPhongBan { get; set; } = string.Empty;
    public string? TenTiengAnh { get; set; }
    public LoaiPhongBan LoaiPhongBan { get; set; }
    public string TenLoaiPhongBan => LoaiPhongBan switch
    {
        LoaiPhongBan.QuanLy => "Quản lý (6422)",
        LoaiPhongBan.BanHang => "Bán hàng (6421)",
        LoaiPhongBan.SanXuat => "Sản xuất (154)",
        LoaiPhongBan.KhoaChuyenMon => "Khoa chuyên môn (154)",
        _ => "Khác"
    };
    public string? MaTaiKhoanChiPhi { get; set; }
    public string? TenTruongPhong { get; set; }
    public bool LaTrungTamLoiNhuan { get; set; }
    public bool DangHoatDong { get; set; }
    public int SoNhanVien { get; set; }
    public int Level { get; set; } = 0;
}

public class DepartmentEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Chi nhánh là bắt buộc")]
    [Display(Name = "Chi nhánh trực thuộc")]
    public long ChiNhanhId { get; set; }

    [Display(Name = "Phòng ban cha")]
    public long? PhongBanChaId { get; set; }

    [Required(ErrorMessage = "Mã phòng ban là bắt buộc")]
    [StringLength(50, ErrorMessage = "Mã phòng ban không quá 50 ký tự")]
    [Display(Name = "Mã phòng ban")]
    public string MaPhongBan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
    [StringLength(255, ErrorMessage = "Tên phòng ban không quá 255 ký tự")]
    [Display(Name = "Tên phòng ban / Bộ phận")]
    public string TenPhongBan { get; set; } = string.Empty;

    [StringLength(255)]
    [Display(Name = "Tên tiếng Anh (nếu có)")]
    public string? TenTiengAnh { get; set; }

    [Required]
    [Display(Name = "Loại phòng ban")]
    public LoaiPhongBan LoaiPhongBan { get; set; } = LoaiPhongBan.QuanLy;

    [StringLength(20)]
    [Display(Name = "Mã tài khoản chi phí mặc định (vd: 6422, 6421, 154)")]
    public string? MaTaiKhoanChiPhi { get; set; }

    [Display(Name = "Trưởng phòng / Phụ trách")]
    public long? TruongPhongId { get; set; }

    [Display(Name = "Trung tâm lợi nhuận (Profit Center)")]
    public bool LaTrungTamLoiNhuan { get; set; } = false;

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public List<SelectListItem> BranchList { get; set; } = new();
    public List<SelectListItem> ParentDepartmentList { get; set; } = new();
    public List<SelectListItem> EmployeeList { get; set; } = new();
}
