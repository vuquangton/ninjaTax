namespace ninjaTax.Models.Entities;

public enum PhepTinhQuyDoi
{
    Nhan = 1,
    Chia = 2
}

/// <summary>
/// Đơn vị tính quy đổi đa cấp (Multi-UoM) cho vật tư hàng hóa.
/// Cho phép giao dịch bằng ĐVT phụ (Thùng, Két, Lốc...) và tự động quy đổi về ĐVT cơ bản.
/// </summary>
public class DonViTinhQuyDoi
{
    public long Id { get; set; }

    public long VatTuHangHoaId { get; set; }
    public VatTuHangHoa? VatTuHangHoa { get; set; }

    /// <summary>
    /// Tên đơn vị tính quy đổi (VD: Thùng, Két, Lốc, Hộp, Vỉ)
    /// </summary>
    public string TenDonViTinh { get; set; } = string.Empty;

    /// <summary>
    /// Hệ số quy đổi so với đơn vị tính cơ bản (VD: 1 Thùng = 24 Lon -> TyLeQuyDoi = 24)
    /// </summary>
    public decimal TyLeQuyDoi { get; set; } = 1m;

    /// <summary>
    /// Phép tính quy đổi: Nhân (Base = Sub * Rate) hoặc Chia (Base = Sub / Rate)
    /// </summary>
    public PhepTinhQuyDoi PhepTinh { get; set; } = PhepTinhQuyDoi.Nhan;

    /// <summary>
    /// Đơn giá bán niêm yết theo đơn vị tính quy đổi
    /// </summary>
    public decimal DonGiaBanQuyDoi { get; set; }

    /// <summary>
    /// Đơn vị tính mặc định khi bán hàng
    /// </summary>
    public bool LaDonViBanMacDinh { get; set; }

    /// <summary>
    /// Đơn vị tính mặc định khi mua hàng
    /// </summary>
    public bool LaDonViMuaMacDinh { get; set; }

    public bool DangHoatDong { get; set; } = true;
}
