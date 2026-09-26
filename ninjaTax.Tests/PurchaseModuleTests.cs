using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class PurchaseModuleTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public PurchaseModuleTests()
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
    public async Task TaoMoiHoaDonMua_CalculatesTotalsAndSavesCorrectly()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var service = new HachToanMuaHangService(context, butToanService, NullLogger<HachToanMuaHangService>.Instance);

        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var vt = await context.VatTuHangHoas.FirstAsync(v => v.LoaiVatTu == LoaiVatTuHangHoa.HangHoa);
        var tkKho = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tkThue = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1331");
        var tkNoNCC = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        var hoaDon = new HoaDonMuaHang
        {
            SoChungTu = "MH-TEST-001",
            SoHoaDon = "00001234",
            NhaCungCapId = ncc.Id,
            NgayHoaDon = DateTime.Today,
            NgayHachToan = DateTime.Today,
            NgayChungTu = DateTime.Today,
            DienGiai = "Mua hàng hóa kiểm kho nhập",
            ChiTietHangs = new List<ChiTietHoaDonMua>
            {
                new ChiTietHoaDonMua
                {
                    DongSo = 1,
                    VatTuHangHoaId = vt.Id,
                    SoLuong = 2m,
                    DonGia = 10000000m,
                    ThanhTien = 20000000m,
                    TiLeChietKhau = 5m,     // CK 5% = 1.000.000 đ
                    ThueSuatVat = 10m,      // VAT 10% = 1.900.000 đ
                    TaiKhoanNoId = tkKho.Id,
                    TaiKhoanThueId = tkThue.Id,
                    TaiKhoanCoId = tkNoNCC.Id
                }
            }
        };

        var (ok, msg, created) = await service.TaoMoiAsync(hoaDon);

        Assert.True(ok, msg);
        Assert.NotNull(created);
        Assert.Equal(20000000m, created.TongTienHang);
        Assert.Equal(1000000m, created.TongTienChietKhau);
        Assert.Equal(1900000m, created.TongTienThueVat);
        Assert.Equal(20900000m, created.TongThanhToan);
        Assert.Equal(20900000m, created.ConPhaiTra);
        Assert.False(created.DaGhiSo);
    }

    [Fact]
    public async Task GhiSoHoaDonMua_AutoCreatesGeneralLedgerVoucher_CompliantWithTT99()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var service = new HachToanMuaHangService(context, butToanService, NullLogger<HachToanMuaHangService>.Instance);

        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var vt = await context.VatTuHangHoas.FirstAsync(v => v.LoaiVatTu == LoaiVatTuHangHoa.HangHoa);
        var tkKho = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561");
        var tkThue = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1331");
        var tkNoNCC = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        var hoaDon = new HoaDonMuaHang
        {
            SoChungTu = "MH-TEST-002",
            SoHoaDon = "00005678",
            KHMauSoHoaDon = "1C26TAA",
            KyHieuHoaDon = "C26T",
            NhaCungCapId = ncc.Id,
            DienGiai = "Mua server kiểm tra tự động ghi sổ TT99",
            ChiTietHangs = new List<ChiTietHoaDonMua>
            {
                new ChiTietHoaDonMua
                {
                    DongSo = 1,
                    VatTuHangHoaId = vt.Id,
                    SoLuong = 1m,
                    DonGia = 50000000m,
                    ThanhTien = 50000000m,
                    TiLeChietKhau = 0m,
                    ThueSuatVat = 10m,
                    TaiKhoanNoId = tkKho.Id,
                    TaiKhoanThueId = tkThue.Id,
                    TaiKhoanCoId = tkNoNCC.Id
                }
            }
        };

        var (_, _, created) = await service.TaoMoiAsync(hoaDon);
        Assert.NotNull(created);

        // Ghi sổ
        var (okGhiSo, msgGhiSo, butToan) = await service.GhiSoAsync(created.Id);

        Assert.True(okGhiSo, msgGhiSo);
        Assert.NotNull(butToan);
        Assert.Equal(55000000m, butToan.TongNo);
        Assert.Equal(55000000m, butToan.TongCo);
        Assert.Equal(butToan.TongNo, butToan.TongCo); // Double-entry balance
        Assert.Equal($"{hoaDon.KyHieuHoaDon}-{hoaDon.SoHoaDon}", butToan.SoChungTuGoc); // Mandatory source doc

        // Kiểm tra chi tiết định khoản
        Assert.Equal(2, butToan.ChiTietButToans.Count); // Nợ 1561/Có 331 (50tr), Nợ 1331/Có 331 (5tr)

        var hdUpdated = await service.LayTheoIdAsync(created.Id);
        Assert.NotNull(hdUpdated);
        Assert.True(hdUpdated.DaGhiSo);
        Assert.Equal(butToan.Id, hdUpdated.ButToanId);
    }
}
