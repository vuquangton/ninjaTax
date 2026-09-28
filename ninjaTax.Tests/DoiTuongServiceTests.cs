using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class DoiTuongServiceTests
{
    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:memdb_doituong_{Guid.NewGuid()}?mode=memory&cache=shared")
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
        var service = new DoiTuongService(context);

        var model = new DoiTuongCreateEditViewModel
        {
            MaDoiTuong = " kh_new_001 ",
            TenDoiTuong = "Công ty TNHH Giải Pháp Công Nghệ Alpha",
            Loai = LoaiDoiTuong.KhachHang,
            MaSoThue = "0101234567",
            DiaChi = "123 Đường Láng, Hà Nội",
            SoDienThoai = "0987654321",
            Email = "contact@alpha.vn",
            DangHoatDong = true
        };

        // Act
        var result = await service.TaoMoiAsync(model);

        // Assert
        Assert.True(result.ThanhCong, result.ThongBao);
        Assert.NotNull(result.DoiTuongId);

        var created = await context.DoiTuongs.FindAsync(result.DoiTuongId);
        Assert.NotNull(created);
        Assert.Equal("KH_NEW_001", created.MaDoiTuong);
        Assert.Equal("Công ty TNHH Giải Pháp Công Nghệ Alpha", created.TenDoiTuong);
    }

    [Fact]
    public async Task TaoMoiAsync_TrungMaDoiTuong_TraVeThatBai()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        // KH001 đã có trong seed data
        var service = new DoiTuongService(context);

        var model = new DoiTuongCreateEditViewModel
        {
            MaDoiTuong = "KH001",
            TenDoiTuong = "Nhà cung cấp Xi Măng Trùng Lặp",
            Loai = LoaiDoiTuong.NhaCungCap
        };

        // Act
        var result = await service.TaoMoiAsync(model);

        // Assert
        Assert.False(result.ThanhCong);
        Assert.Contains("đã tồn tại", result.ThongBao);
    }

    [Fact]
    public async Task XoaAsync_ChuaPhatSinhGiaoDich_XoaThanhCong()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var dt = new DoiTuong
        {
            MaDoiTuong = "KH999",
            TenDoiTuong = "Khách Hàng Chưa Có Phát Sinh",
            Loai = LoaiDoiTuong.KhachHang,
            DangHoatDong = true
        };
        context.DoiTuongs.Add(dt);
        await context.SaveChangesAsync();

        var service = new DoiTuongService(context);

        // Act
        var result = await service.XoaAsync(dt.Id);

        // Assert
        Assert.True(result.ThanhCong);
        Assert.Null(await context.DoiTuongs.FindAsync(dt.Id));
    }

    [Fact]
    public async Task XoaAsync_DaPhatSinhButToan_TuChoiVaBaoToanDuLieu()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var dt = new DoiTuong
        {
            MaDoiTuong = "KH_TRANSACTION",
            TenDoiTuong = "Khách Hàng Đã Phát Sinh",
            Loai = LoaiDoiTuong.KhachHang,
            DangHoatDong = true
        };
        context.DoiTuongs.Add(dt);
        await context.SaveChangesAsync();

        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan.StartsWith("511"));

        var bt = new ButToan
        {
            SoChungTu = "PKT-TEST-001",
            NgayHachToan = DateTime.Today,
            NgayChungTu = DateTime.Today,
            DienGiai = "Bán hàng cho KH_TRANSACTION",
            TongNo = 10000000,
            TongCo = 10000000,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        context.ButToans.Add(bt);
        await context.SaveChangesAsync();

        context.ChiTietButToans.Add(new ChiTietButToan
        {
            ButToanId = bt.Id,
            TaiKhoanNoId = tk131.Id,
            TaiKhoanCoId = tk511.Id,
            SoTien = 10000000,
            DoiTuongId = dt.Id
        });
        await context.SaveChangesAsync();

        var service = new DoiTuongService(context);

        // Act
        var result = await service.XoaAsync(dt.Id);

        // Assert
        Assert.False(result.ThanhCong);
        Assert.Contains("đã phát sinh giao dịch", result.ThongBao);
        Assert.NotNull(await context.DoiTuongs.FindAsync(dt.Id)); // Dữ liệu còn nguyên vẹn
    }

    [Fact]
    public async Task CapNhatAsync_ThongTinHopLe_CapNhatThanhCong()
    {
        // Arrange
        using var context = await GetDatabaseContextAsync();
        var dt = new DoiTuong
        {
            MaDoiTuong = "KH002",
            TenDoiTuong = "Tên Cũ",
            Loai = LoaiDoiTuong.KhachHang,
            DangHoatDong = true
        };
        context.DoiTuongs.Add(dt);
        await context.SaveChangesAsync();

        var service = new DoiTuongService(context);
        var model = new DoiTuongCreateEditViewModel
        {
            Id = dt.Id,
            MaDoiTuong = "KH002",
            TenDoiTuong = "Tên Mới Sau Cập Nhật",
            Loai = LoaiDoiTuong.KhachHang,
            DiaChi = "Tầng 5, Tòa nhà Bitexco, Q1, TP.HCM",
            DangHoatDong = true
        };

        // Act
        var result = await service.CapNhatAsync(dt.Id, model);

        // Assert
        Assert.True(result.ThanhCong);
        var updated = await context.DoiTuongs.FindAsync(dt.Id);
        Assert.NotNull(updated);
        Assert.Equal("Tên Mới Sau Cập Nhật", updated.TenDoiTuong);
        Assert.Equal("Tầng 5, Tòa nhà Bitexco, Q1, TP.HCM", updated.DiaChi);
    }
}
