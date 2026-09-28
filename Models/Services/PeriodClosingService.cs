using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ kết chuyển tự động doanh thu & chi phí cuối kỳ tuân thủ nghiêm ngặt Thông tư 99/2025/TT-BTC.
/// Quy tắc cốt lõi:
/// - TUYỆT ĐỐI CẤM TÀI KHOẢN 911.
/// - Doanh thu (511, 515, 711) kết chuyển trực tiếp Nợ 5xx, 7xx / Có 4212.
/// - Chi phí (632, 635, 641, 642, 811, 821) kết chuyển trực tiếp Nợ 4212 / Có 6xx, 8xx.
/// - Đảm bảo nguyên tắc cân đối kép và kiểm tra khóa sổ kế toán.
/// </summary>
public class PeriodClosingService : IPeriodClosingService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ILogger<PeriodClosingService> _logger;

    public PeriodClosingService(
        AppDbContext context,
        IButToanService butToanService,
        ILogger<PeriodClosingService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _logger = logger;
    }

    public async Task<KetChuyenCuoiKyResult> TaoButToanKetChuyenAsync(int nam, int? thang = null)
    {
        var tuNgay = thang.HasValue
            ? new DateTime(nam, thang.Value, 1)
            : new DateTime(nam, 1, 1);

        var denNgay = thang.HasValue
            ? new DateTime(nam, thang.Value, DateTime.DaysInMonth(nam, thang.Value), 23, 59, 59)
            : new DateTime(nam, 12, 31, 23, 59, 59);

        // 1. Kiểm tra khóa sổ kế toán
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh != null && !cauHinh.ChoPhepGhiSo(denNgay.Date))
        {
            return new KetChuyenCuoiKyResult
            {
                ThanhCong = false,
                ThongBao = $"Kỳ kế toán đến ngày {denNgay:dd/MM/yyyy} đã bị khóa sổ. Không thể thực hiện kết chuyển."
            };
        }

        // 2. Tìm tài khoản 4212 (Lợi nhuận sau thuế chưa phân phối năm nay)
        var tk4212 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "4212");
        if (tk4212 == null)
        {
            return new KetChuyenCuoiKyResult
            {
                ThanhCong = false,
                ThongBao = "Không tìm thấy Tài khoản 4212 trong danh mục hệ thống tài khoản kế toán."
            };
        }

        // 3. Quét toàn bộ chi tiết bút toán ĐÃ GHI SỔ trong kỳ loại trừ các bút toán kết chuyển trước đó
        var postedDetails = await _context.ChiTietButToans
            .Include(c => c.TaiKhoanNo)
            .Include(c => c.TaiKhoanCo)
            .Include(c => c.ButToan)
            .Where(c => c.ButToan != null &&
                        c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        !c.ButToan.SoChungTu.StartsWith("PKT-KC-") &&
                        c.ButToan.NgayHachToan >= tuNgay &&
                        c.ButToan.NgayHachToan <= denNgay)
            .ToListAsync();

        // 4. Tính toán số dư chưa kết chuyển của các tài khoản Doanh thu (5xx, 7xx) và Chi phí (6xx, 8xx)
        // Lấy tất cả tài khoản loại 5, 6, 7, 8
        var danhSachTkKetChuyen = await _context.TaiKhoans
            .Where(t => t.DangHoatDong &&
                        (t.MaTaiKhoan.StartsWith("5") ||
                         t.MaTaiKhoan.StartsWith("6") ||
                         t.MaTaiKhoan.StartsWith("7") ||
                         t.MaTaiKhoan.StartsWith("8")))
            .ToListAsync();

        var chiTietMoi = new List<ChiTietButToan>();
        int dongSo = 1;
        decimal tongDoanhThu = 0m;
        decimal tongChiPhi = 0m;
        var nhatKy = new List<string>();

        foreach (var tk in danhSachTkKetChuyen.Where(t => !t.LaTaiKhoanSoCai))
        {
            // Số phát sinh Có và Nợ
            var psCo = postedDetails.Where(c => c.TaiKhoanCoId == tk.Id).Sum(c => c.SoTien);
            var psNo = postedDetails.Where(c => c.TaiKhoanNoId == tk.Id).Sum(c => c.SoTien);

            if (tk.MaTaiKhoan.StartsWith("5") || tk.MaTaiKhoan.StartsWith("7"))
            {
                // Doanh thu / Thu nhập khác: Phát sinh Có lớn hơn Nợ -> Kết chuyển Nợ 5xx,7xx / Có 4212
                var duCo = psCo - psNo;
                if (duCo > 0)
                {
                    chiTietMoi.Add(new ChiTietButToan
                    {
                        DongSo = dongSo++,
                        TaiKhoanNoId = tk.Id,
                        TaiKhoanCoId = tk4212.Id,
                        SoTien = duCo,
                        DienGiai = $"Kết chuyển doanh thu {tk.MaTaiKhoan} sang TK 4212"
                    });
                    tongDoanhThu += duCo;
                    nhatKy.Add($"Kết chuyển DT {tk.MaTaiKhoan} ({tk.TenTaiKhoan}): {duCo:N0} đ");
                }
            }
            else if (tk.MaTaiKhoan.StartsWith("6") || tk.MaTaiKhoan.StartsWith("8"))
            {
                // Chi phí: Phát sinh Nợ lớn hơn Có -> Kết chuyển Nợ 4212 / Có 6xx,8xx
                var duNo = psNo - psCo;
                if (duNo > 0)
                {
                    chiTietMoi.Add(new ChiTietButToan
                    {
                        DongSo = dongSo++,
                        TaiKhoanNoId = tk4212.Id,
                        TaiKhoanCoId = tk.Id,
                        SoTien = duNo,
                        DienGiai = $"Kết chuyển chi phí {tk.MaTaiKhoan} sang TK 4212"
                    });
                    tongChiPhi += duNo;
                    nhatKy.Add($"Kết chuyển CP {tk.MaTaiKhoan} ({tk.TenTaiKhoan}): {duNo:N0} đ");
                }
            }
        }

        if (chiTietMoi.Count == 0)
        {
            return new KetChuyenCuoiKyResult
            {
                ThanhCong = false,
                ThongBao = "Không có số dư doanh thu hoặc chi phí cần kết chuyển trong kỳ đã chọn."
            };
        }

        // 5. Tạo chứng từ Bút toán kết chuyển
        var soChungTu = thang.HasValue
            ? $"PKT-KC-{nam}{thang.Value:00}"
            : $"PKT-KC-{nam}";

        // Xóa chứng từ kết chuyển cũ nếu có
        var existingBt = await _context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstOrDefaultAsync(b => b.SoChungTu == soChungTu);

        if (existingBt != null)
        {
            _context.ChiTietButToans.RemoveRange(existingBt.ChiTietButToans);
            _context.ButToans.Remove(existingBt);
            await _context.SaveChangesAsync();
        }

        var butToan = new ButToan
        {
            SoChungTu = soChungTu,
            NgayHachToan = denNgay.Date,
            NgayChungTu = denNgay.Date,
            DienGiai = thang.HasValue
                ? $"Kết chuyển kết quả kinh doanh Tháng {thang}/{nam} (TT99)"
                : $"Kết chuyển kết quả kinh doanh Năm {nam} (TT99)",
            TrangThai = TrangThaiButToan.DaGhiSo,
            TongNo = chiTietMoi.Sum(c => c.SoTien),
            TongCo = chiTietMoi.Sum(c => c.SoTien),
            TongTien = chiTietMoi.Sum(c => c.SoTien),
            ChiTietButToans = chiTietMoi
        };

        _context.ButToans.Add(butToan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã hoàn tất kết chuyển kỳ {Ky}: Tổng DT={DT}, Tổng CP={CP}, Lãi={Lai}",
            soChungTu, tongDoanhThu, tongChiPhi, tongDoanhThu - tongChiPhi);

        return new KetChuyenCuoiKyResult
        {
            ThanhCong = true,
            ThongBao = $"Đã thực hiện kết chuyển thành công chứng từ {soChungTu}.",
            ButToanKetChuyenId = butToan.Id,
            SoChungTuKetChuyen = soChungTu,
            TongDoanhThuKetChuyen = tongDoanhThu,
            TongChiPhiKetChuyen = tongChiPhi,
            LoiNhuanSauThueKetChuyen = tongDoanhThu - tongChiPhi,
            NhatKyKetChuyen = nhatKy
        };
    }

    public async Task<(bool ThanhCong, string? ThongBao)> HuyKetChuyenAsync(long butToanKetChuyenId)
    {
        var bt = await _context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstOrDefaultAsync(b => b.Id == butToanKetChuyenId);

        if (bt == null)
        {
            return (false, "Không tìm thấy bút toán kết chuyển.");
        }

        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh != null && !cauHinh.ChoPhepGhiSo(bt.NgayHachToan))
        {
            return (false, $"Kỳ kế toán đã bị khóa sổ đến hết ngày {cauHinh.NgayKhoaSo:dd/MM/yyyy}. Không thể hủy kết chuyển.");
        }

        _context.ChiTietButToans.RemoveRange(bt.ChiTietButToans);
        _context.ButToans.Remove(bt);
        await _context.SaveChangesAsync();

        return (true, "Đã hủy chứng từ kết chuyển thành công.");
    }
}
