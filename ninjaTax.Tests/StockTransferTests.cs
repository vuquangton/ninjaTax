using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class StockTransferTests
{
    private async Task<AppDbContext> CreateInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=file:mem_stock_transfer_test?mode=memory&cache=shared")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public async Task TaoPhieuDieuChuyenKho_TransfersStockAndPostsGLCorrectly()
    {
        using var context = await CreateInMemoryDbContextAsync();
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var inventoryService = new InventoryService(context, butToanService, NullLogger<InventoryService>.Instance);

        var branch = await context.ChiNhanhs.FirstAsync();
        var khoA = new Kho { MaKho = "KHO-HN", TenKho = "Kho Hà Nội", ChiNhanhId = branch.Id, DangHoatDong = true };
        var khoB = new Kho { MaKho = "KHO-HP", TenKho = "Kho Hải Phòng", ChiNhanhId = branch.Id, DangHoatDong = true };
        context.Khos.AddRange(khoA, khoB);

        var vatTu = new VatTuHangHoa
        {
            MaVatTu = "LAPTOP-DELL",
            TenVatTu = "Laptop Dell Latitude",
            DonViTinh = "Cái",
            LoaiVatTu = LoaiVatTuHangHoa.HangHoa,
            DangHoatDong = true,
            DangTheoDoiTonKho = true
        };
        context.VatTuHangHoas.Add(vatTu);
        await context.SaveChangesAsync();

        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // 1. Nhập trước 20 cái vào Kho A
        var pn = new PhieuNhapKho
        {
            ChiNhanhId = branch.Id,
            KhoId = khoA.Id,
            SoPhieu = "PNK-001",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            TrangThai = TrangThaiPhieuKho.DaGhiSo,
            TongSoLuong = 20,
            TongTienHang = 200_000_000,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new ChiTietNhapKho
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 20,
                    DonGia = 10_000_000,
                    ThanhTien = 200_000_000,
                    TaiKhoanNoId = tk1561.Id,
                    TaiKhoanCoId = tk331.Id
                }
            }
        };
        context.PhieuNhapKhos.Add(pn);
        await context.SaveChangesAsync();

        // 2. Thử chuyển 25 cái từ Kho A sang Kho B -> Phải ném lỗi xuất âm
        var phieuQuaHan = new PhieuDieuChuyenKho
        {
            ChiNhanhId = branch.Id,
            KhoXuatId = khoA.Id,
            KhoNhapId = khoB.Id,
            SoPhieu = "DCK-OVERFLOW",
            NgayDieuChuyen = DateTime.Today,
            NgayHachToan = DateTime.Today,
            ChiTietDieuChuyens = new List<ChiTietDieuChuyenKho>
            {
                new ChiTietDieuChuyenKho
                {
                    VatTuHangHoaId = vatTu.Id,
                    DonViTinh = "Cái",
                    SoLuong = 25,
                    DonGiaVon = 10_000_000,
                    ThanhTien = 250_000_000
                }
            }
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => inventoryService.TaoPhieuDieuChuyenKhoAsync(phieuQuaHan));

        // 3. Chuyển hợp lệ 10 cái từ Kho A sang Kho B
        var phieuHopLe = new PhieuDieuChuyenKho
        {
            ChiNhanhId = branch.Id,
            KhoXuatId = khoA.Id,
            KhoNhapId = khoB.Id,
            SoPhieu = "DCK-2026-001",
            NgayDieuChuyen = DateTime.Today,
            NgayHachToan = DateTime.Today,
            ChiTietDieuChuyens = new List<ChiTietDieuChuyenKho>
            {
                new ChiTietDieuChuyenKho
                {
                    VatTuHangHoaId = vatTu.Id,
                    DonViTinh = "Cái",
                    SoLuong = 10,
                    DonGiaVon = 10_000_000,
                    ThanhTien = 100_000_000
                }
            }
        };

        var result = await inventoryService.TaoPhieuDieuChuyenKhoAsync(phieuHopLe);
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        // Cùng tài khoản 1561 -> 1561: Không tạo bút toán GL trùng Nợ/Có mà ghi sổ qua PhieuXuatKho/PhieuNhapKho
        Assert.Null(result.ButToanId);

        // 4. Kiểm tra tồn kho sau điều chuyển
        var tonKhoA = await inventoryService.GetStockBalanceAsync(khoA.Id, vatTu.Id);
        var tonKhoB = await inventoryService.GetStockBalanceAsync(khoB.Id, vatTu.Id);
        Assert.Equal(10, tonKhoA);
        Assert.Equal(10, tonKhoB);

        // 5. Kiểm tra hủy điều chuyển kho
        var huyOk = await inventoryService.HuyPhieuDieuChuyenKhoAsync(result.Id);
        Assert.True(huyOk);

        var tonKhoASauHuy = await inventoryService.GetStockBalanceAsync(khoA.Id, vatTu.Id);
        var tonKhoBSauHuy = await inventoryService.GetStockBalanceAsync(khoB.Id, vatTu.Id);
        Assert.Equal(20, tonKhoASauHuy);
        Assert.Equal(0, tonKhoBSauHuy);

        // 6. Kiểm tra điều chuyển khác tài khoản (ví dụ 152 -> 1561): Phải sinh bút toán Sổ Cái TongNo == TongCo
        var tk152 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "152");
        var phieuKhacTk = new PhieuDieuChuyenKho
        {
            ChiNhanhId = branch.Id,
            KhoXuatId = khoA.Id,
            KhoNhapId = khoB.Id,
            SoPhieu = "DCK-2026-002",
            NgayDieuChuyen = DateTime.Today,
            NgayHachToan = DateTime.Today,
            ChiTietDieuChuyens = new List<ChiTietDieuChuyenKho>
            {
                new ChiTietDieuChuyenKho
                {
                    VatTuHangHoaId = vatTu.Id,
                    DonViTinh = "Cái",
                    SoLuong = 5,
                    DonGiaVon = 10_000_000,
                    ThanhTien = 50_000_000,
                    TaiKhoanXuatId = tk152.Id,
                    TaiKhoanNhapId = tk1561.Id
                }
            }
        };

        var resultKhacTk = await inventoryService.TaoPhieuDieuChuyenKhoAsync(phieuKhacTk);
        Assert.NotNull(resultKhacTk);
        Assert.NotNull(resultKhacTk.ButToanId);

        var bt = await context.ButToans.Include(b => b.ChiTietButToans).FirstOrDefaultAsync(b => b.Id == resultKhacTk.ButToanId);
        Assert.NotNull(bt);
        Assert.Equal(50_000_000, bt.TongNo);
        Assert.Equal(50_000_000, bt.TongCo);
    }
}
