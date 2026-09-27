using System.ComponentModel.DataAnnotations;

namespace ninjaTax.Models.Entities;

public enum NhaCungCapHddt
{
    ChuaKetNoi = 0,
    VNPT = 1,
    Viettel = 2,
    MISA = 3,
    Bkav = 4,
    ThaiSon = 5
}

public enum LoaiChungThuSo
{
    UsbToken = 1,
    CloudHsm = 2
}

/// <summary>
/// Cấu hình kết nối Hóa đơn điện tử & Chữ ký số theo Nghị định 123/2020 & Thông tư 78/2021.
/// </summary>
public class CauHinhHoaDonDienTu
{
    public long Id { get; set; }

    public long ChiNhanhId { get; set; }
    public virtual ChiNhanh? ChiNhanh { get; set; }

    public NhaCungCapHddt NhaCungCap { get; set; } = NhaCungCapHddt.ChuaKetNoi;

    [MaxLength(255)]
    public string? DuongDanApi { get; set; }

    [MaxLength(100)]
    public string? TaiKhoanApi { get; set; }

    [MaxLength(255)]
    public string? MatKhauApi { get; set; }

    [MaxLength(20)]
    public string MauSoHoaDon { get; set; } = "1";

    /// <summary>
    /// Ký hiệu hóa đơn chuẩn NĐ 123 (Ví dụ: C26TAA)
    /// </summary>
    [MaxLength(20)]
    public string KyHieuHoaDon { get; set; } = "C26TAA";

    public LoaiChungThuSo LoaiChungThuSo { get; set; } = LoaiChungThuSo.UsbToken;

    [MaxLength(100)]
    public string? SeriChungThuSo { get; set; }

    public bool TuDongPhatHanh { get; set; } = false;

    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;
}

