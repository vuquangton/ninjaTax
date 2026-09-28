using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class VatTuHangHoaServiceTests
{
    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:memdb_vattu_{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;
    }

    private async Task<AppDbContext> GetDatabaseContextAsync()
    {
        var options = CreateNewContextOptions();
        var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
        return context;
    }

    [Fact]
    public async Task TaoMoiAsync_ThongTinHopLe_ThanhCongVaVietHoaMa()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var service = new VatTuHangHoaService(context);

        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk5111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan.StartsWith("511"));
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");

        var model = new VatTuHangHoaCreateViewModel
        {
            MaVatTu = " lap_dell_xps ",
            TenVatTu = "Laptop Dell XPS 15 9530",
            DonViTinh = "Chiếc",
            LoaiVatTu = LoaiVatTuHangHoa.HangHoa,
            TaiKhoanKhoId = tk1561.Id,
            TaiKhoanDoanhThuId = tk5111.Id,
            TaiKhoanGiaVonId = tk632.Id,
            ThueSuatVatMacDinh = 10,
            DonGiaMuaGanNhat = 35000000,
            DonGiaBanTieuChuan = 42000000,
            DangTheoDoiTonKho = true
        };

        // Act
        var result = await service.TaoMoiAsync(model);

        // Assert
        Assert.True(result.ThanhCong, result.ThongBao);
        Assert.NotNull(result.VatTuId);

        var created = await context.VatTuHangHoas.FindAsync(result.VatTuId);
        Assert.NotNull(created);
        Assert.Equal("LAP_DELL_XPS", created.MaVatTu);
        Assert.Equal("Laptop Dell XPS 15 9530", created.TenVatTu);
    }

    [Fact]
    public async Task TaoMoiAsync_TrungMaVatTu_BaoLoiThatBai()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var service = new VatTuHangHoaService(context);

        // HH001 đã có trong seed
        var model = new VatTuHangHoaCreateViewModel
        {
            MaVatTu = "HH001",
            TenVatTu = "Hàng hóa trùng mã",
            DonViTinh = "Cái"
        };

        // Act
        var result = await service.TaoMoiAsync(model);

        // Assert
        Assert.False(result.ThanhCong);
        Assert.Contains("đã tồn tại", result.ThongBao);
    }

    [Fact]
    public async Task CapNhatAsync_ThongTinHopLe_CapNhatThanhCong()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var vt = new VatTuHangHoa
        {
            MaVatTu = "VT_EDIT_TEST",
            TenVatTu = "Tên Ban Đầu",
            DonViTinh = "Hộp",
            LoaiVatTu = LoaiVatTuHangHoa.VatTu,
            DonGiaBanTieuChuan = 100000,
            DangHoatDong = true
        };
        context.VatTuHangHoas.Add(vt);
        await context.SaveChangesAsync();

        var service = new VatTuHangHoaService(context);
        var editModel = new VatTuHangHoaEditViewModel
        {
            Id = vt.Id,
            MaVatTu = "VT_EDIT_TEST",
            TenVatTu = "Tên Sau Cập Nhật",
            DonViTinh = "Thùng",
            LoaiVatTu = LoaiVatTuHangHoa.VatTu,
            DonGiaBanTieuChuan = 120000,
            DangHoatDong = true
        };

        // Act
        var result = await service.CapNhatAsync(vt.Id, editModel);

        // Assert
        Assert.True(result.ThanhCong, result.ThongBao);
        var updated = await context.VatTuHangHoas.FindAsync(vt.Id);
        Assert.NotNull(updated);
        Assert.Equal("Tên Sau Cập Nhật", updated.TenVatTu);
        Assert.Equal("Thùng", updated.DonViTinh);
        Assert.Equal(120000, updated.DonGiaBanTieuChuan);
    }

    [Fact]
    public async Task XoaAsync_DaPhatSinhGiaoDichKho_TuChoiVaBaoToanDuLieu()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var vt = new VatTuHangHoa
        {
            MaVatTu = "VT_TX_TEST",
            TenVatTu = "Vật tư có phiếu kho",
            DonViTinh = "Kg",
            DangHoatDong = true,
            DangTheoDoiTonKho = true
        };
        context.VatTuHangHoas.Add(vt);
        await context.SaveChangesAsync();

        var kho = await context.Khos.FirstAsync();
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);

        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = kho.ChiNhanhId,
            KhoId = kho.Id,
            NhaCungCapId = ncc.Id,
            SoPhieu = "PNK-TEST-9999",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            TrangThai = TrangThaiPhieuKho.DaGhiSo
        };
        context.PhieuNhapKhos.Add(pnk);
        await context.SaveChangesAsync();

        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        context.ChiTietNhapKhos.Add(new ChiTietNhapKho
        {
            PhieuNhapKhoId = pnk.Id,
            VatTuHangHoaId = vt.Id,
            SoLuong = 10,
            DonGia = 50000,
            ThanhTien = 500000,
            TaiKhoanNoId = tk1561.Id,
            TaiKhoanCoId = tk331.Id
        });
        await context.SaveChangesAsync();

        var service = new VatTuHangHoaService(context);

        // Act
        var result = await service.XoaAsync(vt.Id);

        // Assert
        Assert.False(result.ThanhCong);
        Assert.Contains("đã phát sinh giao dịch", result.ThongBao);
        Assert.NotNull(await context.VatTuHangHoas.FindAsync(vt.Id));
    }
}
