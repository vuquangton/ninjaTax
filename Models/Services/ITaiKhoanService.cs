using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Hợp đồng dịch vụ quản trị Hệ thống Tài khoản kế toán (COA Service Contract)
/// </summary>
public interface ITaiKhoanService
{
    /// <summary>
    /// Lấy danh sách tài khoản kế toán dạng danh mục phẳng hoặc hỗ trợ cây phân cấp
    /// </summary>
    Task<List<TaiKhoanViewModel>> LayDanhSachAsync(string? timKiem = null, LoaiTaiKhoan? loaiTaiKhoan = null);

    /// <summary>
    /// Lấy thông tin tài khoản theo Id
    /// </summary>
    Task<TaiKhoan?> LayTheoIdAsync(long id);

    /// <summary>
    /// Lấy thông tin tài khoản theo Mã
    /// </summary>
    Task<TaiKhoan?> LayTheoMaAsync(string maTaiKhoan);

    /// <summary>
    /// Thêm mới tài khoản kế toán (kiểm tra tiền tố mẹ, tính duy nhất của mã, cập nhật cờ LaTaiKhoanSoCai của mẹ)
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao, long? TaiKhoanId)> TaoMoiAsync(TaiKhoanCreateEditViewModel model);

    /// <summary>
    /// Chỉnh sửa tài khoản kế toán (chống đổi mã nếu đã có bút toán, kiểm soát trạng thái hoạt động)
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, TaiKhoanCreateEditViewModel model);

    /// <summary>
    /// Xóa tài khoản kế toán (tuyệt đối cấm nếu đã phát sinh bút toán hoặc đang có tài khoản con)
    /// </summary>
    Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id);

    /// <summary>
    /// Lấy danh sách các tài khoản có thể làm tài khoản mẹ
    /// </summary>
    Task<List<TaiKhoan>> LayDanhSachTaiKhoanMeAsync();
}
