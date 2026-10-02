using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ tính toán và trích lập dự phòng nợ phải thu khó đòi theo Thông tư 48/2019/TT-BTC
/// </summary>
public class BadDebtProvisionService : IBadDebtProvisionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BadDebtProvisionService> _logger;

    public BadDebtProvisionService(AppDbContext context, ILogger<BadDebtProvisionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ChiTietTrichLapDuPhongViewModel>> LayDanhSachNoQuaHanTt48Async(DateTime mocThoiGian)
    {
        var moc = mocThoiGian.Date;
        var hoaDons = await _context.HoaDonBanHangs
            .Include(h => h.KhachHang)
            .Where(h => (h.TongThanhToan - h.DaThuTien) > 0 && h.TrangThai != TrangThaiHddt.DaHuy)
            .ToListAsync();

        var danhSach = new List<ChiTietTrichLapDuPhongViewModel>();

        foreach (var hd in hoaDons)
        {
            var conNo = hd.ConPhaiThu;
            var soNgayQuaHan = (int)(moc - hd.HanThanhToan.Date).TotalDays;

            // Theo TT 48/2019/TT-BTC: Quá hạn từ 6 tháng (180 ngày) trở lên mới được trích lập
            if (soNgayQuaHan < 180) continue;

            decimal tyLe = 0m;
            if (soNgayQuaHan >= 1095) // >= 3 năm
            {
                tyLe = 100m;
            }
            else if (soNgayQuaHan >= 730) // >= 2 năm (< 3 năm)
            {
                tyLe = 70m;
            }
            else if (soNgayQuaHan >= 365) // >= 1 năm (< 2 năm)
            {
                tyLe = 50m;
            }
            else // 180 <= soNgayQuaHan < 365 (từ 6 tháng đến dưới 1 năm)
            {
                tyLe = 30m;
            }

            var soTienDuPhong = Math.Round(conNo * tyLe / 100m, 4);

            danhSach.Add(new ChiTietTrichLapDuPhongViewModel
            {
                KhachHangId = hd.KhachHangId,
                MaKhachHang = hd.KhachHang?.MaDoiTuong ?? "KH-UNKNOWN",
                TenKhachHang = hd.KhachHang?.TenDoiTuong ?? "Khách hàng",
                HoaDonBanHangId = hd.Id,
                SoHoaDon = !string.IsNullOrWhiteSpace(hd.SoHoaDon) ? hd.SoHoaDon : hd.SoChungTu,
                NgayHoaDon = hd.NgayHoaDon,
                HanThanhToan = hd.HanThanhToan,
                SoTienConNo = conNo,
                SoNgayQuaHan = soNgayQuaHan,
                TyLeTrichLap = tyLe,
                SoTienDuPhong = soTienDuPhong
            });
        }

        return danhSach.OrderByDescending(x => x.SoNgayQuaHan).ToList();
    }

    public async Task<BangTrichLapDuPhongNoPhaiThu> TaoBangTrichLapDuPhongAsync(DateTime ngayHachToan, string? ghiChu = null)
    {
        var chiNhanh = await _context.ChiNhanhs.FirstOrDefaultAsync(c => c.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh)
                       ?? await _context.ChiNhanhs.FirstOrDefaultAsync();
        if (chiNhanh == null)
        {
            throw new InvalidOperationException("Hệ thống chưa có chi nhánh trụ sở chính.");
        }

        var danhSachQuaHan = await LayDanhSachNoQuaHanTt48Async(ngayHachToan);

        var tongNoQuaHan = danhSachQuaHan.Sum(x => x.SoTienConNo);
        var tongDuPhongPhaiTrich = danhSachQuaHan.Sum(x => x.SoTienDuPhong);

        // Lấy số dư Có hiện tại của TK 2293 trước thời điểm hạch toán
        var tk2293 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "2293");
        decimal soDuHienTai2293 = 0m;
        if (tk2293 != null)
        {
            var butToans = await _context.ChiTietButToans
                .Where(c => c.ButToan!.NgayHachToan <= ngayHachToan.Date && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo)
                .Where(c => c.TaiKhoanNoId == tk2293.Id || c.TaiKhoanCoId == tk2293.Id)
                .Select(c => new { c.TaiKhoanNoId, c.TaiKhoanCoId, c.SoTien })
                .ToListAsync();

            var tongCo = butToans.Where(b => b.TaiKhoanCoId == tk2293.Id).Sum(b => b.SoTien);
            var tongNo = butToans.Where(b => b.TaiKhoanNoId == tk2293.Id).Sum(b => b.SoTien);
            soDuHienTai2293 = Math.Max(0m, tongCo - tongNo);
        }

        decimal soTienTrichThem = 0m;
        decimal soTienHoanNhap = 0m;

        if (tongDuPhongPhaiTrich > soDuHienTai2293)
        {
            soTienTrichThem = tongDuPhongPhaiTrich - soDuHienTai2293;
        }
        else if (tongDuPhongPhaiTrich < soDuHienTai2293)
        {
            soTienHoanNhap = soDuHienTai2293 - tongDuPhongPhaiTrich;
        }

        var soChungTu = $"DPNT-{ngayHachToan:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        var bangTrichLap = new BangTrichLapDuPhongNoPhaiThu
        {
            ChiNhanhId = chiNhanh.Id,
            SoChungTu = soChungTu,
            NgayLap = DateTime.Today,
            NgayHachToan = ngayHachToan.Date,
            TongNoQuaHan = tongNoQuaHan,
            TongSoDuPhongPhaiTrich = tongDuPhongPhaiTrich,
            SoDuDuPhongHienTai2293 = soDuHienTai2293,
            SoTienTrichThem = soTienTrichThem,
            SoTienHoanNhap = soTienHoanNhap,
            TrangThai = TrangThaiBangTrichLap.TamTinh,
            GhiChu = ghiChu ?? $"Trích lập dự phòng nợ phải thu khó đòi theo TT48 kỳ {ngayHachToan:yyyy-MM-dd}",
            ChiTietTrichLaps = danhSachQuaHan.Select(d => new ChiTietTrichLapDuPhong
            {
                KhachHangId = d.KhachHangId,
                HoaDonBanHangId = d.HoaDonBanHangId,
                SoHoaDon = d.SoHoaDon,
                NgayHoaDon = d.NgayHoaDon,
                HanThanhToan = d.HanThanhToan,
                SoTienConNo = d.SoTienConNo,
                SoNgayQuaHan = d.SoNgayQuaHan,
                TyLeTrichLap = d.TyLeTrichLap,
                SoTienDuPhong = d.SoTienDuPhong,
                LyDoDacBiet = d.LyDoDacBiet
            }).ToList()
        };

        await _context.BangTrichLapDuPhongNoPhaiThus.AddAsync(bangTrichLap);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã tạo Bảng trích lập dự phòng nợ phải thu {SoChungTu}. Tổng dự phòng cần trích: {Tong:N0}", soChungTu, tongDuPhongPhaiTrich);
        return bangTrichLap;
    }

    public async Task<(bool ThanhCong, string? ThongBao)> GhiSoBangTrichLapAsync(long bangTrichLapId)
    {
        var bang = await _context.BangTrichLapDuPhongNoPhaiThus
            .Include(b => b.ChiTietTrichLaps)
            .FirstOrDefaultAsync(b => b.Id == bangTrichLapId);

        if (bang == null)
        {
            return (false, "Không tìm thấy bảng trích lập dự phòng.");
        }

        if (bang.TrangThai == TrangThaiBangTrichLap.DaGhiSo)
        {
            return (false, "Bảng trích lập đã được ghi sổ trước đó.");
        }

        // Kiểm tra khóa sổ
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh?.NgayKhoaSo.HasValue == true && bang.NgayHachToan <= cauHinh.NgayKhoaSo.Value.Date)
        {
            return (false, $"Kỳ kế toán đã khóa sổ đến ngày {cauHinh.NgayKhoaSo:dd/MM/yyyy}. Không thể ghi sổ.");
        }

        var tk6426 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "6426")
                     ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "6422")
                     ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "642");

        var tk2293 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "2293");

        if (tk6426 == null || tk2293 == null)
        {
            return (false, "Hệ thống thiếu tài khoản 6426 hoặc 2293 trong danh mục.");
        }

        // Hạch toán:
        // Trích lập bổ sung: Nợ 6426 / Có 2293 (SoTienTrichThem)
        // Hoàn nhập dự phòng: Nợ 2293 / Có 6426 (SoTienHoanNhap)
        if (bang.SoTienTrichThem > 0)
        {
            var butToan = new ButToan
            {
                SoChungTu = $"PKT-{bang.SoChungTu}",
                NgayHachToan = bang.NgayHachToan,
                NgayChungTu = bang.NgayLap,
                SoChungTuGoc = bang.SoChungTu,
                NgayChungTuGoc = bang.NgayLap,
                DienGiai = $"Trích lập bổ sung dự phòng nợ phải thu khó đòi TT48 theo {bang.SoChungTu}",
                TongTien = bang.SoTienTrichThem,
                TrangThai = TrangThaiButToan.DaGhiSo,
                ChiTietButToans = new List<ChiTietButToan>
                {
                    new()
                    {
                        DongSo = 1,
                        TaiKhoanNoId = tk6426.Id,
                        TaiKhoanCoId = tk2293.Id,
                        SoTien = bang.SoTienTrichThem,
                        DienGiai = "Trích lập bổ sung dự phòng nợ phải thu khó đòi"
                    }
                }
            };
            await _context.ButToans.AddAsync(butToan);
            await _context.SaveChangesAsync();
            bang.ButToanId = butToan.Id;
        }
        else if (bang.SoTienHoanNhap > 0)
        {
            var butToan = new ButToan
            {
                SoChungTu = $"PKT-{bang.SoChungTu}",
                NgayHachToan = bang.NgayHachToan,
                NgayChungTu = bang.NgayLap,
                SoChungTuGoc = bang.SoChungTu,
                NgayChungTuGoc = bang.NgayLap,
                DienGiai = $"Hoàn nhập dự phòng nợ phải thu khó đòi TT48 theo {bang.SoChungTu}",
                TongTien = bang.SoTienHoanNhap,
                TrangThai = TrangThaiButToan.DaGhiSo,
                ChiTietButToans = new List<ChiTietButToan>
                {
                    new()
                    {
                        DongSo = 1,
                        TaiKhoanNoId = tk2293.Id,
                        TaiKhoanCoId = tk6426.Id,
                        SoTien = bang.SoTienHoanNhap,
                        DienGiai = "Hoàn nhập dự phòng nợ phải thu khó đòi"
                    }
                }
            };
            await _context.ButToans.AddAsync(butToan);
            await _context.SaveChangesAsync();
            bang.ButToanId = butToan.Id;
        }

        bang.TrangThai = TrangThaiBangTrichLap.DaGhiSo;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã ghi sổ Bảng trích lập dự phòng {SoChungTu}", bang.SoChungTu);
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> HuyBangTrichLapAsync(long bangTrichLapId)
    {
        var bang = await _context.BangTrichLapDuPhongNoPhaiThus
            .Include(b => b.ButToan)
            .FirstOrDefaultAsync(b => b.Id == bangTrichLapId);

        if (bang == null)
        {
            return (false, "Không tìm thấy bảng trích lập dự phòng.");
        }

        // Kiểm tra khóa sổ
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh?.NgayKhoaSo.HasValue == true && bang.NgayHachToan <= cauHinh.NgayKhoaSo.Value.Date)
        {
            return (false, $"Kỳ kế toán đã khóa sổ đến ngày {cauHinh.NgayKhoaSo:dd/MM/yyyy}. Không thể hủy bảng trích lập.");
        }

        if (bang.ButToanId.HasValue)
        {
            var butToan = await _context.ButToans.FindAsync(bang.ButToanId.Value);
            if (butToan != null)
            {
                var chiTiets = await _context.ChiTietButToans.Where(c => c.ButToanId == butToan.Id).ToListAsync();
                _context.ChiTietButToans.RemoveRange(chiTiets);
                _context.ButToans.Remove(butToan);
            }
            bang.ButToanId = null;
        }

        bang.TrangThai = TrangThaiBangTrichLap.DaHuy;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã hủy Bảng trích lập dự phòng {SoChungTu}", bang.SoChungTu);
        return (true, null);
    }
}
