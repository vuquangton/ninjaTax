using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class CashPaymentSafetyTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public CashPaymentSafetyTests()
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
    public async Task ChotChan1_NegativeCash_ThrowsException_AndBlocksPosting()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var congNoService = new CongNoService(context, NullLogger<CongNoService>.Instance);
        var thuChiService = new ThuChiService(context, butToanService, congNoService, NullLogger<ThuChiService>.Instance);

        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk642 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "642");

        // Quỹ tiền mặt hiện tại = 0 VNĐ
        var tonBanDau = await thuChiService.TinhTonQuyKhaDungAsync(DateTime.Today);
        Assert.Equal(0m, tonBanDau);

        // Cố gắng lập và ghi sổ phiếu chi 5.000.000 VNĐ
        var pc = await thuChiService.TaoChungTuThuChiAsync(new ThuChiCreateViewModel
        {
            LoaiChungTu = LoaiChungTuThuChi.ChiTienMat,
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            LyDo = "Chi tiếp khách khi quỹ không đủ",
            ChiTiets = new()
            {
                new()
                {
                    DienGiai = "Tiền ăn trưa",
                    TaiKhoanNoId = tk642.Id,
                    TaiKhoanCoId = tk1111.Id,
                    SoTien = 5000000m
                }
            }
        });

        // Chốt chặn 1: Bẫy âm quỹ tiền mặt phải kích hoạt và chặn ghi sổ
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await thuChiService.GhiSoChungTuAsync(pc.Id);
        });

        Assert.Contains("Tồn quỹ tiền mặt không đủ để chi", ex.Message);

        // Trạng thái chứng từ vẫn phải là ChoGhiSo, không được ghi sổ
        var pcReload = await context.ChungTuThuChis.FindAsync(pc.Id);
        Assert.NotNull(pcReload);
        Assert.Equal(TrangThaiThuChi.ChoGhiSo, pcReload.TrangThai);
        Assert.Null(pcReload.ButToanId);
    }

    [Fact]
    public async Task ChotChan2_Bẫy20Trieu_FlagsViolationWhenPayingCashForInvoiceGte20M()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var congNoService = new CongNoService(context, NullLogger<CongNoService>.Instance);
        var thuChiService = new ThuChiService(context, butToanService, congNoService, NullLogger<ThuChiService>.Instance);

        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Tạo Hóa đơn mua hàng 25.000.000 VNĐ (>= 20M)
        var hd = new HoaDonMuaHang
        {
            SoChungTu = "MH-001",
            SoHoaDon = "0000123",
            NhaCungCapId = ncc.Id,
            NgayHoaDon = DateTime.Today,
            TongTienHang = 22727273m,
            TongTienThueVat = 2272727m,
            TongThanhToan = 25000000m,
            DaThanhToan = 0m
        };
        context.HoaDonMuaHangs.Add(hd);
        await context.SaveChangesAsync();

        // Lập phiếu chi tiền mặt cho hóa đơn 25M
        var pc = await thuChiService.TaoChungTuThuChiAsync(new ThuChiCreateViewModel
        {
            LoaiChungTu = LoaiChungTuThuChi.ChiTienMat,
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            DoiTuongId = ncc.Id,
            HoaDonMuaHangId = hd.Id,
            LyDo = "Chi tiền mặt trả tiền hàng hóa đơn 25 triệu",
            ChiTiets = new()
            {
                new()
                {
                    DienGiai = "Trả tiền người bán",
                    TaiKhoanNoId = tk331.Id,
                    TaiKhoanCoId = tk1111.Id,
                    SoTien = 25000000m,
                    DoiTuongId = ncc.Id
                }
            }
        });

        // Bẫy 20 triệu: Hệ thống bắt buộc đánh dấu ViPhamQuyTac20Tr = true
        Assert.True(pc.ViPhamQuyTac20Tr);
        Assert.StartsWith("PC-", pc.SoChungTu);
    }

    [Fact]
    public async Task UyNhiemChi_ValidPayment_DeductsBankBalance_AndSettlesPurchaseInvoice()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var congNoService = new CongNoService(context, NullLogger<CongNoService>.Instance);
        var thuChiService = new ThuChiService(context, butToanService, congNoService, NullLogger<ThuChiService>.Instance);

        var ncc = await context.DoiTuongs.FirstAsync(d => d.Loai == LoaiDoiTuong.NhaCungCap);
        var tk1121 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1121");
        var tk331 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "331");

        // Tạo tài khoản ngân hàng số dư ban đầu 100.000.000 VNĐ
        var tkNh = await thuChiService.TaoTaiKhoanNganHangAsync(new TaiKhoanNganHang
        {
            SoTaiKhoan = "190012345678",
            TenNganHang = "Techcombank",
            SoDuBanDau = 100000000m,
            DangHoatDong = true
        });

        // Tạo Hóa đơn mua hàng 40.000.000 VNĐ
        var hd = new HoaDonMuaHang
        {
            SoChungTu = "MH-002",
            SoHoaDon = "0000999",
            NhaCungCapId = ncc.Id,
            NgayHoaDon = DateTime.Today,
            TongTienHang = 36363636m,
            TongTienThueVat = 3636364m,
            TongThanhToan = 40000000m,
            DaThanhToan = 0m
        };
        context.HoaDonMuaHangs.Add(hd);
        await context.SaveChangesAsync();

        // Lập Ủy Nhiệm Chi thanh toán
        var unc = await thuChiService.TaoChungTuThuChiAsync(new ThuChiCreateViewModel
        {
            LoaiChungTu = LoaiChungTuThuChi.UyNhiemChi,
            NgayChungTu = DateTime.Today,
            NgayHachToan = DateTime.Today,
            TaiKhoanNganHangId = tkNh.Id,
            DoiTuongId = ncc.Id,
            HoaDonMuaHangId = hd.Id,
            LyDo = "Chuyển khoản thanh toán HĐ 0000999",
            ChiTiets = new()
            {
                new()
                {
                    DienGiai = "Chuyển khoản thanh toán tiền hàng",
                    TaiKhoanNoId = tk331.Id,
                    TaiKhoanCoId = tk1121.Id,
                    SoTien = 40000000m,
                    DoiTuongId = ncc.Id
                }
            }
        });

        Assert.False(unc.ViPhamQuyTac20Tr); // Chuyển khoản ngân hàng nên an toàn tuyệt đối
        Assert.StartsWith("UNC-", unc.SoChungTu);

        // Ghi sổ
        await thuChiService.GhiSoChungTuAsync(unc.Id);

        // Số dư ngân hàng còn lại: 100M - 40M = 60M
        var soDuSauChi = await thuChiService.TinhTonQuyKhaDungAsync(DateTime.Today, tkNh.Id);
        Assert.Equal(60000000m, soDuSauChi);

        // Hóa đơn mua hàng đã tất toán
        var hdSauChi = await context.HoaDonMuaHangs.FindAsync(hd.Id);
        Assert.NotNull(hdSauChi);
        Assert.Equal(40000000m, hdSauChi.DaThanhToan);
        Assert.Equal(0m, hdSauChi.ConPhaiTra);
    }
}
