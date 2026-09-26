using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class HoaDonMuaCreateViewModel
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

    [Required(ErrorMessage = "Vui lòng chọn Nhà cung cấp")]
    [Display(Name = "Nhà cung cấp")]
    public long NhaCungCapId { get; set; }

    [Display(Name = "Ký hiệu mẫu số HĐ")]
    public string KHMauSoHoaDon { get; set; } = "1C26TAA";

    [Display(Name = "Ký hiệu HĐ")]
    public string KyHieuHoaDon { get; set; } = "C26T";

    [Required(ErrorMessage = "Bắt buộc nhập Số HĐĐT đầu vào")]
    [Display(Name = "Số HĐĐT")]
    [StringLength(50)]
    public string SoHoaDon { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Ngày hóa đơn")]
    [Display(Name = "Ngày hóa đơn")]
    [DataType(DataType.Date)]
    public DateTime NgayHoaDon { get; set; } = DateTime.Today;

    [Display(Name = "Hạn thanh toán")]
    [DataType(DataType.Date)]
    public DateTime HanThanhToan { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Vui lòng nhập Diễn giải")]
    [Display(Name = "Diễn giải")]
    public string DienGiai { get; set; } = string.Empty;

    [Display(Name = "Mua hàng kiêm nhập kho")]
    public bool MuaHangKiemKho { get; set; } = true;

    public List<ChiTietHoaDonMuaItemViewModel> ChiTiets { get; set; } = new()
    {
        new ChiTietHoaDonMuaItemViewModel { DongSo = 1 }
    };

    public List<SelectListItem> DanhSachNhaCungCap { get; set; } = new();
    public List<SelectListItem> DanhSachVatTu { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanNo { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanCo { get; set; } = new();
}

public class ChiTietHoaDonMuaItemViewModel
{
    public int DongSo { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn Hàng hóa/Vật tư")]
    [Display(Name = "Mặt hàng")]
    public long VatTuHangHoaId { get; set; }

    [Required(ErrorMessage = "Nhập số lượng")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Số lượng phải > 0")]
    public decimal SoLuong { get; set; } = 1;

    [Required(ErrorMessage = "Nhập đơn giá")]
    [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không hợp lệ")]
    public decimal DonGia { get; set; }

    public decimal TiLeChietKhau { get; set; }
    public decimal ThueSuatVat { get; set; } = 10m;

    public long TaiKhoanNoId { get; set; }
    public long TaiKhoanThueId { get; set; }
    public long TaiKhoanCoId { get; set; }
}

public class HoaDonMuaIndexViewModel
{
    public List<HoaDonMuaHang> DanhSachHoaDon { get; set; } = new();
    public decimal TongGiaTriMua { get; set; }
    public decimal TongConPhaiTra { get; set; }
}
