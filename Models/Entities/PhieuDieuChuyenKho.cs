namespace ninjaTax.Models.Entities;

public enum TrangThaiDieuChuyenKho
{
    TamTinh = 0,
    DangVanChuyen = 1,
    DaHoanThanh = 2,
    DaHuy = 3
}

/// <summary>
/// Phiếu xuất kho kiêm vận chuyển nội bộ / Phiếu điều chuyển kho (Mẫu 03-VT theo TT 200 & NĐ 123/2020).
/// Hạch toán chuyển đổi địa điểm kho: Nợ 1561/152 (Kho Đến) / Có 1561/152 (Kho Đi).
/// </summary>
public class PhieuDieuChuyenKho
{
    public long Id { get; set; }

    public long ChiNhanhId { get; set; }
    public ChiNhanh? ChiNhanh { get; set; }

    /// <summary>
    /// Số phiếu điều chuyển (VD: DCK-2026-0001)
    /// </summary>
    public string SoPhieu { get; set; } = string.Empty;

    public DateTime NgayDieuChuyen { get; set; }
    public DateTime NgayHachToan { get; set; }

    public long KhoXuatId { get; set; }
    public Kho? KhoXuat { get; set; }

    public long KhoNhapId { get; set; }
    public Kho? KhoNhap { get; set; }

    /// <summary>
    /// Họ tên người vận chuyển
    /// </summary>
    public string? NguoiVanChuyen { get; set; }

    /// <summary>
    /// Biển kiểm soát / Phương tiện vận tải
    /// </summary>
    public string? PhuongTienVanChuyen { get; set; }

    /// <summary>
    /// Số lệnh điều động theo quy định Nghị định 123/2020/NĐ-CP
    /// </summary>
    public string? LenhDieuDongSo { get; set; }

    /// <summary>
    /// Lý do điều chuyển nội bộ
    /// </summary>
    public string? LyDoDieuChuyen { get; set; }

    public decimal TongSoLuong { get; set; }
    public decimal TongGiaTri { get; set; }

    public TrangThaiDieuChuyenKho TrangThai { get; set; } = TrangThaiDieuChuyenKho.TamTinh;

    public long? ButToanId { get; set; }
    public ButToan? ButToan { get; set; }

    public List<ChiTietDieuChuyenKho> ChiTietDieuChuyens { get; set; } = new();
}
