namespace ninjaTax.Models.Entities;

/// <summary>
/// Bảng giá bán (price list) cho các mặt hàng / dịch vụ.
/// Hỗ trợ nhiều bảng giá đồng thời, với thời gian hiệu lực.
/// </summary>
public class BangGiaBan
{
    public long Id { get; set; }
    public string MaBangGia { get; set; } = string.Empty; // max 20
    public string TenBangGia { get; set; } = string.Empty; // max 255
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ChiTietBangGia> ChiTietBangGias { get; set; } = new List<ChiTietBangGia>();
}

/// <summary>
/// Chi tiết giá cho một mặt hàng trong một bảng giá.
/// </summary>
public class ChiTietBangGia
{
    public long Id { get; set; }
    public long BangGiaBanId { get; set; }
    public BangGiaBan? BangGiaBan { get; set; }
    public long VatTuHangHoaId { get; set; }
    public VatTuHangHoa? VatTuHangHoa { get; set; }
    public decimal DonGia { get; set; } // price per unit, precision 19,4
    public decimal GiaBan => DonGia; // alias for clarity
}
