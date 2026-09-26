using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class PayrollPostingGlTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public PayrollPostingGlTests()
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
    public async Task GhiSoBangLuong_Sinh3ButToanCanDoi_KhongDungTK911()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var timesheetService = new TimesheetService(context, NullLogger<TimesheetService>.Instance);
        var payrollService = new PayrollService(context, butToanService, timesheetService, NullLogger<PayrollService>.Instance);

        // 1. Thêm nhân viên mẫu
        var nv1 = new NhanVien
        {
            MaNhanVien = "NV01",
            HoTen = "Trần Văn An",
            PhongBan = "Kỹ thuật",
            ChucVu = "Kỹ sư",
            LoaiHopDong = LoaiHopDongLaoDong.HopDongDaiHan,
            LuongCoBan = 22_000_000m,
            LuongDongBaoHiem = 20_000_000m,
            PhuCapAnTrua = 730_000m,
            DongBaoHiem = true,
            DangLamViec = true
        };
        var nv2 = new NhanVien
        {
            MaNhanVien = "NV02",
            HoTen = "Lê Thị Bình",
            PhongBan = "Kinh doanh",
            ChucVu = "Cộng tác viên",
            LoaiHopDong = LoaiHopDongLaoDong.ThuViecThoiVu,
            LuongCoBan = 6_000_000m,
            LuongDongBaoHiem = 0m,
            DongBaoHiem = false,
            CoCamKet08 = false, // Phải chịu thuế 10% = 600.000 VNĐ
            DangLamViec = true
        };

        await context.NhanViens.AddRangeAsync(nv1, nv2);
        await context.SaveChangesAsync();

        var kyKeToan = "2026-09";

        // 2. Lập bảng chấm công đủ 22 ngày công chuẩn
        var bcc = await timesheetService.LayHoacTaoBangChamCongAsync(kyKeToan, 22);
        foreach (var dong in bcc.DongChamCongs)
        {
            dong.SoNgayDiLam = 22m;
        }
        await timesheetService.CapNhatChiTietChamCongAsync(bcc.Id, bcc.DongChamCongs);

        // 3. Tính bảng lương tháng
        var bangLuongVm = await payrollService.TinhLuongThangAsync(kyKeToan);
        Assert.NotNull(bangLuongVm);
        Assert.Equal(2, bangLuongVm.ChiTiets.Count);
        Assert.True(bangLuongVm.TongQuyLuong > 0);

        // 4. Ghi sổ bảng lương vào Sổ Cái TT99
        var blDaGhiSo = await payrollService.GhiSoBangLuongAsync(bangLuongVm.BangLuongId);

        Assert.Equal(TrangThaiBangLuong.DaGhiSo, blDaGhiSo.TrangThai);
        Assert.NotNull(blDaGhiSo.ButToanChiPhiLuongId);
        Assert.NotNull(blDaGhiSo.ButToanBaoHiemDnId);
        Assert.NotNull(blDaGhiSo.ButToanKhauTruLuongId);

        // 5. Kiểm tra Sổ Nhật Ký Chung: 3 Bút toán sinh ra phải tuyệt đối cân đối Nợ == Có và KHÔNG DÙNG TK 911
        var btLuong = await butToanService.LayTheoIdAsync(blDaGhiSo.ButToanChiPhiLuongId!.Value);
        Assert.NotNull(btLuong);
        Assert.Equal(TrangThaiButToan.DaGhiSo, btLuong.TrangThai);
        var tongNo1 = btLuong.ChiTietButToans.Sum(c => c.SoTien);
        var tongCo1 = btLuong.ChiTietButToans.Sum(c => c.SoTien);
        Assert.Equal(tongNo1, tongCo1);
        Assert.DoesNotContain(btLuong.ChiTietButToans, c => c.TaiKhoanNo?.MaTaiKhoan == "911" || c.TaiKhoanCo?.MaTaiKhoan == "911");

        var btBhDn = await butToanService.LayTheoIdAsync(blDaGhiSo.ButToanBaoHiemDnId!.Value);
        Assert.NotNull(btBhDn);
        Assert.Equal(TrangThaiButToan.DaGhiSo, btBhDn.TrangThai);
        Assert.DoesNotContain(btBhDn.ChiTietButToans, c => c.TaiKhoanNo?.MaTaiKhoan == "911" || c.TaiKhoanCo?.MaTaiKhoan == "911");

        var btKhauTru = await butToanService.LayTheoIdAsync(blDaGhiSo.ButToanKhauTruLuongId!.Value);
        Assert.NotNull(btKhauTru);
        Assert.Equal(TrangThaiButToan.DaGhiSo, btKhauTru.TrangThai);
        Assert.DoesNotContain(btKhauTru.ChiTietButToans, c => c.TaiKhoanNo?.MaTaiKhoan == "911" || c.TaiKhoanCo?.MaTaiKhoan == "911");

        // 6. Kiểm tra chốt chặn chống ghi sổ 2 lần
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await payrollService.GhiSoBangLuongAsync(blDaGhiSo.Id);
        });

        // 7. Kiểm tra lấy phiếu lương nhân viên
        var payslip = await payrollService.LayPhieuLuongNhanVienAsync(blDaGhiSo.Id, nv1.Id);
        Assert.NotNull(payslip);
        Assert.Equal("NV01", payslip.MaNhanVien);
        Assert.Equal(22m, payslip.SoNgayCongThucTe);
        Assert.True(payslip.ThucLinh > 0);
    }
}
