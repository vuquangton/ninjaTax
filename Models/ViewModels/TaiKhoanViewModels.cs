using System.ComponentModel.DataAnnotations;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

/// <summary>
/// ViewModel hiển thị một dòng tài khoản trong danh mục cây (Tree-table)
/// </summary>
public class TaiKhoanViewModel
{
    public long Id { get; set; }

    [Display(Name = "Số hiệu TK")]
    public string MaTaiKhoan { get; set; } = string.Empty;

    [Display(Name = "Tên tài khoản")]
    public string TenTaiKhoan { get; set; } = string.Empty;

    [Display(Name = "Bậc")]
    public int BacTaiKhoan { get; set; } = 1;

    public long? TaiKhoanMeId { get; set; }

    [Display(Name = "TK Mẹ")]
    public string? MaTaiKhoanMe { get; set; }

    [Display(Name = "Tên TK Mẹ")]
    public string? TenTaiKhoanMe { get; set; }

    [Display(Name = "Loại tài khoản")]
    public LoaiTaiKhoan LoaiTaiKhoan { get; set; }

    [Display(Name = "Tính chất")]
    public TinhChatTaiKhoan TinhChat { get; set; }

    [Display(Name = "Là TK Sổ Cái (Tổng hợp)")]
    public bool LaTaiKhoanSoCai { get; set; }

    [Display(Name = "Đang sử dụng")]
    public bool DangHoatDong { get; set; } = true;

    public int SoLuongCon { get; set; }

    public bool DaPhatSinhGiaoDich { get; set; }

    public string TenHienThi => $"{MaTaiKhoan} - {TenTaiKhoan}";

    public string LoaiTaiKhoanTen => LoaiTaiKhoan switch
    {
        LoaiTaiKhoan.TaiSan => "Tài sản",
        LoaiTaiKhoan.NoPhaiTra => "Nợ phải trả",
        LoaiTaiKhoan.VonChuSoHuu => "Vốn chủ sở hữu",
        LoaiTaiKhoan.DoanhThu => "Doanh thu",
        LoaiTaiKhoan.ChiPhi => "Chi phí",
        LoaiTaiKhoan.ThuNhapKhac => "Thu nhập khác",
        LoaiTaiKhoan.ChiPhiKhac => "Chi phí khác",
        LoaiTaiKhoan.XacDinhKetQuaKinhDoanh => "Xác định KQKD (TK 911)",
        _ => "Khác"
    };

    public string TinhChatTen => TinhChat switch
    {
        TinhChatTaiKhoan.DuNo => "Dư Nợ",
        TinhChatTaiKhoan.DuCo => "Dư Có",
        TinhChatTaiKhoan.LuongTinh => "Lưỡng tính (2 chiều)",
        TinhChatTaiKhoan.KhongCoSoDu => "Không có số dư",
        _ => "Không xác định"
    };
}

/// <summary>
/// ViewModel phục vụ Thêm mới và Chỉnh sửa tài khoản kế toán
/// </summary>
public class TaiKhoanCreateEditViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số hiệu tài khoản")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Số hiệu tài khoản từ 3 đến 20 ký tự")]
    [Display(Name = "Số hiệu tài khoản")]
    public string MaTaiKhoan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
    [StringLength(255, ErrorMessage = "Tên tài khoản không quá 255 ký tự")]
    [Display(Name = "Tên tài khoản")]
    public string TenTaiKhoan { get; set; } = string.Empty;

    [Display(Name = "Tài khoản mẹ")]
    public long? TaiKhoanMeId { get; set; }

    [Display(Name = "Bậc tài khoản")]
    [Range(1, 10, ErrorMessage = "Bậc tài khoản từ 1 đến 10")]
    public int BacTaiKhoan { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn loại tài khoản")]
    [Display(Name = "Loại tài khoản")]
    public LoaiTaiKhoan LoaiTaiKhoan { get; set; } = LoaiTaiKhoan.TaiSan;

    [Required(ErrorMessage = "Vui lòng chọn tính chất tài khoản")]
    [Display(Name = "Tính chất số dư")]
    public TinhChatTaiKhoan TinhChat { get; set; } = TinhChatTaiKhoan.DuNo;

    [Display(Name = "Tài khoản tổng hợp (chỉ cộng dồn, không hạch toán trực tiếp)")]
    public bool LaTaiKhoanSoCai { get; set; }

    [Display(Name = "Đang sử dụng")]
    public bool DangHoatDong { get; set; } = true;

    public bool DaPhatSinhGiaoDich { get; set; }
}
