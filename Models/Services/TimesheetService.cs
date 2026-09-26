using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class TimesheetService : ITimesheetService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TimesheetService> _logger;

    public TimesheetService(AppDbContext context, ILogger<TimesheetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BangChamCongViewModel> LayHoacTaoBangChamCongAsync(string kyKeToan, int soNgayCongChuan = 22)
    {
        var bang = await _context.BangChamCongThangs
            .Include(b => b.ChiTiets)
                .ThenInclude(c => c.NhanVien)
            .FirstOrDefaultAsync(b => b.KyKeToan == kyKeToan);

        if (bang == null)
        {
            var parts = kyKeToan.Split('-');
            int nam = int.Parse(parts[0]);
            int thang = int.Parse(parts[1]);

            bang = new BangChamCongThang
            {
                KyKeToan = kyKeToan,
                Nam = nam,
                Thang = thang,
                SoNgayCongChuan = soNgayCongChuan,
                TrangThai = TrangThaiChamCong.DangCham,
                NgayTao = DateTime.UtcNow
            };

            var nhanViens = await _context.NhanViens
                .Where(n => n.DangLamViec)
                .OrderBy(n => n.MaNhanVien)
                .ToListAsync();

            foreach (var nv in nhanViens)
            {
                bang.ChiTiets.Add(new ChiTietChamCong
                {
                    NhanVienId = nv.Id,
                    SoNgayDiLam = soNgayCongChuan,
                    SoNgayNghiPhep = 0,
                    SoNgayNghiLe = 0,
                    SoNgayNghiKhongLuong = 0,
                    SoNgayNghiOmBhxh = 0,
                    SoNgayNghiThaiSan = 0,
                    GioLamThemNgayThuong = 0,
                    GioLamThemNgayNghi = 0,
                    GioLamThemNgayLe = 0,
                    TongCongTinhLuong = soNgayCongChuan
                });
            }

            await _context.BangChamCongThangs.AddAsync(bang);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Khởi tạo bảng chấm công kỳ {KyKeToan} cho {Count} nhân viên", kyKeToan, nhanViens.Count);
        }

        return new BangChamCongViewModel
        {
            Id = bang.Id,
            KyKeToan = bang.KyKeToan,
            Nam = bang.Nam,
            Thang = bang.Thang,
            SoNgayCongChuan = bang.SoNgayCongChuan,
            TrangThai = bang.TrangThai,
            DongChamCongs = bang.ChiTiets.Select(c => new DongChamCongViewModel
            {
                NhanVienId = c.NhanVienId,
                MaNhanVien = c.NhanVien?.MaNhanVien ?? "",
                HoTen = c.NhanVien?.HoTen ?? "",
                PhongBan = c.NhanVien?.PhongBan,
                SoNgayDiLam = c.SoNgayDiLam,
                SoNgayNghiPhep = c.SoNgayNghiPhep,
                SoNgayNghiLe = c.SoNgayNghiLe,
                SoNgayNghiKhongLuong = c.SoNgayNghiKhongLuong,
                SoNgayNghiOmBhxh = c.SoNgayNghiOmBhxh,
                SoNgayNghiThaiSan = c.SoNgayNghiThaiSan,
                GioLamThemNgayThuong = c.GioLamThemNgayThuong,
                GioLamThemNgayNghi = c.GioLamThemNgayNghi,
                GioLamThemNgayLe = c.GioLamThemNgayLe,
                TongCongTinhLuong = c.TongCongTinhLuong
            }).ToList()
        };
    }

    public async Task CapNhatChiTietChamCongAsync(long bangChamCongId, List<DongChamCongViewModel> danhSach)
    {
        var bang = await _context.BangChamCongThangs
            .Include(b => b.ChiTiets)
            .FirstOrDefaultAsync(b => b.Id == bangChamCongId);

        if (bang == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bảng chấm công id {bangChamCongId}");
        }

        if (bang.TrangThai == TrangThaiChamCong.DaKhoa)
        {
            throw new InvalidOperationException("Bảng chấm công đã khóa, không thể chỉnh sửa.");
        }

        foreach (var dong in danhSach)
        {
            var ct = bang.ChiTiets.FirstOrDefault(c => c.NhanVienId == dong.NhanVienId);
            if (ct != null)
            {
                ct.SoNgayDiLam = dong.SoNgayDiLam;
                ct.SoNgayNghiPhep = dong.SoNgayNghiPhep;
                ct.SoNgayNghiLe = dong.SoNgayNghiLe;
                ct.SoNgayNghiKhongLuong = dong.SoNgayNghiKhongLuong;
                ct.SoNgayNghiOmBhxh = dong.SoNgayNghiOmBhxh;
                ct.SoNgayNghiThaiSan = dong.SoNgayNghiThaiSan;
                ct.GioLamThemNgayThuong = dong.GioLamThemNgayThuong;
                ct.GioLamThemNgayNghi = dong.GioLamThemNgayNghi;
                ct.GioLamThemNgayLe = dong.GioLamThemNgayLe;
                ct.TongCongTinhLuong = dong.SoNgayDiLam + dong.SoNgayNghiPhep + dong.SoNgayNghiLe;
            }
        }

        bang.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<BangChamCongThang> ChotBangChamCongAsync(long bangChamCongId)
    {
        var bang = await _context.BangChamCongThangs.FindAsync(bangChamCongId);
        if (bang == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bảng chấm công id {bangChamCongId}");
        }

        bang.TrangThai = TrangThaiChamCong.DaChot;
        bang.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return bang;
    }
}
