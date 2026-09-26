namespace ninjaTax.Models.Entities;

/// <summary>
/// Phân loại đối tượng pháp nhân/thể nhân
/// </summary>
public enum LoaiDoiTuong
{
    KhachHang = 1,
    NhaCungCap = 2,
    NhanVien = 3,
    Khac = 4
}

/// <summary>
/// Danh mục Đối tượng (Khách hàng, Nhà cung cấp, Cán bộ nhân viên, Đối tác khác).
/// Phục vụ theo dõi công nợ chi tiết (TK 131, TK 331, TK 141...).
/// Khóa chính long/bigint.
/// </summary>
public class DoiTuong
{
    /// <summary>
    /// Khóa chính (bigint/long) duy nhất cho đối tượng
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Mã đối tượng (Mã khách hàng, mã NCC, mã NV)
    /// </summary>
    public string MaDoiTuong { get; set; } = string.Empty;

    /// <summary>
    /// Tên đầy đủ của đối tượng hoặc tên công ty/doanh nghiệp
    /// </summary>
    public string TenDoiTuong { get; set; } = string.Empty;

    /// <summary>
    /// Phân loại đối tượng (Khách hàng, Nhà cung cấp, Nhân viên...)
    /// </summary>
    public LoaiDoiTuong Loai { get; set; } = LoaiDoiTuong.KhachHang;

    /// <summary>
    /// Mã số thuế doanh nghiệp hoặc số định danh cá nhân
    /// </summary>
    public string? MaSoThue { get; set; }

    /// <summary>
    /// Địa chỉ trụ sở hoặc nơi cư trú
    /// </summary>
    public string? DiaChi { get; set; }

    /// <summary>
    /// Số điện thoại liên lạc
    /// </summary>
    public string? SoDienThoai { get; set; }

    /// <summary>
    /// Hòm thư điện tử
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Tên người đại diện hoặc người liên hệ chính
    /// </summary>
    public string? NguoiLienHe { get; set; }

    /// <summary>
    /// Số tài khoản ngân hàng giao dịch
    /// </summary>
    public string? SoTaiKhoanNganHang { get; set; }

    /// <summary>
    /// Tên ngân hàng mở tài khoản
    /// </summary>
    public string? TenNganHang { get; set; }

    /// <summary>
    /// Trạng thái hoạt động (true: đang giao dịch; false: ngừng giao dịch)
    /// </summary>
    public bool DangHoatDong { get; set; } = true;
}
