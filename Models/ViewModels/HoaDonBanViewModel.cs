using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class HoaDonBanCreateViewModel
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

    [Required(ErrorMessage = "Vui lòng chọn Khách hàng")]
    [Display(Name = "Khách hàng")]
    public long KhachHangId { get; set; }

    [Display(Name = "Ký hiệu mẫu số")]
    public string KHMauSo { get; set; } = "1C26TBB";

    [Display(Name = "Ký hiệu HĐ")]
    public string KyHieu { get; set; } = "C26T";

    [Display(Name = "Số HĐĐT")]
    public string? SoHoaDon { get; set; }

    [Display(Name = "Ngày hóa đơn")]
    [DataType(DataType.Date)]
    public DateTime NgayHoaDon { get; set; } = DateTime.Today;

    [Display(Name = "Hạn thanh toán")]
    [DataType(DataType.Date)]
    public DateTime HanThanhToan { get; set; } = DateTime.Today.AddDays(15);

    [Required(ErrorMessage = "Vui lòng nhập Diễn giải")]
    [Display(Name = "Diễn giải")]
    public string DienGiai { get; set; } = string.Empty;

    [Display(Name = "Bán hàng kiêm xuất kho")]
    public bool BanHangKiemXuatKho { get; set; } = true;

    public List<ChiTietHoaDonBanItemViewModel> ChiTiets { get; set; } = new()
    {
        new ChiTietHoaDonBanItemViewModel { DongSo = 1 }
    };

    public List<SelectListItem> DanhSachKhachHang { get; set; } = new();
    public List<SelectListItem> DanhSachVatTu { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanNo { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanDoanhThu { get; set; } = new();
}

public class ChiTietHoaDonBanItemViewModel
{
    public int DongSo { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn Mặt hàng")]
    [Display(Name = "Mặt hàng")]
    public long VatTuHangHoaId { get; set; }

    [Required(ErrorMessage = "Nhập số lượng")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Số lượng phải > 0")]
    public decimal SoLuong { get; set; } = 1;

    [Required(ErrorMessage = "Nhập đơn giá bán")]
    [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không hợp lệ")]
    public decimal DonGia { get; set; }

    public decimal TiLeChietKhau { get; set; }
    public decimal ThueSuatVat { get; set; } = 10m;

    [Display(Name = "Đơn giá vốn xuất kho")]
    public decimal DonGiaVon { get; set; }

    public long TaiKhoanNoId { get; set; }
    public long TaiKhoanDoanhThuId { get; set; }
    public long TaiKhoanThueId { get; set; }
    public long? TaiKhoanGiaVonId { get; set; }
    public long? TaiKhoanKhoId { get; set; }
}

public class HoaDonBanIndexViewModel
{
    public List<HoaDonBanHang> DanhSachHoaDon { get; set; } = new();
    public decimal TongDoanhThuBan { get; set; }
    public decimal TongConPhaiThu { get; set; }
}
