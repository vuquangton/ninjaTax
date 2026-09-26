using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class SalesModuleTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public SalesModuleTests()
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
    public async Task PhatHanhHddt_AssignsTaxAuthorityCodeAndDigitalSignature()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var service = new HachToanBanHangService(context, butToanService, NullLogger<HachToanBanHangService>.Instance);

        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var vt = await context.VatTuHangHoas.FirstAsync(v => v.LoaiVatTu == LoaiVatTuHangHoa.HangHoa);
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk3331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "33311");

        var hoaDon = new HoaDonBanHang
        {
            SoChungTu = "BH-TEST-001",
            KHMauSo = "1C26TBB",
            KyHieu = "C26T",
            KhachHangId = kh.Id,
            DienGiai = "Bán thiết bị CNTT",
            ChiTietBans = new List<ChiTietHoaDonBan>
            {
                new ChiTietHoaDonBan
                {
                    DongSo = 1,
                    VatTuHangHoaId = vt.Id,
                    SoLuong = 1m,
                    DonGia = 30000000m,
                    ThanhTien = 30000000m,
                    ThueSuatVat = 10m,
                    TaiKhoanNoId = tk131.Id,
                    TaiKhoanDoanhThuId = tk511.Id,
                    TaiKhoanThueId = tk3331.Id
                }
            }
        };

        var (_, _, created) = await service.TaoMoiAsync(hoaDon);
        Assert.NotNull(created);
        Assert.Equal(TrangThaiHddt.MoiTao, created.TrangThai);

        // Phát hành HĐĐT
        var (ok, msg) = await service.PhatHanhHddtAsync(created.Id);
        Assert.True(ok, msg);

        var hdPhatHanh = await service.LayTheoIdAsync(created.Id);
        Assert.NotNull(hdPhatHanh);
        Assert.Equal(TrangThaiHddt.CoQuanThueCapMa, hdPhatHanh.TrangThai);
        Assert.False(string.IsNullOrEmpty(hdPhatHanh.MaCoQuanThue));
        Assert.False(string.IsNullOrEmpty(hdPhatHanh.ChuKySo));
        Assert.False(string.IsNullOrEmpty(hdPhatHanh.SoHoaDon));
    }

    [Fact]
    public async Task GhiSoHoaDonBan_GeneratesDualVouchers_RevenueAndCOGS_CompliantWithTT99()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var service = new HachToanBanHangService(context, butToanService, NullLogger<HachToanBanHangService>.Instance);

        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var vt = await context.VatTuHangHoas.FirstAsync(v => v.LoaiVatTu == LoaiVatTuHangHoa.HangHoa);
        var tk131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk3331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "33311");
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");
        var tk1561 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");

        var hoaDon = new HoaDonBanHang
        {
            SoChungTu = "BH-TEST-002",
            KHMauSo = "1C26TBB",
            KyHieu = "C26T",
            SoHoaDon = "00000002",
            KhachHangId = kh.Id,
            BanHangKiemXuatKho = true,
            DienGiai = "Bán máy chủ kiêm xuất kho",
            ChiTietBans = new List<ChiTietHoaDonBan>
            {
                new ChiTietHoaDonBan
                {
                    DongSo = 1,
                    VatTuHangHoaId = vt.Id,
                    SoLuong = 2m,
                    DonGia = 40000000m,
                    ThanhTien = 80000000m,
                    ThueSuatVat = 10m,
                    DonGiaVon = 25000000m, // Tổng vốn = 50.000.000 đ
                    TaiKhoanNoId = tk131.Id,
                    TaiKhoanDoanhThuId = tk511.Id,
                    TaiKhoanThueId = tk3331.Id,
                    TaiKhoanGiaVonId = tk632.Id,
                    TaiKhoanKhoId = tk1561.Id
                }
            }
        };

        var (_, _, created) = await service.TaoMoiAsync(hoaDon);
        Assert.NotNull(created);

        // Ghi sổ
        var (ok, msg, btDoanhThu, btGiaVon) = await service.GhiSoAsync(created.Id);

        Assert.True(ok, msg);
        Assert.NotNull(btDoanhThu);
        Assert.NotNull(btGiaVon);

        // 1. Kiểm tra Bút toán Doanh thu TT99 (Nợ 131/Có 5111: 80tr; Nợ 131/Có 33311: 8tr)
        Assert.Equal(88000000m, btDoanhThu.TongNo);
        Assert.Equal(88000000m, btDoanhThu.TongCo);
        Assert.Equal(btDoanhThu.TongNo, btDoanhThu.TongCo);
        Assert.Equal(2, btDoanhThu.ChiTietButToans.Count);

        // 2. Kiểm tra Bút toán Giá vốn TT99 (Nợ 632/Có 1561: 50tr)
        Assert.Equal(50000000m, btGiaVon.TongNo);
        Assert.Equal(50000000m, btGiaVon.TongCo);
        Assert.Equal(btGiaVon.TongNo, btGiaVon.TongCo);
        Assert.Single(btGiaVon.ChiTietButToans);

        // Kiểm tra liên kết ngược
        var hdUpdated = await service.LayTheoIdAsync(created.Id);
        Assert.NotNull(hdUpdated);
        Assert.True(hdUpdated.DaGhiSo);
        Assert.Equal(btDoanhThu.Id, hdUpdated.ButToanDoanhThuId);
        Assert.Equal(btGiaVon.Id, hdUpdated.ButToanGiaVonId);
    }
}
