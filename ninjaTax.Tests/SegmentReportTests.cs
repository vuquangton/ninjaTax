using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class SegmentReportTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public SegmentReportTests()
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
    public async Task LapBaoCaoBoPhan_DoiChieuKhopDoanhThuVaChiPhiB02()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var reportService = new FinancialReportService(context, NullLogger<FinancialReportService>.Instance);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "511");
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk6421 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "6421");
        var tk6422 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "6422");
        var tk154 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "154");
        var tk334 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "334");

        // 1. Tạo 2 phòng ban test
        var pbSales = new PhongBan
        {
            ChiNhanhId = chiNhanh.Id,
            MaPhongBan = "PB-TEST-SEG-SALES",
            TenPhongBan = "Bộ Phận Bán Hàng Dự Án",
            LoaiPhongBan = LoaiPhongBan.BanHang,
            LaTrungTamLoiNhuan = true,
            DangHoatDong = true
        };
        var pbOps = new PhongBan
        {
            ChiNhanhId = chiNhanh.Id,
            MaPhongBan = "PB-TEST-SEG-OPS",
            TenPhongBan = "Bộ Phận Vận Hành & Sản Xuất",
            LoaiPhongBan = LoaiPhongBan.SanXuat,
            LaTrungTamLoiNhuan = false,
            DangHoatDong = true
        };

        await context.PhongBans.AddRangeAsync(pbSales, pbOps);
        await context.SaveChangesAsync();

        var namTaiChinh = 2026;
        var ngayHachToan = new DateTime(namTaiChinh, 6, 15);

        // 2. Bút toán doanh thu gán cho Sales: Nợ 131 / Có 511 = 100.000.000đ
        var btDoanhThu = new ButToan
        {
            SoChungTu = "HD-SALES-001",
            SoChungTuGoc = "HĐ-GOC-001",
            NgayChungTuGoc = ngayHachToan,
            NgayChungTu = ngayHachToan,
            NgayHachToan = ngayHachToan,
            DienGiai = "Doanh thu bán hàng dự án",
            TongTien = 100_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DienGiai = "Doanh thu hợp đồng",
                    TaiKhoanNoId = tk131.Id,
                    TaiKhoanCoId = tk511.Id,
                    SoTien = 100_000_000m,
                    PhongBanId = pbSales.Id
                }
            }
        };

        // 3. Bút toán chi phí bán hàng: Nợ 6421 / Có 334 = 30.000.000đ
        var btCpSales = new ButToan
        {
            SoChungTu = "CP-SALES-001",
            SoChungTuGoc = "CP-GOC-001",
            NgayChungTuGoc = ngayHachToan,
            NgayChungTu = ngayHachToan,
            NgayHachToan = ngayHachToan,
            DienGiai = "Lương bộ phận bán hàng",
            TongTien = 30_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DienGiai = "Chi phí lương bán hàng",
                    TaiKhoanNoId = tk6421.Id,
                    TaiKhoanCoId = tk334.Id,
                    SoTien = 30_000_000m,
                    PhongBanId = pbSales.Id
                }
            }
        };

        // 4. Bút toán chi phí sản xuất: Nợ 154 / Có 334 = 20.000.000đ
        var btCpOps = new ButToan
        {
            SoChungTu = "CP-OPS-001",
            SoChungTuGoc = "CP-GOC-002",
            NgayChungTuGoc = ngayHachToan,
            NgayChungTu = ngayHachToan,
            NgayHachToan = ngayHachToan,
            DienGiai = "Lương nhân công phân xưởng",
            TongTien = 20_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DienGiai = "Chi phí nhân công trực tiếp",
                    TaiKhoanNoId = tk154.Id,
                    TaiKhoanCoId = tk334.Id,
                    SoTien = 20_000_000m,
                    PhongBanId = pbOps.Id
                }
            }
        };

        // 5. Bút toán chi phí quản lý chung chưa gán phòng ban: Nợ 6422 / Có 334 = 10.000.000đ
        var btCpChung = new ButToan
        {
            SoChungTu = "CP-CHUNG-001",
            SoChungTuGoc = "CP-GOC-003",
            NgayChungTuGoc = ngayHachToan,
            NgayChungTu = ngayHachToan,
            NgayHachToan = ngayHachToan,
            DienGiai = "Chi phí văn phòng chung",
            TongTien = 10_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DienGiai = "Chi phí quản lý chung",
                    TaiKhoanNoId = tk6422.Id,
                    TaiKhoanCoId = tk334.Id,
                    SoTien = 10_000_000m,
                    PhongBanId = null
                }
            }
        };

        var (_, _, res1) = await butToanService.TaoMoiAsync(btDoanhThu);
        await butToanService.GhiSoAsync(res1!.Id);

        var (_, _, res2) = await butToanService.TaoMoiAsync(btCpSales);
        await butToanService.GhiSoAsync(res2!.Id);

        var (_, _, res3) = await butToanService.TaoMoiAsync(btCpOps);
        await butToanService.GhiSoAsync(res3!.Id);

        var (_, _, res4) = await butToanService.TaoMoiAsync(btCpChung);
        await butToanService.GhiSoAsync(res4!.Id);

        // 6. Lập Báo cáo bộ phận theo IFRS 8 / VAS 28
        var report = await reportService.LapBaoCaoBoPhanAsync(namTaiChinh);

        Assert.NotNull(report);
        Assert.NotEmpty(report.Segments);

        // Kiểm tra bộ phận Sales
        var segSales = report.Segments.FirstOrDefault(s => s.PhongBanId == pbSales.Id);
        Assert.NotNull(segSales);
        Assert.Equal(100_000_000m, segSales.DoanhThuBanHang);
        Assert.Equal(30_000_000m, segSales.ChiPhiBanHang);
        Assert.Equal(70_000_000m, segSales.LoiNhuanThuanBoPhan);

        // Kiểm tra bộ phận Ops
        var segOps = report.Segments.FirstOrDefault(s => s.PhongBanId == pbOps.Id);
        Assert.NotNull(segOps);
        Assert.Equal(20_000_000m, segOps.GiaVonBanHang);

        // Kiểm tra tổng cộng toàn công ty
        Assert.Equal(100_000_000m, report.TongCongToanCongTy.DoanhThuBanHang);
        Assert.Equal(60_000_000m, report.TongCongToanCongTy.TongChiPhiHoatDong); // 20m + 30m + 10m
        Assert.Equal(40_000_000m, report.TongCongToanCongTy.LoiNhuanThuanBoPhan); // 100m - 60m

        // Kiểm tra đối soát với B02-DN
        Assert.True(report.IsKhopDoanhThuB02);
    }
}
