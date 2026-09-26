using System.ComponentModel.DataAnnotations;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class ThuChiFilterViewModel
{
    public LoaiChungTuThuChi? LoaiChungTu { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public string? TuKhoa { get; set; }
    public List<ChungTuThuChiItemViewModel> DanhSach { get; set; } = new();
}

public class ChungTuThuChiItemViewModel
{
    public long Id { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
    public LoaiChungTuThuChi LoaiChungTu { get; set; }
    public DateTime NgayChungTu { get; set; }
    public string? TenDoiTuong { get; set; }
    public string? NguoiGiaoNopNhan { get; set; }
    public string? LyDo { get; set; }
    public decimal TongTien { get; set; }
    public bool ViPhamQuyTac20Tr { get; set; }
    public TrangThaiThuChi TrangThai { get; set; }
    public string? SoTaiKhoanNganHang { get; set; }
    public string? SoButToan { get; set; }
}

public class ThuChiCreateViewModel
{
    public LoaiChungTuThuChi LoaiChungTu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập ngày chứng từ")]
    public DateTime NgayChungTu { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng nhập ngày hạch toán")]
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    public string? SoChungTuGoc { get; set; }

    public long? DoiTuongId { get; set; }

    public string? NguoiGiaoNopNhan { get; set; }

    public string? DiaChi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do / diễn giải chung")]
    [StringLength(500)]
    public string LyDo { get; set; } = string.Empty;

    public long? TaiKhoanNganHangId { get; set; }

    public long? HoaDonBanHangId { get; set; }
    public long? HoaDonMuaHangId { get; set; }

    public decimal TonQuyHienTai { get; set; } = 0;

    public List<ChiTietThuChiItemViewModel> ChiTiets { get; set; } = new();
}

public class ChiTietThuChiItemViewModel
{
    public string DienGiai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Chọn tài khoản Nợ")]
    public long TaiKhoanNoId { get; set; }

    [Required(ErrorMessage = "Chọn tài khoản Có")]
    public long TaiKhoanCoId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
    public decimal SoTien { get; set; }

    public long? DoiTuongId { get; set; }
}

public class SoQuyBaoCaoViewModel
{
    public string LoaiSo { get; set; } = "1111"; // "1111" hoặc "1121"
    public string TenSo { get; set; } = "SỔ QUỸ TIỀN MẶT";
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public long? TaiKhoanNganHangId { get; set; }
    public string? TenNganHang { get; set; }
    public string? SoTaiKhoanNganHang { get; set; }

    public decimal SoDuDauKy { get; set; }
    public decimal TongThuTrongKy { get; set; }
    public decimal TongChiTrongKy { get; set; }
    public decimal SoDuCuoiKy { get; set; }

    public List<DongSoQuyViewModel> DongSoQuys { get; set; } = new();
}

public class DongSoQuyViewModel
{
    public DateTime NgayGhiSo { get; set; }
    public DateTime NgayChungTu { get; set; }
    public string SoChungTuThu { get; set; } = string.Empty;
    public string SoChungTuChi { get; set; } = string.Empty;
    public string DienGiai { get; set; } = string.Empty;
    public string TaiKhoanDoiUng { get; set; } = string.Empty;
    public decimal SoTienThu { get; set; }
    public decimal SoTienChi { get; set; }
    public decimal SoDuLuyKe { get; set; }
}
