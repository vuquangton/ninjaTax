using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests.Services;

public class InvoiceServiceTests
{
    private readonly AppDbContext _context;
    private readonly IInvoiceService _service;

    public InvoiceServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        DbInitializer.Initialize(_context);
        _service = new InvoiceService(_context);
    }

    [Fact]
    public async Task CreateInvoice_CreatesInvoiceAndPostsJournal()
    {
        // Arrange
        var khachHang = new DoiTuong { TenDoiTuong = "Customer A", Loai = LoaiDoiTuong.KhachHang };
        _context.DoiTuongs.Add(khachHang);
        await _context.SaveChangesAsync();

        var vatTu = new VatTuHangHoa { TenVatTu = "Product X", MaVatTu = "PX001" };
        _context.VatTuHangHoas.Add(vatTu);
        await _context.SaveChangesAsync();

        var tk131 = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "131");
        var tk511 = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");
        var tk33311 = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "33311");

        var invoice = new HoaDonBanHang
        {
            SoChungTu = "BH-2026-00001",
            KhachHangId = khachHang.Id,
            BanHangKiemXuatKho = false,
            ChiTietBans = new List<ChiTietHoaDonBan>
            {
                new ChiTietHoaDonBan
                {
                    VatTuHangHoaId = vatTu.Id,
                    SoLuong = 2,
                    DonGia = 100m,
                    ThanhTien = 200m,
                    TiLeChietKhau = 0m,
                    TienChietKhau = 0m,
                    ThueSuatVat = 10m,
                    TienThueVat = 20m,
                    TaiKhoanNoId = tk131.Id,
                    TaiKhoanDoanhThuId = tk511.Id,
                    TaiKhoanThueId = tk33311.Id
                }
            }
        };

        // Act
        var result = await _service.CreateInvoiceAsync(invoice);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, await _context.HoaDonBanHangs.CountAsync());
        var journal = await _context.ButToans.FirstOrDefaultAsync(b => b.Id == result.ButToanDoanhThuId);
        Assert.NotNull(journal);
        Assert.Equal(journal.TongNo, journal.TongCo);
    }
}
