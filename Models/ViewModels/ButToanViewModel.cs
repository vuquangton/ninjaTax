using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

/// <summary>
/// ViewModel phục vụ tạo mới Bút toán Nhật ký chung
/// </summary>
public class ButToanCreateViewModel
{
    [Display(Name = "Số chứng từ")]
    public string? SoChungTu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập Ngày hạch toán")]
    [Display(Name = "Ngày hạch toán")]
    [DataType(DataType.Date)]
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng nhập Ngày chứng từ")]
    [Display(Name = "Ngày chứng từ")]
    [DataType(DataType.Date)]
    public DateTime NgayChungTu { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Quy định TT99: Bắt buộc nhập Số chứng từ gốc")]
    [Display(Name = "Số chứng từ gốc")]
    [StringLength(100)]
    public string SoChungTuGoc { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quy định TT99: Bắt buộc nhập Ngày chứng từ gốc")]
    [Display(Name = "Ngày chứng từ gốc")]
    [DataType(DataType.Date)]
    public DateTime NgayChungTuGoc { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng nhập Diễn giải")]
    [Display(Name = "Diễn giải")]
    [StringLength(500)]
    public string DienGiai { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách dòng định khoản chi tiết
    /// </summary>
    public List<ChiTietButToanItemViewModel> ChiTiets { get; set; } = new()
    {
        new ChiTietButToanItemViewModel { DongSo = 1 }
    };

    // Danh sách chọn cho giao diện
    public List<SelectListItem> DanhSachTaiKhoan { get; set; } = new();
    public List<SelectListItem> DanhSachDoiTuong { get; set; } = new();
}

/// <summary>
/// ViewModel dòng định khoản chi tiết
/// </summary>
public class ChiTietButToanItemViewModel
{
    public int DongSo { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn Tài khoản Nợ")]
    [Display(Name = "TK Nợ")]
    public long TaiKhoanNoId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn Tài khoản Có")]
    [Display(Name = "TK Có")]
    public long TaiKhoanCoId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập Số tiền")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
    [Display(Name = "Số tiền")]
    public decimal SoTien { get; set; }

    [Display(Name = "Diễn giải")]
    public string? DienGiai { get; set; }

    [Display(Name = "Đối tượng")]
    public long? DoiTuongId { get; set; }
}

/// <summary>
/// ViewModel hiển thị danh sách sổ Nhật ký chung
/// </summary>
public class ButToanIndexViewModel
{
    public List<ButToan> DanhSachButToan { get; set; } = new();
    public decimal TongPhatSinh { get; set; }
    public int TongSoChungTu { get; set; }
}
