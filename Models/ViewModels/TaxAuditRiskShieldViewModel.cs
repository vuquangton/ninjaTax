using ninjaTax.Models.Entities;

namespace ninjaTax.Models.ViewModels;

public class TaxAuditShieldReportViewModel
{
    public long Id { get; set; }
    public int NamTaiChinh { get; set; }
    public DateTime NgayQuet { get; set; } = DateTime.Now;

    public int TongSoPhatHien { get; set; }
    public int SoCanhBaoDoNghiemTrong { get; set; }
    public int SoCanhBaoVangChuY { get; set; }

    public decimal TongTienChiPhiRuiRo { get; set; }
    public decimal TongTienThueTruyThuUocTinh { get; set; }
    public decimal TongTienPhatChamNopUocTinh { get; set; }

    public List<TaxRiskItemViewModel> PhatHiens { get; set; } = new();
}

public class TaxRiskItemViewModel
{
    public LoaiBayThue LoaiBay { get; set; }
    public MucDoRuiRo MucDo { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string MoTaChiTiet { get; set; } = string.Empty;
    public string? MaChungTuLienQuan { get; set; }
    public DateTime? NgayPhatSinh { get; set; }
    public decimal SoTienViPham { get; set; }
    public decimal SoTienThueRuiRo { get; set; }
    public decimal SoTienPhatDuKien { get; set; }
    public string CanCuPhapLy { get; set; } = string.Empty;
    public string BienPhapKhacPhuc { get; set; } = string.Empty;
}
