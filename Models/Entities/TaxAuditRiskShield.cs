using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ninjaTax.Models.Entities;

public enum LoaiBayThue
{
    Bay1_HoaDonTren20TrTienMat = 1,
    Bay2_AmQuyTienMatThoiDiem = 2,
    Bay3_TamNopThieu80PhanTramTndn = 3,
    Bay4_NoLuongQua30Thang3 = 4,
    Bay5_KhauHaoVuotKhungHoacCcdcQua36Thang = 5,
    Bay6_ThieuKhauTru10PhanTramThoiVu = 6,
    Bay7_BanHangDuoiGiaVon = 7,
    Bay8_ChiPhiLaiVayChuaGopDuVonDieuLe = 8,
    Bay8_LaiVayVuot30EbitdaLienKet = 8
}

public enum MucDoRuiRo
{
    TrungBinh = 1,
    CanhBaoVang = 1,
    Cao_CanhBaoDo = 2,
    BaoDongDo = 2
}

public class TaxAuditRiskShieldReport
{
    public long Id { get; set; }
    public int NamTaiChinh { get; set; }
    public DateTime NgayQuet { get; set; } = DateTime.UtcNow;

    public int TongSoPhatHien { get; set; }
    public int SoCanhBaoDoNghiemTrong { get; set; }
    public int SoCanhBaoVangChuY { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongTienChiPhiRuiRo { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongTienThueTruyThuUocTinh { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongTienPhatChamNopUocTinh { get; set; }

    public ICollection<TaxRiskFinding> DanhSachPhatHien { get; set; } = new List<TaxRiskFinding>();

    [NotMapped]
    public ICollection<TaxRiskFinding> ChiTietRuiRos { get => DanhSachPhatHien; set => DanhSachPhatHien = value; }
}

public class TaxRiskFinding
{
    public long Id { get; set; }
    public long TaxAuditRiskShieldReportId { get; set; }

    public LoaiBayThue LoaiBay { get; set; }
    public MucDoRuiRo MucDo { get; set; }

    [MaxLength(255)]
    public string TieuDe { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string MoTaChiTiet { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MaChungTuLienQuan { get; set; }

    public DateTime? NgayPhatSinh { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal SoTienViPham { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal SoTienThueRuiRo { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal SoTienPhatDuKien { get; set; }

    [MaxLength(255)]
    public string CanCuPhapLy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BienPhapKhacPhuc { get; set; } = string.Empty;

    public TaxAuditRiskShieldReport? Report { get; set; }
}
