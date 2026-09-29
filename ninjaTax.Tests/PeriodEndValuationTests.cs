using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class PeriodEndValuationTests
{
    private async Task<AppDbContext> CreateDatabaseContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:memdb_val_{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public async Task RecalculatePeriodWeightedAverageCost_UpdatesIssueUnitCostAndGlVouchers()
    {
        // Arrange
        using var context = await CreateDatabaseContextAsync();
        var butToanService = new ButToanService(context, new NullLogger<ButToanService>());
        var inventoryService = new InventoryService(context, butToanService, new NullLogger<InventoryService>());

        var branch = await context.ChiNhanhs.FirstAsync();
        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        var kho = new Kho { TenKho = "Kho Hàng Hóa", MaKho = "KHH", ChiNhanhId = branch.Id, TaiKhoanKhoMacDinhId = tk1561.Id };
        var vt = new VatTuHangHoa { MaVatTu = "IP16", TenVatTu = "iPhone 16", DonViTinh = "Cái", DonGiaMuaGanNhat = 20000000, TaiKhoanKhoId = tk1561.Id };

        context.Khos.Add(kho);
        context.VatTuHangHoas.Add(vt);
        await context.SaveChangesAsync();

        var tuNgay = new DateTime(2026, 3, 1);
        var denNgay = new DateTime(2026, 3, 31);

        // Nhập lô 1: 10 cái @ 20,000,000 = 200,000,000
        var pnk1 = new PhieuNhapKho
        {
            ChiNhanhId = branch.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-03-01",
            NgayHachToan = new DateTime(2026, 3, 5),
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new ChiTietNhapKho { VatTuHangHoaId = vt.Id, SoLuong = 10, DonGia = 20000000, ThanhTien = 200000000, TaiKhoanNoId = tk1561.Id, TaiKhoanCoId = tk331.Id }
            }
        };

        // Nhập lô 2: 10 cái @ 22,000,000 = 220,000,000 (Tổng nhập = 20 cái @ 420M -> BQ = 21,000,000)
        var pnk2 = new PhieuNhapKho
        {
            ChiNhanhId = branch.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-03-02",
            NgayHachToan = new DateTime(2026, 3, 15),
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new ChiTietNhapKho { VatTuHangHoaId = vt.Id, SoLuong = 10, DonGia = 22000000, ThanhTien = 220000000, TaiKhoanNoId = tk1561.Id, TaiKhoanCoId = tk331.Id }
            }
        };

        // Xuất kho giữa kỳ: 5 cái, ban đầu tạm tính giá vốn @ 20,000,000 = 100,000,000
        var butToanXuat = new ButToan
        {
            SoChungTu = "PKT-XK-01",
            NgayHachToan = new DateTime(2026, 3, 20),
            TongTien = 100000000,
            TongNo = 100000000,
            TongCo = 100000000,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new ChiTietButToan { TaiKhoanNoId = tk632.Id, TaiKhoanCoId = tk1561.Id, SoTien = 100000000 }
            }
        };

        var pxk = new PhieuXuatKho
        {
            ChiNhanhId = branch.Id,
            KhoId = kho.Id,
            SoPhieu = "PXK-03-01",
            NgayHachToan = new DateTime(2026, 3, 20),
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            TongTienGiaVon = 100000000,
            ButToan = butToanXuat,
            ChiTietXuatKhos = new List<ChiTietXuatKho>
            {
                new ChiTietXuatKho
                {
                    VatTuHangHoaId = vt.Id,
                    SoLuong = 5,
                    DonGiaVon = 20000000,
                    TienGiaVon = 100000000,
                    TaiKhoanNoId = tk632.Id,
                    TaiKhoanCoId = tk1561.Id
                }
            }
        };

        context.PhieuNhapKhos.AddRange(pnk1, pnk2);
        context.PhieuXuatKhos.Add(pxk);
        await context.SaveChangesAsync();

        // Act: Tính lại giá xuất kho BQGQ tháng 3/2026
        var result = await inventoryService.RecalculatePeriodWeightedAverageCostAsync(tuNgay, denNgay, kho.Id, branch.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.SoDongCapNhat);

        var updatedLine = await context.ChiTietXuatKhos.FirstAsync(c => c.PhieuXuatKhoId == pxk.Id);
        // Đơn giá BQ mới = 420M / 20 = 21,000,000
        Assert.Equal(21000000m, updatedLine.DonGiaVon);
        Assert.Equal(105000000m, updatedLine.TienGiaVon);
        Assert.Equal(5000000m, updatedLine.ChenhLechGiaVon);

        var updatedPxk = await context.PhieuXuatKhos.Include(p => p.ButToan).FirstAsync(p => p.Id == pxk.Id);
        Assert.Equal(105000000m, updatedPxk.TongTienGiaVon);
        Assert.NotNull(updatedPxk.ButToan);
        Assert.Equal(105000000m, updatedPxk.ButToan.TongTien);
    }
}
