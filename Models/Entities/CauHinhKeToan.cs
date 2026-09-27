using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum CheDoKeToanDoanhNghiep
{
    TT99_2025 = 1 // Thông tư 99/2025/TT-BTC
}

public enum PhuongPhapTinhThueGtgt
{
    KhauTru = 1, // Mẫu 01/GTGT (TK 133, TK 3331)
    TrucTiep = 2  // Mẫu 04/GTGT
}

public enum PhuongPhapGiaXuatKho
{
    BinhQuanCuoiKy = 1,
    BinhQuanTucThoi = 2,
    FIFO = 3
}

public enum PhuongPhapKhauHao
{
    DuongThang = 1,
    SoDuGiamDanCoDieuChinh = 2
}

/// <summary>
/// Thực thể Cấu hình Kế toán & Khóa sổ (Accounting System Configuration & Lock Date).
/// </summary>
public class CauHinhKeToan
{
    public long Id { get; set; }

    public long DoanhNghiepId { get; set; }
    public virtual ThongTinDoanhNghiep? DoanhNghiep { get; set; }

    public CheDoKeToanDoanhNghiep CheDoKeToan { get; set; } = CheDoKeToanDoanhNghiep.TT99_2025;

    [MaxLength(10)]
    public string DonViTienTe { get; set; } = "VND";

    /// <summary>
    /// Ngày bắt đầu năm tài chính (Mặc định 1)
    /// </summary>
    public int NgayBatDauNienDo { get; set; } = 1;

    /// <summary>
    /// Tháng bắt đầu năm tài chính (Mặc định 1 = 01/01 đến 31/12; có thể là 4, 7, 10 đối với FDI)
    /// </summary>
    public int ThangBatDauNienDo { get; set; } = 1;

    public PhuongPhapTinhThueGtgt PhuongPhapThueGtgt { get; set; } = PhuongPhapTinhThueGtgt.KhauTru;
    public PhuongPhapGiaXuatKho PhuongPhapXuatKho { get; set; } = PhuongPhapGiaXuatKho.BinhQuanCuoiKy;
    public PhuongPhapKhauHao PhuongPhapKhauHaoTscd { get; set; } = PhuongPhapKhauHao.DuongThang;

    /// <summary>
    /// Ngày khóa sổ kế toán.
    /// Bất biến: Chứng từ có NgayHachToan <= NgayKhoaSo tuyệt đối không được thêm/sửa/xóa.
    /// </summary>
    public DateTime? NgayKhoaSo { get; set; }

    public bool CanhBaoChiVuotQuy { get; set; } = true;
    public bool CanhBaoXuatAmKho { get; set; } = true;
    public bool CanhBaoHoaDonTren20TrTienMat { get; set; } = true;

    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kiểm tra xem một ngày hạch toán có được phép ghi sổ không dựa trên ngày khóa sổ.
    /// </summary>
    public bool ChoPhepGhiSo(DateTime ngayHachToan)
    {
        if (!NgayKhoaSo.HasValue) return true;
        return ngayHachToan.Date > NgayKhoaSo.Value.Date;
    }
}

