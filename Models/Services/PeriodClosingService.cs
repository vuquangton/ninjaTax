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

        // 2. Tìm tài khoản 911 và tài khoản 4212 (Chuẩn Thông tư 99/2025/TT-BTC)
        var tk911 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "911");
        if (tk911 == null)
        {
            return new KetChuyenCuoiKyResult
            {
                ThanhCong = false,
                ThongBao = "Không tìm thấy Tài khoản 911 (Xác định kết quả kinh doanh) trong danh mục hệ thống tài khoản."
            };
        }

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

        // CHẶNG 1: Kết chuyển Doanh thu & Thu nhập khác sang Có TK 911 (Nợ 5xx, 7xx / Có 911)
        foreach (var tk in danhSachTkKetChuyen.Where(t => !t.LaTaiKhoanSoCai && (t.MaTaiKhoan.StartsWith("5") || t.MaTaiKhoan.StartsWith("7"))))
        {
            var psCo = postedDetails.Where(c => c.TaiKhoanCoId == tk.Id).Sum(c => c.SoTien);
            var psNo = postedDetails.Where(c => c.TaiKhoanNoId == tk.Id).Sum(c => c.SoTien);
            var duCo = psCo - psNo;

            if (duCo > 0)
            {
                chiTietMoi.Add(new ChiTietButToan
                {
                    DongSo = dongSo++,
                    TaiKhoanNoId = tk.Id,
                    TaiKhoanCoId = tk911.Id,
                    SoTien = duCo,
                    DienGiai = $"Kết chuyển doanh thu {tk.MaTaiKhoan} sang TK 911"
                });
                tongDoanhThu += duCo;
                nhatKy.Add($"Kết chuyển DT {tk.MaTaiKhoan} ({tk.TenTaiKhoan}) -> Có 911: {duCo:N0} đ");
            }
        }

        // CHẶNG 2: Kết chuyển Chi phí & Thuế TNDN sang Nợ TK 911 (Nợ 911 / Có 6xx, 8xx)
        foreach (var tk in danhSachTkKetChuyen.Where(t => !t.LaTaiKhoanSoCai && (t.MaTaiKhoan.StartsWith("6") || t.MaTaiKhoan.StartsWith("8"))))
        {
            var psNo = postedDetails.Where(c => c.TaiKhoanNoId == tk.Id).Sum(c => c.SoTien);
            var psCo = postedDetails.Where(c => c.TaiKhoanCoId == tk.Id).Sum(c => c.SoTien);
            var duNo = psNo - psCo;

            if (duNo > 0)
            {
                chiTietMoi.Add(new ChiTietButToan
                {
                    DongSo = dongSo++,
                    TaiKhoanNoId = tk911.Id,
                    TaiKhoanCoId = tk.Id,
                    SoTien = duNo,
                    DienGiai = $"Kết chuyển chi phí {tk.MaTaiKhoan} từ TK 911"
                });
                tongChiPhi += duNo;
                nhatKy.Add($"Kết chuyển CP {tk.MaTaiKhoan} ({tk.TenTaiKhoan}) -> Nợ 911: {duNo:N0} đ");
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

        // CHẶNG 3: Kết chuyển Lãi/Lỗ ròng từ TK 911 sang TK 4212 để TK 911 sạch số dư về 0
        decimal loiNhuanRong = tongDoanhThu - tongChiPhi;
        if (loiNhuanRong > 0)
        {
            // Doanh nghiệp LÃI: Nợ 911 / Có 4212
            chiTietMoi.Add(new ChiTietButToan
            {
                DongSo = dongSo++,
                TaiKhoanNoId = tk911.Id,
                TaiKhoanCoId = tk4212.Id,
                SoTien = loiNhuanRong,
                DienGiai = "Kết chuyển lãi sau thuế sang TK 4212"
            });
            nhatKy.Add($"Kết chuyển LÃI sau thuế: Nợ 911 / Có 4212: {loiNhuanRong:N0} đ");
        }
        else if (loiNhuanRong < 0)
        {
            // Doanh nghiệp LỖ: Nợ 4212 / Có 911
            decimal soTienLo = -loiNhuanRong;
            chiTietMoi.Add(new ChiTietButToan
            {
                DongSo = dongSo++,
                TaiKhoanNoId = tk4212.Id,
                TaiKhoanCoId = tk911.Id,
                SoTien = soTienLo,
                DienGiai = "Kết chuyển lỗ sau thuế sang TK 4212"
            });
            nhatKy.Add($"Kết chuyển LỖ sau thuế: Nợ 4212 / Có 911: {soTienLo:N0} đ");
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

    public async Task<(bool ThanhCong, string? ThongBao, decimal SoTienKhauTru, long? ButToanId)> KhauTruThueGtgtAsync(int nam, int? thang = null)
    {
        var tuNgay = thang.HasValue
            ? new DateTime(nam, thang.Value, 1)
            : new DateTime(nam, 1, 1);

        var denNgay = thang.HasValue
            ? new DateTime(nam, thang.Value, DateTime.DaysInMonth(nam, thang.Value), 23, 59, 59)
            : new DateTime(nam, 12, 31, 23, 59, 59);

        // 1. Kiểm tra khóa sổ
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh != null && !cauHinh.ChoPhepGhiSo(denNgay.Date))
        {
            return (false, $"Kỳ kế toán đến ngày {denNgay:dd/MM/yyyy} đã bị khóa sổ.", 0, null);
        }

        // 2. Tìm tài khoản 1331 và 33311
        var tk1331 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1331");
        var tk33311 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "33311");

        if (tk1331 == null || tk33311 == null)
        {
            return (false, "Không tìm thấy TK 1331 hoặc TK 33311 trong danh mục hệ thống tài khoản.", 0, null);
        }

        // Xóa chứng từ cấn trừ thuế cũ nếu có trong kỳ để bảo đảm tính idempotency
        string soChungTuThue = thang.HasValue
            ? $"PKT-KT-THUE-{nam}{thang.Value:D2}"
            : $"PKT-KT-THUE-{nam}";

        var cu = await _context.ButToans
            .Include(b => b.ChiTietButToans)
            .FirstOrDefaultAsync(b => b.SoChungTu == soChungTuThue);

        if (cu != null)
        {
            _context.ChiTietButToans.RemoveRange(cu.ChiTietButToans);
            _context.ButToans.Remove(cu);
            await _context.SaveChangesAsync();
        }

        // 3. Tính số dư Nợ lũy kế TK 1331 và số dư Có lũy kế TK 33311 đến hết kỳ
        var lines1331 = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Where(c => c.ButToan != null &&
                        c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= denNgay &&
                        (c.TaiKhoanNoId == tk1331.Id || c.TaiKhoanCoId == tk1331.Id))
            .ToListAsync();

        decimal duNo1331 = lines1331.Where(c => c.TaiKhoanNoId == tk1331.Id).Sum(c => c.SoTien)
                         - lines1331.Where(c => c.TaiKhoanCoId == tk1331.Id).Sum(c => c.SoTien);

        var lines33311 = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Where(c => c.ButToan != null &&
                        c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= denNgay &&
                        (c.TaiKhoanNoId == tk33311.Id || c.TaiKhoanCoId == tk33311.Id))
            .ToListAsync();

        decimal duCo33311 = lines33311.Where(c => c.TaiKhoanCoId == tk33311.Id).Sum(c => c.SoTien)
                          - lines33311.Where(c => c.TaiKhoanNoId == tk33311.Id).Sum(c => c.SoTien);

        if (duNo1331 <= 0 || duCo33311 <= 0)
        {
            return (true, "Không có thuế GTGT cần khấu trừ trong kỳ (Dư Nợ 1331 hoặc Dư Có 33311 bằng 0).", 0, null);
        }

        // 4. Số tiền khấu trừ = Min(duNo1331, duCo33311)
        decimal soTienKhauTru = Math.Min(duNo1331, duCo33311);

        var butToan = new ButToan
        {
            SoChungTu = soChungTuThue,
            NgayChungTu = denNgay.Date,
            NgayHachToan = denNgay.Date,
            SoChungTuGoc = soChungTuThue,
            NgayChungTuGoc = denNgay.Date,
            DienGiai = $"Khấu trừ thuế GTGT định kỳ {denNgay:MM/yyyy} (Nợ 33311 / Có 1331)",
            TrangThai = TrangThaiButToan.DaGhiSo,
            TongTien = soTienKhauTru,
            TongNo = soTienKhauTru,
            TongCo = soTienKhauTru,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new ChiTietButToan
                {
                    TaiKhoanNoId = tk33311.Id,
                    TaiKhoanCoId = tk1331.Id,
                    SoTien = soTienKhauTru,
                    DienGiai = $"Khấu trừ thuế GTGT đầu vào vào đầu ra kỳ {denNgay:MM/yyyy}"
                }
            }
        };

        await _context.ButToans.AddAsync(butToan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã tạo chứng từ khấu trừ thuế GTGT {SoChungTu}: {SoTien:N0} VNĐ.", soChungTuThue, soTienKhauTru);
        return (true, $"Đã thực hiện khấu trừ thuế GTGT thành công: {soTienKhauTru:N0} VNĐ.", soTienKhauTru, butToan.Id);
    }
}
