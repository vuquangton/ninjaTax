using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class AccountingBookLockingTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public AccountingBookLockingTests()
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
    public async Task ValidateCanPostTransaction_NgayHachToanTruocNgayKhoaSo_NemInvalidOperationException()
    {
        using var context = new AppDbContext(_dbOptions);
        var companyService = new CompanyService(context, NullLogger<CompanyService>.Instance);

        // Khóa sổ đến ngày 31/12/2025
        await companyService.LockBookToDateAsync(new DateTime(2025, 12, 31));

        // Thử hạch toán ngày 15/10/2025 (nằm trong kỳ đã khóa sổ)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            companyService.ValidateCanPostTransactionAsync(new DateTime(2025, 10, 15)));

        Assert.Contains("đã khóa sổ", ex.Message);
    }

    [Fact]
    public async Task ValidateCanPostTransaction_NgayHachToanSauNgayKhoaSo_TraVeTrue()
    {
        using var context = new AppDbContext(_dbOptions);
        var companyService = new CompanyService(context, NullLogger<CompanyService>.Instance);

        // Khóa sổ đến ngày 31/12/2025
        await companyService.LockBookToDateAsync(new DateTime(2025, 12, 31));

        // Hạch toán ngày 05/01/2026 (sau ngày khóa sổ)
        var allowed = await companyService.ValidateCanPostTransactionAsync(new DateTime(2026, 1, 5));

        Assert.True(allowed);
    }

    [Fact]
    public async Task UnlockBook_MoKhoaSổ_ChoPhepGhiSoMoiNgay()
    {
        using var context = new AppDbContext(_dbOptions);
        var companyService = new CompanyService(context, NullLogger<CompanyService>.Instance);

        // Khóa sổ rồi mở khóa
        await companyService.LockBookToDateAsync(new DateTime(2025, 12, 31));
        await companyService.UnlockBookAsync();

        // Bây giờ chứng từ 2025 được phép ghi sổ bình thường
        var allowed = await companyService.ValidateCanPostTransactionAsync(new DateTime(2025, 10, 15));

        Assert.True(allowed);
    }

    [Fact]
    public async Task DeleteBranch_TruSoChinh_NemInvalidOperationException()
    {
        using var context = new AppDbContext(_dbOptions);
        var companyService = new CompanyService(context, NullLogger<CompanyService>.Instance);

        // Lấy danh sách chi nhánh (chứa Trụ sở chính)
        var branches = await companyService.GetBranchesAsync();
        var ho = branches.First(b => b.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh);

        // Cố gắng xóa Trụ sở chính
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            companyService.DeleteBranchAsync(ho.Id));

        Assert.Contains("Trụ sở chính", ex.Message);
    }

    [Fact]
    public async Task ButToanService_GhiSo_KhiDaKhoaSổ_TraVeFalse()
    {
        using var context = new AppDbContext(_dbOptions);
        var companyService = new CompanyService(context, NullLogger<CompanyService>.Instance);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);

        var tk111 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "1111");
        var tk511 = await context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "5111");

        // Khóa sổ đến ngày 31/12/2025
        await companyService.LockBookToDateAsync(new DateTime(2025, 12, 31));

        // Bút toán ngày 15/10/2025
        var bt = new ButToan
        {
            SoChungTu = "PKT-TEST-LOCK",
            NgayChungTu = new DateTime(2025, 10, 15),
            NgayHachToan = new DateTime(2025, 10, 15),
            DienGiai = "Bút toán kỳ đã khóa sổ",
            TongTien = 10_000_000m,
            TrangThai = TrangThaiButToan.ChuaGhiSo
        };
        bt.ChiTietButToans.Add(new ChiTietButToan { DongSo = 1, TaiKhoanNoId = tk111.Id, TaiKhoanCoId = tk511.Id, SoTien = 10_000_000m });
        context.ButToans.Add(bt);
        await context.SaveChangesAsync();

        var (thanhCong, thongBao) = await butToanService.GhiSoAsync(bt.Id);

        Assert.False(thanhCong);
        Assert.Contains("khóa sổ", thongBao);
    }
}
