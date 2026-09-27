using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class InventoryIndexViewModel
{
    public long? SelectedBranchId { get; set; }
    public long? SelectedKhoId { get; set; }
    public int? SelectedLoaiPhieu { get; set; } // 1: Nhập, 2: Xuất, null: Tất cả
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }

    public List<SelectListItem> BranchList { get; set; } = new();
    public List<SelectListItem> WarehouseList { get; set; } = new();

    public List<InwardVoucherListItemViewModel> InwardVouchers { get; set; } = new();
    public List<OutwardVoucherListItemViewModel> OutwardVouchers { get; set; } = new();

    public decimal TongTienNhap => InwardVouchers.Sum(v => v.TongTienHang);
    public decimal TongTienXuatGiaVon => OutwardVouchers.Sum(v => v.TongTienGiaVon);
}

public class InwardVoucherListItemViewModel
{
    public long Id { get; set; }
    public string SoPhieu { get; set; } = string.Empty;
    public DateTime NgayNhap { get; set; }
    public DateTime NgayHachToan { get; set; }
    public string TenKho { get; set; } = string.Empty;
    public LoaiNhapKho LoaiNhapKho { get; set; }
    public string? TenNhaCungCap { get; set; }
    public string DienGiai { get; set; } = string.Empty;
    public decimal TongSoLuong { get; set; }
    public decimal TongTienHang { get; set; }
    public TrangThaiPhieuKho TrangThai { get; set; }
    public long? ButToanId { get; set; }
}

public class OutwardVoucherListItemViewModel
{
    public long Id { get; set; }
    public string SoPhieu { get; set; } = string.Empty;
    public DateTime NgayXuat { get; set; }
    public DateTime NgayHachToan { get; set; }
    public string TenKho { get; set; } = string.Empty;
    public LoaiXuatKho LoaiXuatKho { get; set; }
    public string? TenKhachHang { get; set; }
    public string? TenPhongBan { get; set; }
    public string DienGiai { get; set; } = string.Empty;
    public decimal TongSoLuong { get; set; }
    public decimal TongTienGiaVon { get; set; }
    public TrangThaiPhieuKho TrangThai { get; set; }
    public long? ButToanId { get; set; }
}

public class PhieuNhapKhoEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Chi nhánh là bắt buộc")]
    [Display(Name = "Chi nhánh")]
    public long ChiNhanhId { get; set; }

    [Required(ErrorMessage = "Kho nhập là bắt buộc")]
    [Display(Name = "Kho nhập hàng")]
    public long KhoId { get; set; }

    [Required(ErrorMessage = "Số phiếu là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Số phiếu nhập")]
    public string SoPhieu { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày nhập kho")]
    public DateTime NgayNhap { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày hạch toán")]
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    [Display(Name = "Loại nhập kho")]
    public LoaiNhapKho LoaiNhapKho { get; set; } = LoaiNhapKho.MuaNgoai;

    [Display(Name = "Nhà cung cấp")]
    public long? NhaCungCapId { get; set; }

    [StringLength(500)]
    [Display(Name = "Diễn giải")]
    public string? DienGiai { get; set; }

    [Display(Name = "Ghi sổ ngay")]
    public bool GhiSoNgay { get; set; } = true;

    public List<ChiTietNhapKhoEditViewModel> ChiTiets { get; set; } = new();

    public List<SelectListItem> BranchList { get; set; } = new();
    public List<SelectListItem> WarehouseList { get; set; } = new();
    public List<SelectListItem> SupplierList { get; set; } = new();
    public List<SelectListItem> ItemList { get; set; } = new();
}

public class ChiTietNhapKhoEditViewModel
{
    public long VatTuHangHoaId { get; set; }
    public string TenVatTu { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
    public long TaiKhoanNoId { get; set; }
    public long TaiKhoanCoId { get; set; }
    public string? SoLo { get; set; }
    public DateTime? HanSuDung { get; set; }
}

public class PhieuXuatKhoEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Chi nhánh là bắt buộc")]
    [Display(Name = "Chi nhánh")]
    public long ChiNhanhId { get; set; }

    [Required(ErrorMessage = "Kho xuất là bắt buộc")]
    [Display(Name = "Kho xuất hàng")]
    public long KhoId { get; set; }

    [Required(ErrorMessage = "Số phiếu là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Số phiếu xuất")]
    public string SoPhieu { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày xuất kho")]
    public DateTime NgayXuat { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày hạch toán")]
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    [Display(Name = "Loại xuất kho")]
    public LoaiXuatKho LoaiXuatKho { get; set; } = LoaiXuatKho.BanHang;

    [Display(Name = "Khách hàng")]
    public long? KhachHangId { get; set; }

    [Display(Name = "Bộ phận / Phòng ban (Chi phí)")]
    public long? PhongBanId { get; set; }

    [StringLength(500)]
    [Display(Name = "Diễn giải")]
    public string? DienGiai { get; set; }

    [Display(Name = "Ghi sổ ngay")]
    public bool GhiSoNgay { get; set; } = true;

    public List<ChiTietXuatKhoEditViewModel> ChiTiets { get; set; } = new();

    public List<SelectListItem> BranchList { get; set; } = new();
    public List<SelectListItem> WarehouseList { get; set; } = new();
    public List<SelectListItem> CustomerList { get; set; } = new();
    public List<SelectListItem> DepartmentList { get; set; } = new();
    public List<SelectListItem> ItemList { get; set; } = new();
}

public class ChiTietXuatKhoEditViewModel
{
    public long VatTuHangHoaId { get; set; }
    public string TenVatTu { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public decimal TonKhoKhaDung { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGiaVon { get; set; }
    public decimal TienGiaVon => SoLuong * DonGiaVon;
    public long TaiKhoanNoId { get; set; }
    public long TaiKhoanCoId { get; set; }
}

public class BaoCaoNhapXuatTonViewModel
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public long? SelectedKhoId { get; set; }
    public long? SelectedBranchId { get; set; }
    public string TenKho { get; set; } = "Tất cả kho";
    public string TenChiNhanh { get; set; } = "Toàn công ty";
    public DateTime NgayLap { get; set; } = DateTime.Today;

    public List<SelectListItem> BranchList { get; set; } = new();
    public List<SelectListItem> WarehouseList { get; set; } = new();

    public List<DongNhapXuatTonViewModel> Items { get; set; } = new();

    public decimal TongTonDauSoLuong => Items.Sum(i => i.TonDauSoLuong);
    public decimal TongTonDauThanhTien => Items.Sum(i => i.TonDauThanhTien);

    public decimal TongNhapSoLuong => Items.Sum(i => i.NhapSoLuong);
    public decimal TongNhapThanhTien => Items.Sum(i => i.NhapThanhTien);

    public decimal TongXuatSoLuong => Items.Sum(i => i.XuatSoLuong);
    public decimal TongXuatThanhTien => Items.Sum(i => i.XuatThanhTien);

    public decimal TongTonCuoiSoLuong => Items.Sum(i => i.TonCuoiSoLuong);
    public decimal TongTonCuoiThanhTien => Items.Sum(i => i.TonCuoiThanhTien);

    /// <summary>
    /// Số dư Nợ Sổ Cái các TK Kho (TK 152, 1561) tại ngày cuối kỳ để đối soát
    /// </summary>
    public decimal SoDuSoCaiTkKho { get; set; }

    /// <summary>
    /// Kiểm tra khớp 100% giữa Báo cáo NXT và Sổ Cái kế toán
    /// </summary>
    public bool IsKhopSoCai => Math.Abs(TongTonCuoiThanhTien - SoDuSoCaiTkKho) < 1.0m;
}

public class DongNhapXuatTonViewModel
{
    public long VatTuHangHoaId { get; set; }
    public string MaVatTu { get; set; } = string.Empty;
    public string TenVatTu { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public LoaiVatTuHangHoa LoaiVatTu { get; set; }

    public decimal TonDauSoLuong { get; set; }
    public decimal TonDauThanhTien { get; set; }

    public decimal NhapSoLuong { get; set; }
    public decimal NhapThanhTien { get; set; }

    public decimal XuatSoLuong { get; set; }
    public decimal XuatThanhTien { get; set; }

    public decimal TonCuoiSoLuong { get; set; }
    public decimal TonCuoiThanhTien { get; set; }

    public decimal DonGiaBinhQuan => TonCuoiSoLuong > 0 ? Math.Round(TonCuoiThanhTien / TonCuoiSoLuong, 4) : 0m;
}
