using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ninjaTax.Models.Entities;

public enum TrangThaiBaoCaoTaiChinh
{
    DangLap = 0,
    DaKhoaSo = 1
}

public enum LoaiBaoCaoTaiChinh
{
    B01_TinhHinhTaiChinh = 1,
    B02_KetQuaKinhDoanh = 2,
    B03_LuuChuyenTienTe = 3,
    B09_ThuyetMinh = 4
}

public class BaoCaoTaiChinhNam
{
    public long Id { get; set; }
    public int NamTaiChinh { get; set; }

    [MaxLength(50)]
    public string SoChungTu { get; set; } = string.Empty;

    public DateTime NgayLap { get; set; } = DateTime.Today;
    public DateTime? NgayKhoaSo { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongTaiSan { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongNguonVon { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal DoanhThuThuan { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal LoiNhuanGop { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal LoiNhuanTruocThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueTndnHienHanh { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal LoiNhuanSauThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal LuuChuyenThuanTrongKy { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TienDauKy { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TienCuoiKy { get; set; }

    public TrangThaiBaoCaoTaiChinh TrangThai { get; set; } = TrangThaiBaoCaoTaiChinh.DangLap;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public ICollection<ChiTietChiTieuBctc> ChiTiets { get; set; } = new List<ChiTietChiTieuBctc>();

    [NotMapped]
    public decimal LuuChuyenTienThuan { get => LuuChuyenThuanTrongKy; set => LuuChuyenThuanTrongKy = value; }
}

public class ChiTietChiTieuBctc
{
    public long Id { get; set; }
    public long BaoCaoTaiChinhNamId { get; set; }

    public LoaiBaoCaoTaiChinh LoaiBaoCao { get; set; }

    [MaxLength(20)]
    public string MaChiTieu { get; set; } = string.Empty;

    [MaxLength(255)]
    public string TenChiTieu { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ThuyetMinh { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal SoDauNam { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal SoCuoiNam { get; set; }

    [MaxLength(500)]
    public string? CongThucThietLap { get; set; }

    public BaoCaoTaiChinhNam? BaoCaoTaiChinhNam { get; set; }
}
