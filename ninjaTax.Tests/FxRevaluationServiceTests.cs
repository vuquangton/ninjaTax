using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class FxRevaluationServiceTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public FxRevaluationServiceTests()
    {
        _sqliteConnection = new SqliteConnection($"Data Source=mem_fx_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
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
    public async Task ThucHienDanhGiaLai_Succeeds_ClearsAccount413ToZero()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new FxRevaluationService(context, NullLogger<FxRevaluationService>.Instance);

        var asOf = new DateTime(2026, 12, 31);
        var tk1122 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1122");
        var tk1111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk4131 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "4131");

        // Giả lập số dư trên TK 1122 (Tiền gửi ngoại tệ) là 250.000.000 đ (tương đương 10.000 USD với tỷ giá 25.000)
        var btTien = new ButToan
        {
            SoChungTu = "PKT-USD-DEPOSIT",
            NgayHachToan = asOf.AddDays(-10),
            TongTien = 250000000m,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new()
                {
                    DongSo = 1,
                    TaiKhoanNoId = tk1122.Id,
                    TaiKhoanCoId = tk1111.Id,
                    SoTien = 250000000m,
                    DienGiai = "Gửi tiền USD vào ngân hàng"
                }
            }
        };
        context.ButToans.Add(btTien);
        await context.SaveChangesAsync();

        // Đánh giá lại với Tỷ giá mua = 25.500 (Tăng 500 VND/USD => Lãi 5.000.000 VND)
        var result = await service.ThucHienDanhGiaLaiCuoiKyAsync(asOf, "USD", 25500m, 25800m, "Đánh giá lại cuối năm 2026");

        Assert.NotNull(result);
        Assert.True(result.DaGhiSo);
        Assert.NotNull(result.ButToanDanhGiaLaiId);
        Assert.NotNull(result.ButToanKetChuyen413Id);

        // Kiểm tra Bất biến VAS 10: Số dư TK 4131 bắt buộc bằng 0 (DuNo == 0 && DuCo == 0)
        var butToans413 = await context.ChiTietButToans
            .Where(c => c.TaiKhoanNoId == tk4131.Id || c.TaiKhoanCoId == tk4131.Id)
            .ToListAsync();

        var tongPhatSinhNo413 = butToans413.Where(c => c.TaiKhoanNoId == tk4131.Id).Sum(c => c.SoTien);
        var tongPhatSinhCo413 = butToans413.Where(c => c.TaiKhoanCoId == tk4131.Id).Sum(c => c.SoTien);

        Assert.True(tongPhatSinhNo413 > 0, "Phải có phát sinh Nợ trên TK 4131 từ bút toán kết chuyển");
        Assert.True(tongPhatSinhCo413 > 0, "Phải có phát sinh Có trên TK 4131 từ bút toán đánh giá lại");
        Assert.Equal(tongPhatSinhNo413, tongPhatSinhCo413); // Số dư thuần bằng 0
    }

    [Fact]
    public async Task ThucHienDanhGiaLai_RespectsAccountingLockDate()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new FxRevaluationService(context, NullLogger<FxRevaluationService>.Instance);

        var asOf = new DateTime(2026, 12, 31);

        // Khóa sổ đến ngày 31/12/2026
        var cauHinh = await context.CauHinhKeToans.FirstAsync();
        cauHinh.NgayKhoaSo = asOf;
        await context.SaveChangesAsync();

        // Thực hiện đánh giá lại vào ngày bị khóa sổ phải ném ngoại lệ
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.ThucHienDanhGiaLaiCuoiKyAsync(asOf, "USD", 25500m, 25800m);
        });
    }
}
