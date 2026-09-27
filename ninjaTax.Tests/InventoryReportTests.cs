using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Controllers;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class InventoryReportTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public InventoryReportTests()
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

    private InventoryService CreateInventoryService(AppDbContext context)
    {
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        return new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);
    }

    [Fact]
    public async Task LapBaoCaoNhapXuatTon_CorrectlyCalculates_Opening_Inward_Outward_Closing()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");

        // 1. Nhập trước kỳ (ngày 01/01/2026) -> Trở thành tồn đầu kỳ
        var pnkDau = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-PREV-01",
            NgayNhap = new DateTime(2026, 1, 5),
            NgayHachToan = new DateTime(2026, 1, 5),
            NhaCungCapId = ncc.Id,
            LoaiNhapKho = LoaiNhapKho.MuaNgoai,
            DienGiai = "Nhập tồn đầu năm",
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10,
                    DonGia = 100_000m,
                    ThanhTien = 1_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await inventoryService.SavePhieuNhapKhoAsync(pnkDau);
        await inventoryService.GhiSoPhieuNhapKhoAsync(pnkDau.Id);

        // 2. Nhập trong kỳ (tháng 2/2026)
        var pnkTrongKy = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-CURR-01",
            NgayNhap = new DateTime(2026, 2, 10),
            NgayHachToan = new DateTime(2026, 2, 10),
            NhaCungCapId = ncc.Id,
            LoaiNhapKho = LoaiNhapKho.MuaNgoai,
            DienGiai = "Nhập trong tháng 2",
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 20,
                    DonGia = 110_000m,
                    ThanhTien = 2_200_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await inventoryService.SavePhieuNhapKhoAsync(pnkTrongKy);
        await inventoryService.GhiSoPhieuNhapKhoAsync(pnkTrongKy.Id);

        // 3. Xuất trong kỳ (tháng 2/2026)
        var pxkTrongKy = new PhieuXuatKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PXK-CURR-01",
            NgayXuat = new DateTime(2026, 2, 15),
            NgayHachToan = new DateTime(2026, 2, 15),
            KhachHangId = kh.Id,
            LoaiXuatKho = LoaiXuatKho.BanHang,
            DienGiai = "Xuất bán trong tháng 2",
            ChiTietXuatKhos = new List<ChiTietXuatKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 15,
                    DonGiaVon = 0, // Sẽ tự động tính BQGQ
                    TaiKhoanNoId = tk632.Id,
                    TaiKhoanCoId = tk156.Id
                }
            }
        };
        await inventoryService.SavePhieuXuatKhoAsync(pxkTrongKy);
        await inventoryService.GhiSoPhieuXuatKhoAsync(pxkTrongKy.Id);

        // Lập báo cáo tháng 2: Từ 01/02/2026 đến 28/02/2026
        var report = await inventoryService.LapBaoCaoNhapXuatTonAsync(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 28),
            kho.Id,
            chiNhanh.Id);

        Assert.NotNull(report);
        var itemReport = report.Items.FirstOrDefault(i => i.VatTuHangHoaId == vatTu.Id);
        Assert.NotNull(itemReport);

        // Tồn đầu: 10 cái, 1.000.000 VNĐ
        Assert.Equal(10m, itemReport.TonDauSoLuong);
        Assert.Equal(1_000_000m, itemReport.TonDauThanhTien);

        // Nhập trong kỳ: 20 cái, 2.200.000 VNĐ
        Assert.Equal(20m, itemReport.NhapSoLuong);
        Assert.Equal(2_200_000m, itemReport.NhapThanhTien);

        // Xuất trong kỳ: 15 cái
        Assert.Equal(15m, itemReport.XuatSoLuong);
        Assert.True(itemReport.XuatThanhTien > 0);

        // Bất biến NXT: Tồn cuối = Tồn đầu + Nhập - Xuất
        Assert.Equal(itemReport.TonDauSoLuong + itemReport.NhapSoLuong - itemReport.XuatSoLuong, itemReport.TonCuoiSoLuong);
        Assert.Equal(itemReport.TonDauThanhTien + itemReport.NhapThanhTien - itemReport.XuatThanhTien, itemReport.TonCuoiThanhTien);

        // Tồn cuối số lượng = 10 + 20 - 15 = 15
        Assert.Equal(15m, itemReport.TonCuoiSoLuong);

        // Khớp 100% với Sổ Cái TK Kho (1561)
        Assert.True(report.IsKhopSoCai);
        Assert.True(Math.Abs(report.TongTonCuoiThanhTien - report.SoDuSoCaiTkKho) < 1.0m);
    }

    [Fact]
    public async Task LapBaoCaoNhapXuatTon_FiltersByWarehouseCorrectly()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var khoTong = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var khoNvl = await context.Khos.FirstAsync(k => k.MaKho == "KHO-NVL");
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Nhập vào Kho Tổng
        var pnk1 = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = khoTong.Id,
            SoPhieu = "PNK-FILTER-01",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            NhaCungCapId = ncc.Id,
            LoaiNhapKho = LoaiNhapKho.MuaNgoai,
            DienGiai = "Nhập Kho Tổng",
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 50,
                    DonGia = 100_000m,
                    ThanhTien = 5_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        await inventoryService.SavePhieuNhapKhoAsync(pnk1);
        await inventoryService.GhiSoPhieuNhapKhoAsync(pnk1.Id);

        // Lọc theo Kho NVL (không có phát sinh)
        var reportKhoNvl = await inventoryService.LapBaoCaoNhapXuatTonAsync(
            DateTime.Today.AddDays(-1),
            DateTime.Today.AddDays(1),
            khoNvl.Id,
            chiNhanh.Id);

        var itemNvl = reportKhoNvl.Items.FirstOrDefault(i => i.VatTuHangHoaId == vatTu.Id);
        Assert.Null(itemNvl); // Không phát sinh ở Kho NVL

        // Lọc theo Kho Tổng (có phát sinh 50)
        var reportKhoTong = await inventoryService.LapBaoCaoNhapXuatTonAsync(
            DateTime.Today.AddDays(-1),
            DateTime.Today.AddDays(1),
            khoTong.Id,
            chiNhanh.Id);

        var itemTong = reportKhoTong.Items.FirstOrDefault(i => i.VatTuHangHoaId == vatTu.Id);
        Assert.NotNull(itemTong);
        Assert.Equal(50m, itemTong.NhapSoLuong);
        Assert.Equal(50m, itemTong.TonCuoiSoLuong);
    }

    [Fact]
    public async Task InventoryController_BaoCaoNhapXuatTon_ReturnsViewWithModel()
    {
        using var context = new AppDbContext(_dbOptions);
        var inventoryService = CreateInventoryService(context);
        var controller = new InventoryController(inventoryService, context, NullLogger<InventoryController>.Instance);

        var result = await controller.BaoCaoNhapXuatTon(null, null, null, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BaoCaoNhapXuatTonViewModel>(viewResult.Model);
        Assert.NotNull(model.BranchList);
        Assert.NotNull(model.WarehouseList);
    }
}
