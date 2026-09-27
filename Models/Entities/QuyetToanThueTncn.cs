using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ninjaTax.Models.Entities;

public class QuyetToanThueTncn
{
    public long Id { get; set; }
    public int NamQuyetToan { get; set; }

    [MaxLength(50)]
    public string SoChungTu { get; set; } = string.Empty;

    public DateTime NgayLap { get; set; } = DateTime.Today;

    public int TongSoNhanVienQuyetToan { get; set; }
    public int SoNhanVienUyQuyen { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThuNhapChiuThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThuNhapMienThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongGiamTruGiaCanh { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongBaoHiemBatBuoc { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThuNhapTinhThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThueDaKhauTru { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThuePhaiNopSauQtt { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThueNopThua { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThueConPhaiNopThem { get; set; }

    public TrangThaiQuyetToan TrangThai { get; set; } = TrangThaiQuyetToan.DangLap;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public ICollection<BangKeQttTncn051> DanhSach051 { get; set; } = new List<BangKeQttTncn051>();
    public ICollection<BangKeQttTncn052> DanhSach052 { get; set; } = new List<BangKeQttTncn052>();

    [NotMapped]
    public ICollection<BangKeQttTncn051> BangKe051s { get => DanhSach051; set => DanhSach051 = value; }
    [NotMapped]
    public ICollection<BangKeQttTncn052> BangKe052s { get => DanhSach052; set => DanhSach052 = value; }
}

public class BangKeQttTncn051
{
    public long Id { get; set; }
    public long QuyetToanThueTncnId { get; set; }
    public long NhanVienId { get; set; }

    [MaxLength(255)]
    public string HoTen { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? MaSoThue { get; set; }

    [MaxLength(20)]
    public string? SoCccd { get; set; }

    public bool CaNhanUyQuyenQuyetToan { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThuNhapChiuThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThuNhapMienThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal GiamTruBanThan { get; set; }

    public int SoNguoiPhuThuoc { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal GiamTruNguoiPhuThuoc { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal BaoHiemBatBuoc { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThuNhapTinhThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueDaKhauTruTrongNam { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThuePhaiNopSauQuyetToan { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueNopThua { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueConPhaiNop { get; set; }

    public NhanVien? NhanVien { get; set; }
    public QuyetToanThueTncn? QuyetToanThueTncn { get; set; }
}

public class BangKeQttTncn052
{
    public long Id { get; set; }
    public long QuyetToanThueTncnId { get; set; }
    public long NhanVienId { get; set; }

    [MaxLength(255)]
    public string HoTen { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? MaSoThue { get; set; }

    [MaxLength(20)]
    public string? SoCccd { get; set; }

    public bool CoCamKet08 { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal TongThuNhapChiuThue { get; set; }

    [Column(TypeName = "TEXT")]
    public decimal ThueTncnDaKhauTru10 { get; set; }

    public NhanVien? NhanVien { get; set; }
    public QuyetToanThueTncn? QuyetToanThueTncn { get; set; }
}
