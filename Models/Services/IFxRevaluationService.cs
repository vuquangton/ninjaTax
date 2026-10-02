using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public class ChiTietDanhGiaLaiPreviewItem
{
    public long TaiKhoanId { get; set; }
    public string MaTaiKhoan { get; set; } = string.Empty;
    public string TenTaiKhoan { get; set; } = string.Empty;
    public long? DoiTuongId { get; set; }
    public string? MaDoiTuong { get; set; }
    public string? TenDoiTuong { get; set; }
    public decimal SoDuNgoaiTe { get; set; }
    public decimal TyGiaGhiSo { get; set; }
    public decimal GiaTriGhiSoVnd { get; set; }
    public decimal TyGiaDanhGiaLai { get; set; }
    public decimal GiaTriDanhGiaLaiVnd { get; set; }
    public decimal ChenhLechVnd { get; set; }
    public bool LaLai => ChenhLechVnd > 0;
}

public class FxRevaluationPreviewModel
{
    public DateTime NgayDanhGia { get; set; }
    public string LoaiTien { get; set; } = "USD";
    public decimal TyGiaMua { get; set; }
    public decimal TyGiaBan { get; set; }
    public decimal TongLaiTyGia => ChiTiets.Where(c => c.LaLai).Sum(c => c.ChenhLechVnd);
    public decimal TongLoTyGia => ChiTiets.Where(c => !c.LaLai).Sum(c => Math.Abs(c.ChenhLechVnd));
    public decimal ChenhLechThuan => TongLaiTyGia - TongLoTyGia;
    public List<ChiTietDanhGiaLaiPreviewItem> ChiTiets { get; set; } = new();
}

public interface IFxRevaluationService
{
    Task<FxRevaluationPreviewModel> XemTruocDanhGiaLaiAsync(DateTime ngayDanhGia, string loaiTien, decimal tyGiaMua, decimal tyGiaBan);
    Task<DanhGiaLaiNgoaiTe> ThucHienDanhGiaLaiCuoiKyAsync(DateTime ngayDanhGia, string loaiTien, decimal tyGiaMua, decimal tyGiaBan, string? ghiChu = null);
    Task<(bool ThanhCong, string? ThongBao)> HuyDanhGiaLaiAsync(long danhGiaLaiId);
}
