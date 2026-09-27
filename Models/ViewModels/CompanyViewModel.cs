using System.ComponentModel.DataAnnotations;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class CompanyDashboardViewModel
{
    public ThongTinDoanhNghiep DoanhNghiep { get; set; } = new();
    public CauHinhKeToan CauHinhKeToan { get; set; } = new();
    public List<ChiNhanh> ChiNhanhs { get; set; } = new();
}

public class CompanyEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Mã doanh nghiệp là bắt buộc")]
    [MaxLength(50)]
    [Display(Name = "Mã doanh nghiệp")]
    public string MaDoanhNghiep { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên doanh nghiệp là bắt buộc")]
    [MaxLength(255)]
    [Display(Name = "Tên doanh nghiệp")]
    public string TenDoanhNghiep { get; set; } = string.Empty;

    [MaxLength(255)]
    [Display(Name = "Tên giao dịch")]
    public string? TenGiaoDich { get; set; }

    [MaxLength(255)]
    [Display(Name = "Tên tiếng Anh")]
    public string? TenTiengAnh { get; set; }

    [Required(ErrorMessage = "Mã số thuế là bắt buộc")]
    [MaxLength(20)]
    [Display(Name = "Mã số thuế")]
    public string MaSoThue { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ trụ sở là bắt buộc")]
    [MaxLength(500)]
    [Display(Name = "Địa chỉ trụ sở")]
    public string DiaChiTruSo { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Tỉnh/Thành phố")]
    public string? TinhThanhPho { get; set; }

    [MaxLength(100)]
    [Display(Name = "Quận/Huyện")]
    public string? QuanHuyen { get; set; }

    [MaxLength(20)]
    [Display(Name = "Mã cơ quan thuế")]
    public string? MaCoQuanThueQuanLy { get; set; }

    [MaxLength(255)]
    [Display(Name = "Cơ quan thuế quản lý")]
    public string? TenCoQuanThueQuanLy { get; set; }

    [MaxLength(255)]
    [Display(Name = "Người đại diện pháp luật")]
    public string? NguoiDaiDienPhapLuat { get; set; }

    [MaxLength(100)]
    [Display(Name = "Chức danh")]
    public string? ChucDanhNguoiDaiDien { get; set; }

    [MaxLength(255)]
    [Display(Name = "Giám đốc")]
    public string? GiamDoc { get; set; }

    [MaxLength(255)]
    [Display(Name = "Kế toán trưởng")]
    public string? KeToanTruong { get; set; }

    [MaxLength(255)]
    [Display(Name = "Người lập biểu")]
    public string? NguoiLapBieu { get; set; }

    [MaxLength(255)]
    [Display(Name = "Thủ quỹ")]
    public string? ThuQuy { get; set; }

    [MaxLength(50)]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(100)]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [Display(Name = "Vốn điều lệ (VND)")]
    public decimal VonDieuLe { get; set; }

    [Display(Name = "Ngày thành lập")]
    [DataType(DataType.Date)]
    public DateTime NgayThanhLap { get; set; } = DateTime.Today;
}

public class AccountingConfigEditViewModel
{
    public long DoanhNghiepId { get; set; }

    [Display(Name = "Chế độ kế toán")]
    public CheDoKeToanDoanhNghiep CheDoKeToan { get; set; } = CheDoKeToanDoanhNghiep.TT99_2025;

    [Required]
    [MaxLength(10)]
    [Display(Name = "Đơn vị tiền tệ")]
    public string DonViTienTe { get; set; } = "VND";

    [Range(1, 31)]
    [Display(Name = "Ngày bắt đầu niên độ")]
    public int NgayBatDauNienDo { get; set; } = 1;

    [Range(1, 12)]
    [Display(Name = "Tháng bắt đầu niên độ")]
    public int ThangBatDauNienDo { get; set; } = 1;

    [Display(Name = "Phương pháp thuế GTGT")]
    public PhuongPhapTinhThueGtgt PhuongPhapThueGtgt { get; set; } = PhuongPhapTinhThueGtgt.KhauTru;

    [Display(Name = "Phương pháp xuất kho")]
    public PhuongPhapGiaXuatKho PhuongPhapXuatKho { get; set; } = PhuongPhapGiaXuatKho.BinhQuanCuoiKy;

    [Display(Name = "Phương pháp khấu hao TSCĐ")]
    public PhuongPhapKhauHao PhuongPhapKhauHaoTscd { get; set; } = PhuongPhapKhauHao.DuongThang;

    [Display(Name = "Ngày khóa sổ hiện tại")]
    [DataType(DataType.Date)]
    public DateTime? NgayKhoaSo { get; set; }

    [Display(Name = "Cảnh báo khi chi vượt số dư quỹ")]
    public bool CanhBaoChiVuotQuy { get; set; } = true;

    [Display(Name = "Cảnh báo khi xuất âm kho")]
    public bool CanhBaoXuatAmKho { get; set; } = true;

    [Display(Name = "Cảnh báo hóa đơn từ 20 triệu thanh toán tiền mặt")]
    public bool CanhBaoHoaDonTren20TrTienMat { get; set; } = true;
}

public class BranchEditViewModel
{
    public long Id { get; set; }
    public long DoanhNghiepId { get; set; }

    [Required(ErrorMessage = "Mã chi nhánh là bắt buộc")]
    [MaxLength(50)]
    [Display(Name = "Mã chi nhánh")]
    public string MaChiNhanh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên chi nhánh là bắt buộc")]
    [MaxLength(255)]
    [Display(Name = "Tên chi nhánh")]
    public string TenChiNhanh { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Mã số thuế chi nhánh (13 số)")]
    public string? MaSoThueChiNhanh { get; set; }

    [Display(Name = "Loai chi nhanh")]
    public LoaiChiNhanh LoaiChiNhanh { get; set; } = LoaiChiNhanh.PhuThuocCungTinh;

    [Display(Name = "Kê khai thuế GTGT riêng")]
    public bool KeKhaiThueGtgtRieng { get; set; }

    [Display(Name = "Kê khai thuế TNCN riêng")]
    public bool KeKhaiThueTncnRieng { get; set; }

    [MaxLength(500)]
    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [MaxLength(100)]
    [Display(Name = "Tỉnh/Thành phố")]
    public string? TinhThanhPho { get; set; }

    [MaxLength(20)]
    [Display(Name = "Mã cơ quan thuế quản lý riêng")]
    public string? MaCoQuanThueQuanLyRieng { get; set; }

    [MaxLength(255)]
    [Display(Name = "Tên cơ quan thuế quản lý riêng")]
    public string? TenCoQuanThueQuanLyRieng { get; set; }

    [MaxLength(255)]
    [Display(Name = "Người đứng đầu")]
    public string? NguoiDungDau { get; set; }

    [MaxLength(50)]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;
}

public class LockBookViewModel
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày khóa sổ kế toán")]
    public DateTime NgayKhoaSo { get; set; } = DateTime.Today;
}
