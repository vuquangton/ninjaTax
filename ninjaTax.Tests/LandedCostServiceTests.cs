using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class LandedCostServiceTests
{
    private async Task<AppDbContext> CreateDatabaseContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:memdb_landed_{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public async Task AllocateCostAsync_ByValue_AllocatesProportionallyAndAbsorbsRounding()
    {
        // Arrange
        using var context = await CreateDatabaseContextAsync();
        var butToanService = new ButToanService(context, new NullLogger<ButToanService>());
        var landedCostService = new LandedCostService(context, butToanService, new NullLogger<LandedCostService>());

        var branch = await context.ChiNhanhs.FirstAsync();
        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        var kho = new Kho { TenKho = "Kho Tổng", MaKho = "KT", ChiNhanhId = branch.Id, TaiKhoanKhoMacDinhId = tk1561.Id };
        var vt1 = new VatTuHangHoa { MaVatTu = "VT01", TenVatTu = "Vật tư 1", DonViTinh = "Cái", TaiKhoanKhoId = tk1561.Id };
        var vt2 = new VatTuHangHoa { MaVatTu = "VT02", TenVatTu = "Vật tư 2", DonViTinh = "Cái", TaiKhoanKhoId = tk1561.Id };

        context.Khos.Add(kho);
        context.VatTuHangHoas.AddRange(vt1, vt2);
        await context.SaveChangesAsync();

        var phieuNhap = new PhieuNhapKho
        {
            ChiNhanhId = branch.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-001",
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new ChiTietNhapKho { VatTuHangHoaId = vt1.Id, SoLuong = 10, DonGia = 1000000, ThanhTien = 10000000, TaiKhoanNoId = tk1561.Id, TaiKhoanCoId = tk331.Id },
                new ChiTietNhapKho { VatTuHangHoaId = vt2.Id, SoLuong = 10, DonGia = 3000000, ThanhTien = 30000000, TaiKhoanNoId = tk1561.Id, TaiKhoanCoId = tk331.Id }
            }
        };
        context.PhieuNhapKhos.Add(phieuNhap);

        var chungTuChiPhi = new ChungTuChiPhiMuaHang
        {
            ChiNhanhId = branch.Id,
            SoChungTu = "CPMH-001",
            TongChiPhi = 1000000, // 1 triệu tiền cước
            ThueSuatVat = 10,
            TienThueVat = 100000,
            TongThanhToan = 1100000
        };
        context.ChungTuChiPhiMuaHangs.Add(chungTuChiPhi);
        await context.SaveChangesAsync();

        var lineIds = phieuNhap.ChiTietNhapKhos.Select(l => l.Id).ToList();

        // Act: Phân bổ theo giá trị (10M / 40M = 25% -> 250k; 30M / 40M = 75% -> 750k)
        var result = await landedCostService.AllocateCostAsync(
            chungTuChiPhi.Id,
            lineIds,
            PhuongThucPhanBoChiPhi.TheoGiaTri,
            tk1561.Id,
            tk331.Id);

        // Assert
        Assert.True(result.Success);
        var updatedChungTu = await context.ChungTuChiPhiMuaHangs
            .Include(c => c.ChiTietPhanBos)
            .Include(c => c.ButToan)
            .FirstOrDefaultAsync(c => c.Id == chungTuChiPhi.Id);

        Assert.NotNull(updatedChungTu);
        Assert.True(updatedChungTu.DaPhanBo);
        Assert.NotNull(updatedChungTu.ButToan);

        var line1 = await context.ChiTietNhapKhos.FindAsync(lineIds[0]);
        var line2 = await context.ChiTietNhapKhos.FindAsync(lineIds[1]);

        Assert.NotNull(line1);
        Assert.NotNull(line2);
        Assert.Equal(250000m, line1.ChiPhiMuaHangPhanBo);
        Assert.Equal(750000m, line2.ChiPhiMuaHangPhanBo);
        Assert.Equal(1000000m, line1.ChiPhiMuaHangPhanBo + line2.ChiPhiMuaHangPhanBo);

        // Đơn giá sau phân bổ: Line 1 = (10,000,000 + 250,000) / 10 = 1,025,000
        Assert.Equal(1025000m, line1.DonGiaSauPhanBo);
        // Line 2 = (30,000,000 + 750,000) / 10 = 3,075,000
        Assert.Equal(3075000m, line2.DonGiaSauPhanBo);
    }

    [Fact]
    public async Task AllocateCostAsync_ByQuantity_AllocatesProportionally()
    {
        // Arrange
        using var context = await CreateDatabaseContextAsync();
        var butToanService = new ButToanService(context, new NullLogger<ButToanService>());
        var landedCostService = new LandedCostService(context, butToanService, new NullLogger<LandedCostService>());

        var branch = await context.ChiNhanhs.FirstAsync();
        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");

        var kho = new Kho { TenKho = "Kho 1", MaKho = "K1", ChiNhanhId = branch.Id, TaiKhoanKhoMacDinhId = tk1561.Id };
        var vt = new VatTuHangHoa { MaVatTu = "VT", TenVatTu = "Vật tư", DonViTinh = "Hộp", TaiKhoanKhoId = tk1561.Id };

        context.Khos.Add(kho);
        context.VatTuHangHoas.Add(vt);
        await context.SaveChangesAsync();

        var phieuNhap = new PhieuNhapKho
        {
            ChiNhanhId = branch.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-002",
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new ChiTietNhapKho { VatTuHangHoaId = vt.Id, SoLuong = 20, DonGia = 10000, ThanhTien = 200000, TaiKhoanNoId = tk1561.Id, TaiKhoanCoId = tk111.Id },
                new ChiTietNhapKho { VatTuHangHoaId = vt.Id, SoLuong = 80, DonGia = 10000, ThanhTien = 800000, TaiKhoanNoId = tk1561.Id, TaiKhoanCoId = tk111.Id }
            }
        };
        context.PhieuNhapKhos.Add(phieuNhap);

        var chungTuChiPhi = new ChungTuChiPhiMuaHang
        {
            ChiNhanhId = branch.Id,
            SoChungTu = "CPMH-002",
            TongChiPhi = 100000,
            TongThanhToan = 100000
        };
        context.ChungTuChiPhiMuaHangs.Add(chungTuChiPhi);
        await context.SaveChangesAsync();

        var lineIds = phieuNhap.ChiTietNhapKhos.Select(l => l.Id).ToList();

        // Act: Phân bổ theo số lượng: 20 vs 80 -> 20,000 và 80,000
        var result = await landedCostService.AllocateCostAsync(
            chungTuChiPhi.Id,
            lineIds,
            PhuongThucPhanBoChiPhi.TheoSoLuong,
            tk1561.Id,
            tk111.Id);

        // Assert
        Assert.True(result.Success);
        var line1 = await context.ChiTietNhapKhos.FindAsync(lineIds[0]);
        var line2 = await context.ChiTietNhapKhos.FindAsync(lineIds[1]);

        Assert.Equal(20000m, line1!.ChiPhiMuaHangPhanBo);
        Assert.Equal(80000m, line2!.ChiPhiMuaHangPhanBo);
    }
}
