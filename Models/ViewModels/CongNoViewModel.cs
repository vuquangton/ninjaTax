using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;

namespace ninjaTax.Models.ViewModels;

public class CongNoIndexViewModel
{
    public DateTime MocThoiGian { get; set; } = DateTime.Today;
    public List<AgingReportItem> BaoCaoTuoiNoPhaiThu { get; set; } = new();
    public List<AgingReportItem> BaoCaoTuoiNoPhaiTra { get; set; } = new();
    public decimal TongPhaiThu => BaoCaoTuoiNoPhaiThu.Sum(x => x.TongNo);
    public decimal TongPhaiTra => BaoCaoTuoiNoPhaiTra.Sum(x => x.TongNo);
}

public class DoiTruCongNoCreateViewModel
{
    [Required]
    public LoaiCongNo Loai { get; set; } = LoaiCongNo.PhaiThuKhachHang;

    [Required(ErrorMessage = "Vui lòng chọn Đối tượng")]
    [Display(Name = "Đối tượng")]
    public long DoiTuongId { get; set; }

    [Display(Name = "Hóa đơn cụ thể (nếu không chọn sẽ đối trừ FIFO)")]
    public long? HoaDonId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập Số tiền thanh toán")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
    [Display(Name = "Số tiền thanh toán (VNĐ)")]
    public decimal SoTien { get; set; }

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public List<SelectListItem> DanhSachDoiTuong { get; set; } = new();
    public List<SelectListItem> DanhSachHoaDon { get; set; } = new();
}
