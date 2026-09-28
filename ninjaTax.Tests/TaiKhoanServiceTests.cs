using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using ninjaTax.Models.ViewModels;
using Xunit;

namespace ninjaTax.Tests;

public class TaiKhoanServiceTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public TaiKhoanServiceTests()
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
    public async Task TaoMoiTaiKhoanCon_ValidPrefix_UpdatesParentToLaTaiKhoanSoCai()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new TaiKhoanService(context, NullLogger<TaiKhoanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "111");
        Assert.True(tk111.LaTaiKhoanSoCai);

        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        Assert.False(tk1111.LaTaiKhoanSoCai); // Trước khi có con, 1111 là tài khoản lá

        // Thêm tài khoản con 11111 trực thuộc 1111
        var model = new TaiKhoanCreateEditViewModel
        {
            MaTaiKhoan = "11111",
            TenTaiKhoan = "Tiền Việt Nam tại quỹ chính Hà Nội",
            TaiKhoanMeId = tk1111.Id,
            TinhChat = TinhChatTaiKhoan.DuNo,
            LoaiTaiKhoan = LoaiTaiKhoan.TaiSan,
            DangHoatDong = true
        };

        var (thanhCong, thongBao, newId) = await service.TaoMoiAsync(model);

        Assert.True(thanhCong);
        Assert.NotNull(newId);

        // Bất biến: Sau khi có con, 1111 phải tự động chuyển thành tài khoản tổng hợp (LaTaiKhoanSoCai = true)
        var tk1111Updated = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        Assert.True(tk1111Updated.LaTaiKhoanSoCai);

        // Tài khoản mới sinh có bậc = bậc mẹ + 1
        var tkNew = await context.TaiKhoans.FirstAsync(t => t.Id == newId.Value);
        Assert.Equal(3, tkNew.BacTaiKhoan);
        Assert.False(tkNew.LaTaiKhoanSoCai); // Tài khoản lá mới cho phép hạch toán
    }

    [Fact]
    public async Task TaoMoiTaiKhoanCon_InvalidPrefix_FailsWithErrorMessage()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new TaiKhoanService(context, NullLogger<TaiKhoanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "111");

        // Cố tình tạo mã con 1129 dưới mẹ 111 (sai tiền tố)
        var model = new TaiKhoanCreateEditViewModel
        {
            MaTaiKhoan = "1129",
            TenTaiKhoan = "Tiền giả mạo mã mẹ",
            TaiKhoanMeId = tk111.Id,
            TinhChat = TinhChatTaiKhoan.DuNo,
            LoaiTaiKhoan = LoaiTaiKhoan.TaiSan
        };

        var (thanhCong, thongBao, _) = await service.TaoMoiAsync(model);

        Assert.False(thanhCong);
        Assert.Contains("111", thongBao);
    }

    [Fact]
    public async Task XoaTaiKhoan_KhiDaPhatSinhButToan_ReturnsFailureAndPreservesData()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new TaiKhoanService(context, NullLogger<TaiKhoanService>.Instance);

        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk5111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");

        // Ghi nhận 1 bút toán giao dịch dùng TK 1111
        var bt = new ButToan
        {
            SoChungTu = "PKT-TEST-COA-01",
            TongTien = 1000000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt.ChiTietButToans.Add(new ChiTietButToan
        {
            DongSo = 1,
            TaiKhoanNoId = tk1111.Id,
            TaiKhoanCoId = tk5111.Id,
            SoTien = 1000000m
        });
        context.ButToans.Add(bt);
        await context.SaveChangesAsync();

        // Thử xóa TK 1111
        var (thanhCong, thongBao) = await service.XoaAsync(tk1111.Id);

        Assert.False(thanhCong);
        Assert.Contains("phát sinh", thongBao);

        // Xác nhận dữ liệu không bị xóa khỏi CSDL
        var tkCheck = await context.TaiKhoans.FindAsync(tk1111.Id);
        Assert.NotNull(tkCheck);
    }

    [Fact]
    public async Task CapNhatTaiKhoan_DoiMaKhiDaPhatSinh_ReturnsFailure()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new TaiKhoanService(context, NullLogger<TaiKhoanService>.Instance);

        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk4111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "4111");

        var bt = new ButToan
        {
            SoChungTu = "PKT-TEST-COA-02",
            TongTien = 2000000m,
            TrangThai = TrangThaiButToan.DaGhiSo
        };
        bt.ChiTietButToans.Add(new ChiTietButToan
        {
            DongSo = 1,
            TaiKhoanNoId = tk1111.Id,
            TaiKhoanCoId = tk4111.Id,
            SoTien = 2000000m
        });
        context.ButToans.Add(bt);
        await context.SaveChangesAsync();

        // Cố tình đổi số hiệu tài khoản 1111 thành 1119
        var updateModel = new TaiKhoanCreateEditViewModel
        {
            Id = tk1111.Id,
            MaTaiKhoan = "1119",
            TenTaiKhoan = "Đổi tên tài khoản",
            TaiKhoanMeId = tk1111.TaiKhoanMeId,
            TinhChat = tk1111.TinhChat,
            LoaiTaiKhoan = tk1111.LoaiTaiKhoan,
            DangHoatDong = true
        };

        var (thanhCong, thongBao) = await service.CapNhatAsync(tk1111.Id, updateModel);

        Assert.False(thanhCong);
        Assert.Contains("phát sinh", thongBao);
    }
}
