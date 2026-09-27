using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using Xunit;

namespace ninjaTax.Tests;

public class WarehouseEntityTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public WarehouseEntityTests()
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
    public async Task DbInitializer_SeedsDefaultWarehouses()
    {
        using var context = new AppDbContext(_dbOptions);

        var khos = await context.Khos.ToListAsync();
        Assert.NotEmpty(khos);
        Assert.Contains(khos, k => k.MaKho == "KHO-TONG");
        Assert.Contains(khos, k => k.MaKho == "KHO-NVL");

        var khoTong = khos.First(k => k.MaKho == "KHO-TONG");
        Assert.True(khoTong.DangHoatDong);
        Assert.NotNull(khoTong.TaiKhoanKhoMacDinhId);
    }

    [Fact]
    public async Task PhieuNhapKho_TaoThanhCong_LuuDayDuChiTiet()
    {
        using var context = new AppDbContext(_dbOptions);
        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        var pnk = new PhieuNhapKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PNK-2026-0001",
            NgayNhap = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LoaiNhapKho = LoaiNhapKho.MuaNgoai,
            NhaCungCapId = ncc.Id,
            DienGiai = "Nhập kho lô hàng mới",
            TongSoLuong = 10m,
            TongTienHang = 50_000_000m,
            TrangThai = TrangThaiPhieuKho.TamTinh,
            ChiTietNhapKhos = new List<ChiTietNhapKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 10m,
                    DonGia = 5_000_000m,
                    ThanhTien = 50_000_000m,
                    TaiKhoanNoId = tk156.Id,
                    TaiKhoanCoId = tk331.Id,
                    SoLo = "LOT-2026-01"
                }
            }
        };

        await context.PhieuNhapKhos.AddAsync(pnk);
        await context.SaveChangesAsync();

        var saved = await context.PhieuNhapKhos
            .Include(p => p.Kho)
            .Include(p => p.ChiTietNhapKhos)
                .ThenInclude(c => c.VatTuHangHoa)
            .FirstOrDefaultAsync(p => p.SoPhieu == "PNK-2026-0001");

        Assert.NotNull(saved);
        Assert.Equal(kho.Id, saved.KhoId);
        Assert.Single(saved.ChiTietNhapKhos);
        Assert.Equal(10m, saved.ChiTietNhapKhos.First().SoLuong);
        Assert.Equal(50_000_000m, saved.ChiTietNhapKhos.First().ThanhTien);
    }

    [Fact]
    public async Task PhieuXuatKho_TaoThanhCong_GanPhongBanPhanBoChiPhi()
    {
        using var context = new AppDbContext(_dbOptions);
        var chiNhanh = await context.ChiNhanhs.FirstAsync();
        var kho = await context.Khos.FirstAsync(k => k.MaKho == "KHO-TONG");
        var kh = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.KhachHang);
        var pb = await context.PhongBans.FirstAsync(p => p.LoaiPhongBan == LoaiPhongBan.BanHang);
        var vatTu = await context.VatTuHangHoas.FirstAsync(v => v.DangTheoDoiTonKho);
        var tk632 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "632");
        var tk156 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1561" || t.MaTaiKhoan == "156");

        var pxk = new PhieuXuatKho
        {
            ChiNhanhId = chiNhanh.Id,
            KhoId = kho.Id,
            SoPhieu = "PXK-2026-0001",
            NgayXuat = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LoaiXuatKho = LoaiXuatKho.BanHang,
            KhachHangId = kh.Id,
            PhongBanId = pb.Id,
            DienGiai = "Xuất kho bán hàng cho khách hàng",
            TongSoLuong = 3m,
            TongTienGiaVon = 15_000_000m,
            TrangThai = TrangThaiPhieuKho.TamTinh,
            ChiTietXuatKhos = new List<ChiTietXuatKho>
            {
                new()
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 3m,
                    DonGiaVon = 5_000_000m,
                    TienGiaVon = 15_000_000m,
                    TaiKhoanNoId = tk632.Id,
                    TaiKhoanCoId = tk156.Id
                }
            }
        };

        await context.PhieuXuatKhos.AddAsync(pxk);
        await context.SaveChangesAsync();

        var saved = await context.PhieuXuatKhos
            .Include(p => p.Kho)
            .Include(p => p.PhongBan)
            .Include(p => p.ChiTietXuatKhos)
            .FirstOrDefaultAsync(p => p.SoPhieu == "PXK-2026-0001");

        Assert.NotNull(saved);
        Assert.Equal(pb.Id, saved.PhongBanId);
        Assert.Single(saved.ChiTietXuatKhos);
        Assert.Equal(15_000_000m, saved.ChiTietXuatKhos.First().TienGiaVon);
    }
}
