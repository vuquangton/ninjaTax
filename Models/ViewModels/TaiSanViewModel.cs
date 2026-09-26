using System.ComponentModel.DataAnnotations;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class TaiSanListViewModel
{
    public LoaiTaiSan? LoaiTaiSan { get; set; }
    public TrangThaiTaiSan? TrangThai { get; set; }
    public string? TuKhoa { get; set; }
    public List<TaiSanItemViewModel> DanhSach { get; set; } = new();
}

public class TaiSanItemViewModel
{
    public long Id { get; set; }
    public string MaTaiSan { get; set; } = string.Empty;
    public string TenTaiSan { get; set; } = string.Empty;
    public LoaiTaiSan LoaiTaiSan { get; set; }
    public DateTime NgayGhiTang { get; set; }
    public DateTime NgayBatDauKhauHao { get; set; }
    public decimal NguyenGia { get; set; }
    public int ThoiGianSuDungThang { get; set; }
    public decimal GiaTriDaKhauHao { get; set; }
    public decimal GiaTriConLai { get; set; }
    public decimal MucKhauHaoThang { get; set; }
    public string? MaTaiKhoanNguyenGia { get; set; }
    public string? MaTaiKhoanKhauHao { get; set; }
    public string? MaTaiKhoanChiPhi { get; set; }
    public string? BoPhanSuDung { get; set; }
    public TrangThaiTaiSan TrangThai { get; set; }
}

public class TaiSanCreateViewModel
{
    [Required(ErrorMessage = "Mã tài sản không được để trống")]
    [StringLength(50)]
    public string MaTaiSan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên tài sản / CCDC không được để trống")]
    [StringLength(255)]
    public string TenTaiSan { get; set; } = string.Empty;

    public LoaiTaiSan LoaiTaiSan { get; set; } = LoaiTaiSan.TaiSanCoDinh;

    [Required]
    public DateTime NgayGhiTang { get; set; } = DateTime.Today;

    [Required]
    public DateTime NgayBatDauKhauHao { get; set; } = DateTime.Today;

    [Range(1, double.MaxValue, ErrorMessage = "Nguyên giá phải lớn hơn 0")]
    public decimal NguyenGia { get; set; }

    [Range(1, 600, ErrorMessage = "Thời gian sử dụng phải từ 1 đến 600 tháng")]
    public int ThoiGianSuDungThang { get; set; } = 36;

    [Required(ErrorMessage = "Chọn tài khoản nguyên giá (211 hoặc 242)")]
    public long TaiKhoanNguyenGiaId { get; set; }

    public long? TaiKhoanKhauHaoId { get; set; }

    [Required(ErrorMessage = "Chọn tài khoản chi phí (642)")]
    public long TaiKhoanChiPhiId { get; set; }

    public string? BoPhanSuDung { get; set; }
    public string? GhiChu { get; set; }
}

public class BangKhauHaoKyViewModel
{
    public string KyKeToan { get; set; } = DateTime.Today.ToString("yyyy-MM");
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public bool DaChayKhauHao { get; set; }
    public decimal TongKhauHaoKy { get; set; }
    public List<DongKhauHaoViewModel> ChiTiets { get; set; } = new();
}

public class DongKhauHaoViewModel
{
    public long TaiSanCoDinhId { get; set; }
    public string MaTaiSan { get; set; } = string.Empty;
    public string TenTaiSan { get; set; } = string.Empty;
    public LoaiTaiSan LoaiTaiSan { get; set; }
    public decimal NguyenGia { get; set; }
    public decimal KhauHaoKyNay { get; set; }
    public decimal LuyKeKhauHao { get; set; }
    public decimal GiaTriConLai { get; set; }
    public string MaTaiKhoanChiPhi { get; set; } = string.Empty;
    public string MaTaiKhoanKhauHao { get; set; } = string.Empty;
    public string? SoButToan { get; set; }
}
