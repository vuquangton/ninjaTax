using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ lập Sổ Cái (S03b-DN), Sổ Nhật ký chung (S03a-DN) và Bảng Cân đối tài khoản 8 cột (Trial Balance) theo TT99/2025/TT-BTC.
/// </summary>
public class GeneralLedgerService : IGeneralLedgerService
{
    private readonly AppDbContext _context;
    private readonly ILogger<GeneralLedgerService> _logger;

    public GeneralLedgerService(
        AppDbContext context,
        ILogger<GeneralLedgerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SoNhatKyChungViewModel> LaySoNhatKyChungAsync(DateTime tuNgay, DateTime denNgay)
    {
        var start = tuNgay.Date;
        var end = denNgay.Date.AddDays(1).AddTicks(-1);

        var query = _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Include(c => c.TaiKhoanNo)
            .Include(c => c.TaiKhoanCo)
            .Include(c => c.DoiTuong)
            .Include(c => c.PhongBan)
            .Where(c => c.ButToan != null &&
                        c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= start &&
                        c.ButToan.NgayHachToan <= end)
            .OrderBy(c => c.ButToan!.NgayHachToan)
            .ThenBy(c => c.ButToanId)
            .ThenBy(c => c.DongSo);

        var details = await query.ToListAsync();

        var rows = details.Select(d => new DongSoNhatKyChungViewModel
        {
            Id = d.Id,
            NgayHachToan = d.ButToan!.NgayHachToan,
            NgayChungTu = d.ButToan!.NgayChungTu,
            SoChungTu = d.ButToan!.SoChungTu,
            DienGiai = string.IsNullOrWhiteSpace(d.DienGiai) ? d.ButToan!.DienGiai : d.DienGiai,
            MaTaiKhoanNo = d.TaiKhoanNo?.MaTaiKhoan ?? string.Empty,
            MaTaiKhoanCo = d.TaiKhoanCo?.MaTaiKhoan ?? string.Empty,
            SoTien = d.SoTien,
            DoiTuong = d.DoiTuong?.TenDoiTuong,
            PhongBan = d.PhongBan?.TenPhongBan
        }).ToList();

        return new SoNhatKyChungViewModel
        {
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TongSoDong = rows.Count,
            TongPhatSinh = rows.Sum(r => r.SoTien),
            DongChiTiets = rows
        };
    }

    public async Task<SoCaiViewModel> LaySoCaiAsync(string maTaiKhoan, DateTime tuNgay, DateTime denNgay)
    {
        var start = tuNgay.Date;
        var end = denNgay.Date.AddDays(1).AddTicks(-1);

        var tk = await _context.TaiKhoans
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

        if (tk == null)
        {
            return new SoCaiViewModel
            {
                MaTaiKhoan = maTaiKhoan,
                TuNgay = tuNgay,
                DenNgay = denNgay
            };
        }

        // 1. Số dư đầu kỳ (Trước ngày bắt đầu)
        var psNoDauKy = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan < start &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTaiKhoan))
            .SumAsync(c => c.SoTien);

        var psCoDauKy = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan < start &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTaiKhoan))
            .SumAsync(c => c.SoTien);

        decimal duNoDauKy = 0m;
        decimal duCoDauKy = 0m;

        if (psNoDauKy >= psCoDauKy)
        {
            duNoDauKy = psNoDauKy - psCoDauKy;
        }
        else
        {
            duCoDauKy = psCoDauKy - psNoDauKy;
        }

        // 2. Phát sinh trong kỳ
        var chiTietsInPeriod = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Include(c => c.TaiKhoanNo)
            .Include(c => c.TaiKhoanCo)
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= start && c.ButToan.NgayHachToan <= end &&
                        ((c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTaiKhoan)) ||
                         (c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTaiKhoan))))
            .OrderBy(c => c.ButToan!.NgayHachToan)
            .ThenBy(c => c.ButToanId)
            .ThenBy(c => c.DongSo)
            .ToListAsync();

        var rows = new List<DongSoCaiViewModel>();
        decimal currentBalance = duNoDauKy - duCoDauKy;

        foreach (var item in chiTietsInPeriod)
        {
            bool isDebit = item.TaiKhoanNo != null && item.TaiKhoanNo.MaTaiKhoan.StartsWith(maTaiKhoan);
            decimal psNo = isDebit ? item.SoTien : 0m;
            decimal psCo = !isDebit ? item.SoTien : 0m;

            currentBalance += psNo - psCo;

            rows.Add(new DongSoCaiViewModel
            {
                ButToanId = item.ButToanId,
                NgayHachToan = item.ButToan!.NgayHachToan,
                NgayChungTu = item.ButToan!.NgayChungTu,
                SoChungTu = item.ButToan!.SoChungTu,
                DienGiai = string.IsNullOrWhiteSpace(item.DienGiai) ? item.ButToan!.DienGiai : item.DienGiai,
                TaiKhoanDoiUng = isDebit ? (item.TaiKhoanCo?.MaTaiKhoan ?? "") : (item.TaiKhoanNo?.MaTaiKhoan ?? ""),
                PhatSinhNo = psNo,
                PhatSinhCo = psCo,
                SoDuLuyKe = currentBalance
            });
        }

        decimal tongPsNo = rows.Sum(r => r.PhatSinhNo);
        decimal tongPsCo = rows.Sum(r => r.PhatSinhCo);

        decimal finalNet = (duNoDauKy - duCoDauKy) + (tongPsNo - tongPsCo);
        decimal duNoCuoiKy = finalNet > 0 ? finalNet : 0m;
        decimal duCoCuoiKy = finalNet < 0 ? -finalNet : 0m;

        return new SoCaiViewModel
        {
            MaTaiKhoan = tk.MaTaiKhoan,
            TenTaiKhoan = tk.TenTaiKhoan,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            DuNoDauKy = duNoDauKy,
            DuCoDauKy = duCoDauKy,
            TongPhatSinhNo = tongPsNo,
            TongPhatSinhCo = tongPsCo,
            DuNoCuoiKy = duNoCuoiKy,
            DuCoCuoiKy = duCoCuoiKy,
            DongChiTiets = rows
        };
    }

    public async Task<BangCanDoiTaiKhoanViewModel> LayBangCanDoiTaiKhoanAsync(DateTime tuNgay, DateTime denNgay)
    {
        var start = tuNgay.Date;
        var end = denNgay.Date.AddDays(1).AddTicks(-1);

        var taiKhoans = await _context.TaiKhoans
            .Where(t => t.DangHoatDong)
            .OrderBy(t => t.MaTaiKhoan)
            .ToListAsync();

        // 1. Lấy tất cả bút toán phát sinh lũy kế đến hết kỳ
        var allLines = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= end)
            .Select(c => new
            {
                c.TaiKhoanNoId,
                c.TaiKhoanCoId,
                c.SoTien,
                Ngay = c.ButToan!.NgayHachToan
            })
            .ToListAsync();

        var linesDauKy = allLines.Where(l => l.Ngay < start).ToList();
        var linesTrongKy = allLines.Where(l => l.Ngay >= start && l.Ngay <= end).ToList();

        var danhSach = new List<DongBangCanDoiTaiKhoanViewModel>();

        foreach (var tk in taiKhoans)
        {
            // Đầu kỳ
            var noDauKy = linesDauKy.Where(l => l.TaiKhoanNoId == tk.Id).Sum(l => l.SoTien);
            var coDauKy = linesDauKy.Where(l => l.TaiKhoanCoId == tk.Id).Sum(l => l.SoTien);

            decimal duNoDau = 0m;
            decimal duCoDau = 0m;
            if (noDauKy >= coDauKy) duNoDau = noDauKy - coDauKy;
            else duCoDau = coDauKy - noDauKy;

            // Trong kỳ
            var psNo = linesTrongKy.Where(l => l.TaiKhoanNoId == tk.Id).Sum(l => l.SoTien);
            var psCo = linesTrongKy.Where(l => l.TaiKhoanCoId == tk.Id).Sum(l => l.SoTien);

            // Cuối kỳ
            decimal finalNet = (duNoDau - duCoDau) + (psNo - psCo);
            decimal duNoCuoi = finalNet > 0 ? finalNet : 0m;
            decimal duCoCuoi = finalNet < 0 ? -finalNet : 0m;

            // Chỉ thêm các tài khoản có số dư hoặc có phát sinh
            if (duNoDau > 0 || duCoDau > 0 || psNo > 0 || psCo > 0 || duNoCuoi > 0 || duCoCuoi > 0)
            {
                danhSach.Add(new DongBangCanDoiTaiKhoanViewModel
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    TenTaiKhoan = tk.TenTaiKhoan,
                    BacTaiKhoan = tk.BacTaiKhoan,
                    LaTaiKhoanSoCai = tk.LaTaiKhoanSoCai,
                    DuNoDauKy = duNoDau,
                    DuCoDauKy = duCoDau,
                    PhatSinhNoTrongKy = psNo,
                    PhatSinhCoTrongKy = psCo,
                    DuNoCuoiKy = duNoCuoi,
                    DuCoCuoiKy = duCoCuoi
                });
            }
        }

        // Tính tổng cộng chỉ dựa trên các tài khoản cấp lá (hoặc tài khoản chi tiết) để tránh nhân đôi
        // Trong hệ thống ninjaTax, các dòng chi tiết ghi nhận vào tài khoản có phát sinh trực tiếp
        return new BangCanDoiTaiKhoanViewModel
        {
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TongDuNoDauKy = danhSach.Sum(d => d.DuNoDauKy),
            TongDuCoDauKy = danhSach.Sum(d => d.DuCoDauKy),
            TongPhatSinhNoTrongKy = danhSach.Sum(d => d.PhatSinhNoTrongKy),
            TongPhatSinhCoTrongKy = danhSach.Sum(d => d.PhatSinhCoTrongKy),
            TongDuNoCuoiKy = danhSach.Sum(d => d.DuNoCuoiKy),
            TongDuCoCuoiKy = danhSach.Sum(d => d.DuCoCuoiKy),
            DanhSachTaiKhoan = danhSach
        };
    }
}
