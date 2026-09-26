using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class FixedAssetDepreciationTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public FixedAssetDepreciationTests()
    {
        _sqliteConnection = new SqliteConnection("Data Source=:memory:");
        _sqliteConnection.Open();

        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        using var context = new AppDbContext(_dbOptions);
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
    }

    public void Dispose()
    {
        _sqliteConnection.Dispose();
    }

    [Fact]
    public async Task ChotChan3_TieuChuanTscd_Duoi30Trieu_ThrowsException()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var taiSanService = new TaiSanService(context, butToanService, NullLogger<TaiSanService>.Instance);

        var tk211 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "211");
        var tk2141 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "2141");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "642");

        // Khai báo TSCĐ hữu hình nhưng nguyên giá chỉ 25.000.000 VNĐ (< 30M)
        var model = new TaiSanCreateViewModel
        {
            MaTaiSan = "TS-MAYIN-01",
            TenTaiSan = "Máy in văn phòng",
            LoaiTaiSan = LoaiTaiSan.TaiSanCoDinh,
            NgayGhiTang = DateTime.Today,
            NgayBatDauKhauHao = DateTime.Today,
            NguyenGia = 25000000m,
            ThoiGianSuDungThang = 36,
            TaiKhoanNguyenGiaId = tk211.Id,
            TaiKhoanKhauHaoId = tk2141.Id,
            TaiKhoanChiPhiId = tk642.Id
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await taiSanService.KhaiBaoTaiSanAsync(model);
        });

        Assert.Contains("không đủ tiêu chuẩn ghi nhận TSCĐ hữu hình (TK 211)", ex.Message);
    }

    [Fact]
    public async Task ChotChan3_TieuChuanTscd_ThoiGianSuDungNhoHon12Thang_ThrowsException()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var taiSanService = new TaiSanService(context, butToanService, NullLogger<TaiSanService>.Instance);

        var tk211 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "211");
        var tk2141 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "2141");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "642");

        // Khai báo TSCĐ nguyên giá 50M nhưng thời gian sử dụng 12 tháng (<= 12 tháng)
        var model = new TaiSanCreateViewModel
        {
            MaTaiSan = "TS-TB-01",
            TenTaiSan = "Thiết bị chuyên dùng ngắn hạn",
            LoaiTaiSan = LoaiTaiSan.TaiSanCoDinh,
            NgayGhiTang = DateTime.Today,
            NgayBatDauKhauHao = DateTime.Today,
            NguyenGia = 50000000m,
            ThoiGianSuDungThang = 12,
            TaiKhoanNguyenGiaId = tk211.Id,
            TaiKhoanKhauHaoId = tk2141.Id,
            TaiKhoanChiPhiId = tk642.Id
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await taiSanService.KhaiBaoTaiSanAsync(model);
        });

        Assert.Contains("phải trên 12 tháng", ex.Message);
    }

    [Fact]
    public async Task ChotChan4_GioiHanPhanBoCcdc_VuotQua36Thang_ThrowsException()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var taiSanService = new TaiSanService(context, butToanService, NullLogger<TaiSanService>.Instance);

        var tk242 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "242");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "642");

        // Khai báo CCDC phân bổ 48 tháng (> 36 tháng)
        var model = new TaiSanCreateViewModel
        {
            MaTaiSan = "CCDC-BAN-01",
            TenTaiSan = "Bộ bàn ghế họp",
            LoaiTaiSan = LoaiTaiSan.CongCuDungCu,
            NgayGhiTang = DateTime.Today,
            NgayBatDauKhauHao = DateTime.Today,
            NguyenGia = 18000000m,
            ThoiGianSuDungThang = 48,
            TaiKhoanNguyenGiaId = tk242.Id,
            TaiKhoanChiPhiId = tk642.Id
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await taiSanService.KhaiBaoTaiSanAsync(model);
        });

        Assert.Contains("tối đa không quá 36 tháng", ex.Message);
    }

    [Fact]
    public async Task TinhKhauHao_TronThangVaLeNgay_GeneratesGlVoucher_TongNoEqualsTongCo_No911()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var taiSanService = new TaiSanService(context, butToanService, NullLogger<TaiSanService>.Instance);

        var tk211 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "211");
        var tk2141 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "2141");
        var tk242 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "242");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "642");

        // 1. TSCĐ tròn tháng: Xe ô tô nguyên giá 600.000.000 đ, sử dụng 60 tháng (5 năm)
        // Mức trích mỗi tháng = 600M / 60 = 10.000.000 đ
        // Bắt đầu trích từ ngày đầu tháng: 2026-09-01
        var tsOto = await taiSanService.KhaiBaoTaiSanAsync(new TaiSanCreateViewModel
        {
            MaTaiSan = "TS-OTO-01",
            TenTaiSan = "Xe ô tô giám đốc",
            LoaiTaiSan = LoaiTaiSan.TaiSanCoDinh,
            NgayGhiTang = new DateTime(2026, 9, 1),
            NgayBatDauKhauHao = new DateTime(2026, 9, 1),
            NguyenGia = 600000000m,
            ThoiGianSuDungThang = 60,
            TaiKhoanNguyenGiaId = tk211.Id,
            TaiKhoanKhauHaoId = tk2141.Id,
            TaiKhoanChiPhiId = tk642.Id
        });

        // 2. CCDC lẻ ngày: Máy tính laptop nguyên giá 24.000.000 đ, sử dụng 24 tháng
        // Mức trích tròn tháng = 24M / 24 = 1.000.000 đ
        // Bắt đầu sử dụng từ ngày 16/09/2026. Tháng 9 có 30 ngày.
        // Số ngày sử dụng = 30 - 16 + 1 = 15 ngày (đúng nửa tháng).
        // Mức trích tháng 9 = 1.000.000 * 15 / 30 = 500.000 đ.
        var ccdcMayTinh = await taiSanService.KhaiBaoTaiSanAsync(new TaiSanCreateViewModel
        {
            MaTaiSan = "CCDC-MT-01",
            TenTaiSan = "Máy tính xách tay Dell",
            LoaiTaiSan = LoaiTaiSan.CongCuDungCu,
            NgayGhiTang = new DateTime(2026, 9, 16),
            NgayBatDauKhauHao = new DateTime(2026, 9, 16),
            NguyenGia = 24000000m,
            ThoiGianSuDungThang = 24,
            TaiKhoanNguyenGiaId = tk242.Id,
            TaiKhoanChiPhiId = tk642.Id
        });

        // 3. Xem trước bảng khấu hao kỳ 2026-09
        var bangDuTinh = await taiSanService.XemBangKhauHaoKyAsync("2026-09");
        Assert.False(bangDuTinh.DaChayKhauHao);
        Assert.Equal(2, bangDuTinh.ChiTiets.Count);

        var dongOto = bangDuTinh.ChiTiets.First(d => d.MaTaiSan == "TS-OTO-01");
        var dongCcdc = bangDuTinh.ChiTiets.First(d => d.MaTaiSan == "CCDC-MT-01");

        Assert.Equal(10000000m, dongOto.KhauHaoKyNay);
        Assert.Equal(500000m, dongCcdc.KhauHaoKyNay);
        Assert.Equal(10500000m, bangDuTinh.TongKhauHaoKy);

        // 4. Chạy và ghi sổ khấu hao kỳ 2026-09
        var ketQua = await taiSanService.ChayVaGhiSoKhauHaoKyAsync("2026-09");
        Assert.Equal(2, ketQua.Count);

        // Bút toán Sổ Cái GL được tự động sinh
        var butToanId = ketQua[0].ButToanId;
        Assert.NotNull(butToanId);

        var butToan = await context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstOrDefaultAsync(b => b.Id == butToanId);

        Assert.NotNull(butToan);
        Assert.Equal("KH-202609", butToan.SoChungTu);
        Assert.Equal(TrangThaiButToan.DaGhiSo, butToan.TrangThai);
        Assert.Equal(10500000m, butToan.TongTien);
        Assert.Equal(10500000m, butToan.TongNo);
        Assert.Equal(10500000m, butToan.TongCo);
        Assert.Equal(butToan.TongNo, butToan.TongCo); // Invariant TT99

        // Kiểm tra cặp tài khoản:
        // Dòng 1 (TSCĐ): Nợ 642 / Có 2141 = 10.000.000 đ
        // Dòng 2 (CCDC): Nợ 642 / Có 242 = 500.000 đ
        var dongBtOto = butToan.ChiTietButToans.First(d => d.TaiKhoanCoId == tk2141.Id);
        var dongBtCcdc = butToan.ChiTietButToans.First(d => d.TaiKhoanCoId == tk242.Id);

        Assert.Equal(tk642.Id, dongBtOto.TaiKhoanNoId);
        Assert.Equal(10000000m, dongBtOto.SoTien);

        Assert.Equal(tk642.Id, dongBtCcdc.TaiKhoanNoId);
        Assert.Equal(500000m, dongBtCcdc.SoTien);

        // Kiểm tra cập nhật giá trị lũy kế và còn lại trên tài sản
        var tsOtoSauKh = await context.TaiSanCoDinhs.FindAsync(tsOto.Id);
        Assert.NotNull(tsOtoSauKh);
        Assert.Equal(10000000m, tsOtoSauKh.GiaTriDaKhauHao);
        Assert.Equal(590000000m, tsOtoSauKh.GiaTriConLai);

        var ccdcSauKh = await context.TaiSanCoDinhs.FindAsync(ccdcMayTinh.Id);
        Assert.NotNull(ccdcSauKh);
        Assert.Equal(500000m, ccdcSauKh.GiaTriDaKhauHao);
        Assert.Equal(23500000m, ccdcSauKh.GiaTriConLai);
    }
}
