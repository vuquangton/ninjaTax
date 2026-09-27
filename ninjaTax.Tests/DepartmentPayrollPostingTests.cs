using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class DepartmentPayrollPostingTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public DepartmentPayrollPostingTests()
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
    public async Task GhiSoBangLuong_PhanBoTheoPhongBan_SinhChiTietButToanDungTaiKhoanVaDungPhongBanId()
    {
        using var context = new AppDbContext(_dbOptions);
        var butToanService = new ButToanService(context, NullLogger<ButToanService>.Instance);
        var timesheetService = new TimesheetService(context, NullLogger<TimesheetService>.Instance);
        var payrollService = new PayrollService(context, butToanService, timesheetService, NullLogger<PayrollService>.Instance);

        var chiNhanh = await context.ChiNhanhs.FirstAsync();

        // 1. Tạo 3 phòng ban với 3 loại chi phí khác nhau
        var pbQuanLy = new PhongBan
        {
            ChiNhanhId = chiNhanh.Id,
            MaPhongBan = "PB-TEST-QL",
            TenPhongBan = "Phòng Quản Lý",
            LoaiPhongBan = LoaiPhongBan.QuanLy, // TK 6422
            DangHoatDong = true
        };
        var pbBanHang = new PhongBan
        {
            ChiNhanhId = chiNhanh.Id,
            MaPhongBan = "PB-TEST-BH",
            TenPhongBan = "Phòng Bán Hàng",
            LoaiPhongBan = LoaiPhongBan.BanHang, // TK 6421
            DangHoatDong = true
        };
        var pbSanXuat = new PhongBan
        {
            ChiNhanhId = chiNhanh.Id,
            MaPhongBan = "PB-TEST-SX",
            TenPhongBan = "Phân Xưởng Sản Xuất",
            LoaiPhongBan = LoaiPhongBan.SanXuat, // TK 154
            DangHoatDong = true
        };

        await context.PhongBans.AddRangeAsync(pbQuanLy, pbBanHang, pbSanXuat);
        await context.SaveChangesAsync();

        // 2. Tạo nhân viên cho từng phòng ban + 1 nhân viên chưa gán phòng ban (fallback)
        var nv1 = new NhanVien
        {
            MaNhanVien = "NV_QL",
            HoTen = "Quản Lý A",
            PhongBanId = pbQuanLy.Id,
            ChucVu = "Trưởng phòng",
            LoaiHopDong = LoaiHopDongLaoDong.HopDongDaiHan,
            LuongCoBan = 20_000_000m,
            LuongDongBaoHiem = 20_000_000m,
            DongBaoHiem = true,
            DangLamViec = true
        };

        var nv2 = new NhanVien
        {
            MaNhanVien = "NV_BH",
            HoTen = "Kinh Doanh B",
            PhongBanId = pbBanHang.Id,
            ChucVu = "Nhân viên Sale",
            LoaiHopDong = LoaiHopDongLaoDong.HopDongDaiHan,
            LuongCoBan = 15_000_000m,
            LuongDongBaoHiem = 15_000_000m,
            DongBaoHiem = true,
            DangLamViec = true
        };

        var nv3 = new NhanVien
        {
            MaNhanVien = "NV_SX",
            HoTen = "Công Nhân C",
            PhongBanId = pbSanXuat.Id,
            ChucVu = "Thợ kỹ thuật",
            LoaiHopDong = LoaiHopDongLaoDong.HopDongDaiHan,
            LuongCoBan = 10_000_000m,
            LuongDongBaoHiem = 10_000_000m,
            DongBaoHiem = true,
            DangLamViec = true
        };

        var nv4 = new NhanVien
        {
            MaNhanVien = "NV_FALLBACK",
            HoTen = "Nhân Sự Mới D",
            PhongBanId = null, // Chưa gán phòng ban -> fallback sang 6422
            ChucVu = "Học việc",
            LoaiHopDong = LoaiHopDongLaoDong.HopDongDaiHan,
            LuongCoBan = 5_000_000m,
            LuongDongBaoHiem = 5_000_000m,
            DongBaoHiem = true,
            DangLamViec = true
        };

        await context.NhanViens.AddRangeAsync(nv1, nv2, nv3, nv4);
        await context.SaveChangesAsync();

        var kyKeToan = "2026-10";

        // 3. Chấm công 22 ngày công chuẩn
        var bcc = await timesheetService.LayHoacTaoBangChamCongAsync(kyKeToan, 22);
        foreach (var dong in bcc.DongChamCongs)
        {
            dong.SoNgayDiLam = 22m;
        }
        await timesheetService.CapNhatChiTietChamCongAsync(bcc.Id, bcc.DongChamCongs);

        // 4. Tính lương tháng
        var bangLuongVm = await payrollService.TinhLuongThangAsync(kyKeToan);
        Assert.NotNull(bangLuongVm);
        Assert.Equal(4, bangLuongVm.ChiTiets.Count);
        Assert.Equal(50_000_000m, bangLuongVm.TongQuyLuong);

        // 5. Ghi sổ bảng lương
        var blDaGhiSo = await payrollService.GhiSoBangLuongAsync(bangLuongVm.BangLuongId);
        Assert.Equal(TrangThaiBangLuong.DaGhiSo, blDaGhiSo.TrangThai);
        Assert.NotNull(blDaGhiSo.ButToanChiPhiLuongId);
        Assert.NotNull(blDaGhiSo.ButToanBaoHiemDnId);

        // 6. Kiểm tra Bút toán 1: Chi phí tiền lương được tách thành các dòng phân bổ đúng phòng ban
        var btLuong = await context.ButToans
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanNo)
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanCo)
            .FirstAsync(b => b.Id == blDaGhiSo.ButToanChiPhiLuongId!.Value);

        Assert.Equal(TrangThaiButToan.DaGhiSo, btLuong.TrangThai);
        Assert.Equal(50_000_000m, btLuong.TongTien);

        // Kiểm tra dòng Nợ 154 cho Phân xưởng sản xuất
        var lineSx = btLuong.ChiTietButToans.FirstOrDefault(c => c.PhongBanId == pbSanXuat.Id);
        Assert.NotNull(lineSx);
        Assert.Equal("154", lineSx.TaiKhoanNo?.MaTaiKhoan);
        Assert.Equal("334", lineSx.TaiKhoanCo?.MaTaiKhoan);
        Assert.Equal(10_000_000m, lineSx.SoTien);

        // Kiểm tra dòng Nợ 6421 cho Phòng bán hàng
        var lineBh = btLuong.ChiTietButToans.FirstOrDefault(c => c.PhongBanId == pbBanHang.Id);
        Assert.NotNull(lineBh);
        Assert.Equal("6421", lineBh.TaiKhoanNo?.MaTaiKhoan);
        Assert.Equal("334", lineBh.TaiKhoanCo?.MaTaiKhoan);
        Assert.Equal(15_000_000m, lineBh.SoTien);

        // Kiểm tra dòng Nợ 6422 cho Phòng quản lý
        var lineQl = btLuong.ChiTietButToans.FirstOrDefault(c => c.PhongBanId == pbQuanLy.Id);
        Assert.NotNull(lineQl);
        Assert.Equal("6422", lineQl.TaiKhoanNo?.MaTaiKhoan);
        Assert.Equal("334", lineQl.TaiKhoanCo?.MaTaiKhoan);
        Assert.Equal(20_000_000m, lineQl.SoTien);

        // Kiểm tra dòng Fallback (nhân viên không có phòng ban gán sang 6422)
        var lineFallback = btLuong.ChiTietButToans.FirstOrDefault(c => c.PhongBanId == null);
        Assert.NotNull(lineFallback);
        Assert.Equal("6422", lineFallback.TaiKhoanNo?.MaTaiKhoan);
        Assert.Equal(5_000_000m, lineFallback.SoTien);

        // Tổng chi tiết bút toán lương phải đúng bằng 50.000.000đ
        Assert.Equal(50_000_000m, btLuong.ChiTietButToans.Sum(c => c.SoTien));

        // 7. Kiểm tra Bút toán 2: Bảo hiểm DN gánh được phân bổ theo phòng ban
        var btBhdn = await context.ButToans
            .Include(b => b.ChiTietButToans)
                .ThenInclude(c => c.TaiKhoanNo)
            .FirstAsync(b => b.Id == blDaGhiSo.ButToanBaoHiemDnId!.Value);

        Assert.Equal(TrangThaiButToan.DaGhiSo, btBhdn.TrangThai);
        // Các chi tiết của Phân xưởng sản xuất phải ghi Nợ 154
        var linesSxBh = btBhdn.ChiTietButToans.Where(c => c.PhongBanId == pbSanXuat.Id).ToList();
        Assert.NotEmpty(linesSxBh);
        Assert.All(linesSxBh, l => Assert.Equal("154", l.TaiKhoanNo?.MaTaiKhoan));

        // Các chi tiết của Phòng Bán Hàng phải ghi Nợ 6421
        var linesBhBh = btBhdn.ChiTietButToans.Where(c => c.PhongBanId == pbBanHang.Id).ToList();
        Assert.NotEmpty(linesBhBh);
        Assert.All(linesBhBh, l => Assert.Equal("6421", l.TaiKhoanNo?.MaTaiKhoan));

        // Tuyệt đối không dùng TK 911 ở bất kỳ đâu
        Assert.DoesNotContain(btLuong.ChiTietButToans, c => c.TaiKhoanNo?.MaTaiKhoan == "911" || c.TaiKhoanCo?.MaTaiKhoan == "911");
        Assert.DoesNotContain(btBhdn.ChiTietButToans, c => c.TaiKhoanNo?.MaTaiKhoan == "911" || c.TaiKhoanCo?.MaTaiKhoan == "911");
    }
}
