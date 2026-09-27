using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetKpisAsync()
    {
        var postedEntries = _context.ChiTietButToans
            .Include(ct => ct.TaiKhoanNo)
            .Include(ct => ct.TaiKhoanCo)
            .Include(ct => ct.ButToan)
            .Where(ct => ct.ButToan != null && ct.ButToan.TrangThai == TrangThaiButToan.DaGhiSo);

        // Doanh thu: Phát sinh Có TK 511, 711
        var doanhThu = await postedEntries
            .Where(ct => ct.TaiKhoanCo != null && (ct.TaiKhoanCo.MaTaiKhoan.StartsWith("511") || ct.TaiKhoanCo.MaTaiKhoan.StartsWith("711")))
            .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;

        // Chi phí: Phát sinh Nợ TK 632, 642, 811
        var chiPhi = await postedEntries
            .Where(ct => ct.TaiKhoanNo != null && (ct.TaiKhoanNo.MaTaiKhoan.StartsWith("632") || ct.TaiKhoanNo.MaTaiKhoan.StartsWith("642") || ct.TaiKhoanNo.MaTaiKhoan.StartsWith("811")))
            .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;

        // Phải thu: Phát sinh Nợ TK 131 trừ Có TK 131
        var no131 = await postedEntries
            .Where(ct => ct.TaiKhoanNo != null && ct.TaiKhoanNo.MaTaiKhoan.StartsWith("131"))
            .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;
        var co131 = await postedEntries
            .Where(ct => ct.TaiKhoanCo != null && ct.TaiKhoanCo.MaTaiKhoan.StartsWith("131"))
            .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;
        var phaiThu = Math.Max(0m, no131 - co131);

        // Phải trả: Phát sinh Có TK 331 trừ Nợ TK 331
        var co331 = await postedEntries
            .Where(ct => ct.TaiKhoanCo != null && ct.TaiKhoanCo.MaTaiKhoan.StartsWith("331"))
            .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;
        var no331 = await postedEntries
            .Where(ct => ct.TaiKhoanNo != null && ct.TaiKhoanNo.MaTaiKhoan.StartsWith("331"))
            .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;
        var phaiTra = Math.Max(0m, co331 - co331 > 0 ? co331 - no331 : co331);

        return new DashboardViewModel
        {
            DoanhThu = doanhThu,
            ChiPhi = chiPhi,
            PhaiThu = phaiThu,
            PhaiTra = phaiTra
        };
    }

    public async Task<List<MonthlyCashFlowDto>> GetCashFlowAsync(int months = 12)
    {
        var result = new List<MonthlyCashFlowDto>();
        var now = DateTime.Today;

        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var month = date.Month;
            var year = date.Year;

            var entries = _context.ChiTietButToans
                .Include(ct => ct.TaiKhoanNo)
                .Include(ct => ct.TaiKhoanCo)
                .Include(ct => ct.ButToan)
                .Where(ct => ct.ButToan != null && ct.ButToan.TrangThai == TrangThaiButToan.DaGhiSo
                             && ct.ButToan.NgayHachToan.Month == month && ct.ButToan.NgayHachToan.Year == year);

            // Inflow: Nợ 111, 112
            var inflow = await entries
                .Where(ct => ct.TaiKhoanNo != null && (ct.TaiKhoanNo.MaTaiKhoan.StartsWith("111") || ct.TaiKhoanNo.MaTaiKhoan.StartsWith("112")))
                .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;

            // Outflow: Có 111, 112
            var outflow = await entries
                .Where(ct => ct.TaiKhoanCo != null && (ct.TaiKhoanCo.MaTaiKhoan.StartsWith("111") || ct.TaiKhoanCo.MaTaiKhoan.StartsWith("112")))
                .SumAsync(ct => (decimal?)ct.SoTien) ?? 0m;

            result.Add(new MonthlyCashFlowDto
            {
                Month = $"T{month:D2}/{year}",
                Inflow = inflow,
                Outflow = outflow
            });
        }

        return result;
    }

    public async Task<List<DebtorDto>> GetTopDebtorsAsync(int count = 10)
    {
        var entries = await _context.ChiTietButToans
            .Include(ct => ct.TaiKhoanNo)
            .Include(ct => ct.TaiKhoanCo)
            .Include(ct => ct.DoiTuong)
            .Include(ct => ct.ButToan)
            .Where(ct => ct.ButToan != null && ct.ButToan.TrangThai == TrangThaiButToan.DaGhiSo
                         && ct.DoiTuongId != null
                         && ((ct.TaiKhoanNo != null && ct.TaiKhoanNo.MaTaiKhoan.StartsWith("131"))
                             || (ct.TaiKhoanCo != null && ct.TaiKhoanCo.MaTaiKhoan.StartsWith("131"))))
            .ToListAsync();

        var debtors = entries
            .GroupBy(ct => new { ct.DoiTuongId, ct.DoiTuong!.MaDoiTuong, ct.DoiTuong.TenDoiTuong })
            .Select(g => new DebtorDto
            {
                DoiTuongId = g.Key.DoiTuongId!.Value,
                MaDoiTuong = g.Key.MaDoiTuong,
                TenDoiTuong = g.Key.TenDoiTuong,
                DuNo = g.Sum(ct => ct.TaiKhoanNo != null && ct.TaiKhoanNo.MaTaiKhoan.StartsWith("131") ? ct.SoTien : -ct.SoTien)
            })
            .Where(d => d.DuNo > 0)
            .OrderByDescending(d => d.DuNo)
            .Take(count)
            .ToList();

        return debtors;
    }
}
