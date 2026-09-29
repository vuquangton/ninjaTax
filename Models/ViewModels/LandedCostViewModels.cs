using System.ComponentModel.DataAnnotations;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class LandedCostIndexViewModel
{
    public List<ChungTuChiPhiMuaHang> DanhSachChiPhi { get; set; } = new();
    public decimal TongChiPhiChuaPhanBo { get; set; }
    public decimal TongChiPhiDaPhanBo { get; set; }
}

public class LandedCostCreateViewModel
{
    [Required(ErrorMessage = "Số chứng từ chi phí là bắt buộc")]
    [Display(Name = "Số chứng từ")]
    public string SoChungTu { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Ngày chứng từ")]
    public DateTime NgayChungTu { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Ngày hạch toán")]
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    [Display(Name = "Nhà cung cấp dịch vụ")]
    public long? NhaCungCapDichVuId { get; set; }

    [Required(ErrorMessage = "Diễn giải chi phí là bắt buộc")]
    [Display(Name = "Diễn giải")]
    public string DienGiai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số tiền chi phí là bắt buộc")]
    [Range(1, double.MaxValue, ErrorMessage = "Chi phí phải lớn hơn 0")]
    [Display(Name = "Số tiền chi phí")]
    public decimal TongChiPhi { get; set; }

    [Display(Name = "Thuế suất VAT (%)")]
    public decimal ThueSuatVat { get; set; } = 10m;

    [Display(Name = "Phương thức phân bổ ngầm định")]
    public PhuongThucPhanBoChiPhi PhuongThucPhanBo { get; set; } = PhuongThucPhanBoChiPhi.TheoGiaTri;
}

public class LandedCostAllocateViewModel
{
    public long ChungTuChiPhiId { get; set; }
    public ChungTuChiPhiMuaHang? ChungTuChiPhi { get; set; }

    [Display(Name = "Phương thức phân bổ")]
    public PhuongThucPhanBoChiPhi PhuongThucPhanBo { get; set; } = PhuongThucPhanBoChiPhi.TheoGiaTri;

    [Display(Name = "Tài khoản chi phí (TK Nợ)")]
    public long TaiKhoanChiPhiId { get; set; }

    [Display(Name = "Tài khoản đối ứng (TK Có)")]
    public long TaiKhoanDoiUngId { get; set; }

    public List<long> SelectedChiTietNhapKhoIds { get; set; } = new();

    public List<ChiTietNhapKho> AvailableChiTietNhapKhos { get; set; } = new();
}
