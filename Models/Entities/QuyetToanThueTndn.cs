using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ninjaTax.Models.Entities;

public enum TrangThaiQuyetToan
{
    DangLap = 0,
    DaDuyet = 1,
    DaKhoaSo = 2
}

public enum LoaiViPhamB4
{
    HoaDonTren20TrTienMat = 1,
    KhauHaoVuotKhung = 2,
    KhongCoHoaDonHopPhap = 3,
    TienPhatViPhamHanhChinh = 4,
    ChiPhiKhongLienQuanSxkd = 5
}

public class QuyetToanThueTndn
{
    public long Id { get; set; }
    public int NamQuyetToan { get; set; }

    [MaxLength(50)]
    public string SoChungTu { get; set; } = string.Empty;

    public DateTime NgayLap { get; set; } = DateTime.Today;

    [Column(TypeName = "TEXT")]
    public decimal ChiTieuA1_LoiNhuanKeToan { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ChiTieuB4_ChiPhiKhongDuocTru { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ChiTieuB7_ThuNhapMienThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ChiTieuB14_ThuNhapChiuThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ChiTieuC1_ThuNhapTinhThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ChiTieuC4_LoKetChuyen { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueSuatPhanTram { get; set; } = 20.0m;

    [Column(TypeName = "TEXT")]
    public decimal ChiTieuC7_ThueTndnPhaiNop { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueTndnTamNopQ1 { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueTndnTamNopQ2 { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueTndnTamNopQ3 { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueTndnTamNopQ4 { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongTamNop4Quy { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TyLeTamNopPhanTram { get; set; }

    public bool ViPhamQuyTac80PhanTram { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal SoTienNopThieu80 { get; set; }

    public int SoNgayChamNop { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TienPhatChamNopDuKien { get; set; }

    public TrangThaiQuyetToan TrangThai { get; set; } = TrangThaiQuyetToan.DangLap;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public ICollection<ChiPhiKhongHopLyB4> DanhSachChiPhiB4 { get; set; } = new List<ChiPhiKhongHopLyB4>();

    [NotMapped]
    public ICollection<ChiPhiKhongHopLyB4> ChiPhiKhongHopLies { get => DanhSachChiPhiB4; set => DanhSachChiPhiB4 = value; }
    [NotMapped]
    public decimal ThueSuat { get => ThueSuatPhanTram; set => ThueSuatPhanTram = value; }
    [NotMapped]
    public decimal TamNopQ1 { get => ThueTndnTamNopQ1; set => ThueTndnTamNopQ1 = value; }
    [NotMapped]
    public decimal TamNopQ2 { get => ThueTndnTamNopQ2; set => ThueTndnTamNopQ2 = value; }
    [NotMapped]
    public decimal TamNopQ3 { get => ThueTndnTamNopQ3; set => ThueTndnTamNopQ3 = value; }
    [NotMapped]
    public decimal TamNopQ4 { get => ThueTndnTamNopQ4; set => ThueTndnTamNopQ4 = value; }

    // Aliases to match service code
    [NotMapped]
    public decimal Nguong80PhanTram { get => Math.Round(ChiTieuC7_ThueTndnPhaiNop * 0.80m, 0); set { } }

    [NotMapped]
    public decimal TyLeTamNop { get => TyLeTamNopPhanTram; set => TyLeTamNopPhanTram = value; }

    [NotMapped]
    public bool ViPham80PhanTram { get => ViPhamQuyTac80PhanTram; set => ViPhamQuyTac80PhanTram = value; }

    [NotMapped]
    public decimal SoTienNopThieu { get => SoTienNopThieu80; set => SoTienNopThieu80 = value; }
}

public class ChiPhiKhongHopLyB4
{
    public long Id { get; set; }
    public long QuyetToanThueTndnId { get; set; }

    public LoaiViPhamB4 LoaiViPham { get; set; }

    [MaxLength(255)]
    public string MoTa { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public decimal SoTien { get; set; }

    [MaxLength(50)]
    public string? SoChungTuLienQuan { get; set; }

    public DateTime? NgayChungTu { get; set; }

    [MaxLength(255)]
    public string CanCuPhapLy { get; set; } = string.Empty;

    public QuyetToanThueTndn? QuyetToanThueTndn { get; set; }
}
