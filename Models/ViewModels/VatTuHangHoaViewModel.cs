using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class VatTuHangHoaCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập Mã vật tư")]
    [Display(Name = "Mã vật tư")]
    [StringLength(50)]
    public string MaVatTu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Tên vật tư hàng hóa")]
    [Display(Name = "Tên vật tư hàng hóa")]
    [StringLength(255)]
    public string TenVatTu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Đơn vị tính")]
    [Display(Name = "Đơn vị tính")]
    [StringLength(50)]
    public string DonViTinh { get; set; } = "Cái";

    [Display(Name = "Tính chất")]
    public LoaiVatTuHangHoa LoaiVatTu { get; set; } = LoaiVatTuHangHoa.HangHoa;

    [Display(Name = "TK Kho ngầm định")]
    public long? TaiKhoanKhoId { get; set; }

    [Display(Name = "TK Doanh thu ngầm định")]
    public long? TaiKhoanDoanhThuId { get; set; }

    [Display(Name = "TK Giá vốn ngầm định")]
    public long? TaiKhoanGiaVonId { get; set; }

    [Display(Name = "Thuế suất VAT (%)")]
    public decimal ThueSuatVatMacDinh { get; set; } = 10m;

    [Display(Name = "Đơn giá mua gần nhất")]
    public decimal DonGiaMuaGanNhat { get; set; }

    [Display(Name = "Đơn giá bán tiêu chuẩn")]
    public decimal DonGiaBanTieuChuan { get; set; }

    [Display(Name = "Theo dõi tồn kho")]
    public bool DangTheoDoiTonKho { get; set; } = true;

    public List<SelectListItem> DanhSachTaiKhoanKho { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanDoanhThu { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanGiaVon { get; set; } = new();
}

public class VatTuHangHoaEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập Mã vật tư")]
    [Display(Name = "Mã vật tư")]
    [StringLength(50)]
    public string MaVatTu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Tên vật tư hàng hóa")]
    [Display(Name = "Tên vật tư hàng hóa")]
    [StringLength(255)]
    public string TenVatTu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Đơn vị tính")]
    [Display(Name = "Đơn vị tính")]
    [StringLength(50)]
    public string DonViTinh { get; set; } = "Cái";

    [Display(Name = "Tính chất")]
    public LoaiVatTuHangHoa LoaiVatTu { get; set; } = LoaiVatTuHangHoa.HangHoa;

    [Display(Name = "TK Kho ngầm định")]
    public long? TaiKhoanKhoId { get; set; }

    [Display(Name = "TK Doanh thu ngầm định")]
    public long? TaiKhoanDoanhThuId { get; set; }

    [Display(Name = "TK Giá vốn ngầm định")]
    public long? TaiKhoanGiaVonId { get; set; }

    [Display(Name = "Thuế suất VAT (%)")]
    public decimal ThueSuatVatMacDinh { get; set; } = 10m;

    [Display(Name = "Đơn giá mua gần nhất")]
    public decimal DonGiaMuaGanNhat { get; set; }

    [Display(Name = "Đơn giá bán tiêu chuẩn")]
    public decimal DonGiaBanTieuChuan { get; set; }

    [Display(Name = "Theo dõi tồn kho")]
    public bool DangTheoDoiTonKho { get; set; } = true;

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;

    public bool DaPhatSinhGiaoDich { get; set; }

    public List<SelectListItem> DanhSachTaiKhoanKho { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanDoanhThu { get; set; } = new();
    public List<SelectListItem> DanhSachTaiKhoanGiaVon { get; set; } = new();
}

public class VatTuHangHoaIndexViewModel
{
    public string? TimKiem { get; set; }
    public LoaiVatTuHangHoa? LoaiFilter { get; set; }
    public List<VatTuHangHoaItemViewModel> DanhSachVatTu { get; set; } = new();
}

public class VatTuHangHoaItemViewModel
{
    public long Id { get; set; }
    public string MaVatTu { get; set; } = string.Empty;
    public string TenVatTu { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public LoaiVatTuHangHoa LoaiVatTu { get; set; }
    public decimal ThueSuatVatMacDinh { get; set; }
    public decimal DonGiaMuaGanNhat { get; set; }
    public decimal DonGiaBanTieuChuan { get; set; }
    public bool DangHoatDong { get; set; }
    public bool DangTheoDoiTonKho { get; set; }
    public bool DaPhatSinhGiaoDich { get; set; }
    public string? TaiKhoanKhoMa { get; set; }
    public string? TaiKhoanDoanhThuMa { get; set; }
    public string? TaiKhoanGiaVonMa { get; set; }
}
