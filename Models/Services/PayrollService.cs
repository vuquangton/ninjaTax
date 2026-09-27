using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ITimesheetService _timesheetService;
    private readonly ILogger<PayrollService> _logger;

    // Các hằng số quy định pháp luật
    // NĐ 73/2024/NĐ-CP: Mức lương cơ sở = 2.340.000 VNĐ
    public const decimal LuongCoSo = 2_340_000m;
    public const decimal TranBhxhBhyt = 20 * LuongCoSo; // 46.800.000 VNĐ

    // NĐ 74/2024/NĐ-CP: Mức lương tối thiểu vùng tháng
    public static readonly Dictionary<int, decimal> LuongToiThieuVung = new()
    {
        { 1, 4_960_000m },
        { 2, 4_410_000m },
        { 3, 3_860_000m },
        { 4, 3_450_000m }
    };

    // Tỷ lệ trích BHXH, BHYT, BHTN, KPCĐ
    public const decimal TyLeBhxhDn = 0.175m;
    public const decimal TyLeBhytDn = 0.030m;
    public const decimal TyLeBhtnDn = 0.010m;
    public const decimal TyLeKpcdDn = 0.020m;

    public const decimal TyLeBhxhNld = 0.080m;
    public const decimal TyLeBhytNld = 0.015m;
    public const decimal TyLeBhtnNld = 0.010m;

    // Giảm trừ gia cảnh theo NQ 954/2020/UBTVQH14
    public const decimal GiamTruBanThanDinhMuc = 11_000_000m;
    public const decimal GiamTruPhuThuocDinhMuc = 4_400_000m;

    // Hạn mức phụ cấp ăn trưa miễn thuế TNCN theo TT 26/2016/TT-BLĐTBXH
    public const decimal PhuCapAnTruaMienThueMax = 730_000m;
    // Hạn mức phụ cấp trang phục miễn thuế TNCN bằng tiền theo TT 111/2013: 5.000.000 VNĐ/năm = ~416.666 VNĐ/tháng
    public const decimal PhuCapTrangPhucMienThueThangMax = 5_000_000m / 12m;

    public PayrollService(
        AppDbContext context,
        IButToanService butToanService,
        ITimesheetService timesheetService,
        ILogger<PayrollService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _timesheetService = timesheetService;
        _logger = logger;
    }

    public (decimal BhxhDn, decimal BhytDn, decimal BhtnDn, decimal KpcdDn) TinhBaoHiemDoanhNghiep(decimal luongDongBh, int vung = 1)
    {
        var toiThieuVung = LuongToiThieuVung.GetValueOrDefault(vung, 4_960_000m);
        if (luongDongBh < toiThieuVung)
        {
            throw new InvalidOperationException($"Mức lương đóng BHXH {luongDongBh:N0} VNĐ thấp hơn mức lương tối thiểu vùng {toiThieuVung:N0} VNĐ theo Nghị định 74/2024/NĐ-CP.");
        }

        // Trần BHXH & BHYT: 20 lần lương cơ sở (NĐ 73/2024)
        var luongTinhBhxhBhyt = Math.Min(luongDongBh, TranBhxhBhyt);
        // Trần BHTN: 20 lần lương tối thiểu vùng (NĐ 74/2024)
        var tranBhtn = 20 * toiThieuVung;
        var luongTinhBhtn = Math.Min(luongDongBh, tranBhtn);

        var bhxhDn = Math.Round(luongTinhBhxhBhyt * TyLeBhxhDn, 0, MidpointRounding.AwayFromZero);
        var bhytDn = Math.Round(luongTinhBhxhBhyt * TyLeBhytDn, 0, MidpointRounding.AwayFromZero);
        var bhtnDn = Math.Round(luongTinhBhtn * TyLeBhtnDn, 0, MidpointRounding.AwayFromZero);
        var kpcdDn = Math.Round(luongTinhBhxhBhyt * TyLeKpcdDn, 0, MidpointRounding.AwayFromZero);

        return (bhxhDn, bhytDn, bhtnDn, kpcdDn);
    }

    public (decimal BhxhNld, decimal BhytNld, decimal BhtnNld) TinhBaoHiemNguoiLaoDong(decimal luongDongBh, int vung = 1)
    {
        var toiThieuVung = LuongToiThieuVung.GetValueOrDefault(vung, 4_960_000m);
        if (luongDongBh < toiThieuVung)
        {
            throw new InvalidOperationException($"Mức lương đóng BHXH {luongDongBh:N0} VNĐ thấp hơn mức lương tối thiểu vùng {toiThieuVung:N0} VNĐ theo Nghị định 74/2024/NĐ-CP.");
        }

        var luongTinhBhxhBhyt = Math.Min(luongDongBh, TranBhxhBhyt);
        var tranBhtn = 20 * toiThieuVung;
        var luongTinhBhtn = Math.Min(luongDongBh, tranBhtn);

        var bhxhNld = Math.Round(luongTinhBhxhBhyt * TyLeBhxhNld, 0, MidpointRounding.AwayFromZero);
        var bhytNld = Math.Round(luongTinhBhxhBhyt * TyLeBhytNld, 0, MidpointRounding.AwayFromZero);
        var bhtnNld = Math.Round(luongTinhBhtn * TyLeBhtnNld, 0, MidpointRounding.AwayFromZero);

        return (bhxhNld, bhytNld, bhtnNld);
    }

    public decimal TinhThueTncn(NhanVien nv, decimal tongThuNhap, decimal baoHiemNld, decimal otMienThue, decimal phuCapMienThue)
    {
        // 1. Lao động thử việc / thời vụ dưới 3 tháng
        if (nv.LoaiHopDong == LoaiHopDongLaoDong.ThuViecThoiVu)
        {
            // Điểm i Khoản 1 Điều 25 TT 111/2013/TT-BTC:
            // Chi trả từ 2.000.000 VNĐ/lần trở lên phải khấu trừ 10% tại nguồn
            if (tongThuNhap < 2_000_000m)
            {
                return 0m;
            }

            // Có cam kết Mẫu 08/CK-TNCN theo TT 80/2021 và đã có MST cá nhân
            if (nv.CoCamKet08 && !string.IsNullOrWhiteSpace(nv.MaSoThue))
            {
                return 0m;
            }

            // Khấu trừ toàn phần 10% trên tổng thu nhập (không trừ gia cảnh)
            return Math.Round(tongThuNhap * 0.10m, 0, MidpointRounding.AwayFromZero);
        }

        // 2. Lao động ký hợp đồng từ 3 tháng trở lên: Áp dụng Biểu thuế lũy tiến từng phần 7 bậc
        var thuNhapChiuThue = tongThuNhap - otMienThue - phuCapMienThue;
        var giamTruBanThan = GiamTruBanThanDinhMuc;
        var giamTruPhuThuoc = nv.SoNguoiPhuThuoc * GiamTruPhuThuocDinhMuc;
        var tongGiamTru = giamTruBanThan + giamTruPhuThuoc + baoHiemNld;

        var thuNhapTinhThue = thuNhapChiuThue - tongGiamTru;
        if (thuNhapTinhThue <= 0)
        {
            return 0m;
        }

        decimal thueTncn = 0m;
        if (thuNhapTinhThue <= 5_000_000m)
        {
            thueTncn = thuNhapTinhThue * 0.05m;
        }
        else if (thuNhapTinhThue <= 10_000_000m)
        {
            thueTncn = (thuNhapTinhThue * 0.10m) - 250_000m;
        }
        else if (thuNhapTinhThue <= 18_000_000m)
        {
            thueTncn = (thuNhapTinhThue * 0.15m) - 750_000m;
        }
        else if (thuNhapTinhThue <= 32_000_000m)
        {
            thueTncn = (thuNhapTinhThue * 0.20m) - 1_650_000m;
        }
        else if (thuNhapTinhThue <= 52_000_000m)
        {
            thueTncn = (thuNhapTinhThue * 0.25m) - 3_250_000m;
        }
        else if (thuNhapTinhThue <= 80_000_000m)
        {
            thueTncn = (thuNhapTinhThue * 0.30m) - 5_850_000m;
        }
        else
        {
            thueTncn = (thuNhapTinhThue * 0.35m) - 9_850_000m;
        }

        return Math.Max(0m, Math.Round(thueTncn, 0, MidpointRounding.AwayFromZero));
    }

    public async Task<BangLuongChiTietViewModel> TinhLuongThangAsync(string kyKeToan)
    {
        var bangChamCong = await _timesheetService.LayHoacTaoBangChamCongAsync(kyKeToan);
        var nhanViens = await _context.NhanViens
            .Where(nv => nv.DangLamViec)
            .OrderBy(nv => nv.MaNhanVien)
            .ToListAsync();

        var bangLuong = await _context.BangLuongThangs
            .Include(bl => bl.ChiTiets)
            .FirstOrDefaultAsync(bl => bl.KyKeToan == kyKeToan);

        if (bangLuong == null)
        {
            bangLuong = new BangLuongThang
            {
                SoChungTu = $"BL-{kyKeToan.Replace("-", "")}",
                KyKeToan = kyKeToan,
                NgayLap = DateTime.Today,
                SoNgayCongChuan = bangChamCong.SoNgayCongChuan,
                TrangThai = TrangThaiBangLuong.ChoDuyet,
                NgayTao = DateTime.UtcNow
            };
            await _context.BangLuongThangs.AddAsync(bangLuong);
            await _context.SaveChangesAsync();
        }
        else if (bangLuong.TrangThai == TrangThaiBangLuong.DaGhiSo)
        {
            // Trả về view model hiện tại nếu đã ghi sổ
            return MapToViewModel(bangLuong);
        }

        // Xóa chi tiết cũ nếu tính lại khi chưa ghi sổ
        if (bangLuong.ChiTiets.Any())
        {
            _context.ChiTietLuongNhanViens.RemoveRange(bangLuong.ChiTiets);
            await _context.SaveChangesAsync();
        }

        var chiTietList = new List<ChiTietLuongNhanVien>();
        decimal tongQuyLuong = 0m;
        decimal tongBhDn = 0m;
        decimal tongBhNld = 0m;
        decimal tongThue = 0m;
        decimal tongThucLinh = 0m;

        var soNgayChuan = bangLuong.SoNgayCongChuan > 0 ? bangLuong.SoNgayCongChuan : 22;

        foreach (var nv in nhanViens)
        {
            var cc = bangChamCong.DongChamCongs.FirstOrDefault(c => c.NhanVienId == nv.Id);
            var soNgayDiLam = cc?.SoNgayDiLam ?? 0m;
            var soNgayPhep = cc?.SoNgayNghiPhep ?? 0m;
            var soNgayLe = cc?.SoNgayNghiLe ?? 0m;
            var otThuong = cc?.GioLamThemNgayThuong ?? 0m;
            var otNghi = cc?.GioLamThemNgayNghi ?? 0m;
            var otLe = cc?.GioLamThemNgayLe ?? 0m;
            var tamUng = 0m;

            var donGiaNgay = soNgayChuan > 0 ? nv.LuongCoBan / soNgayChuan : 0m;
            var donGiaGio = donGiaNgay / 8m;

            // 1. Lương thời gian (ngày làm + phép năm + nghỉ lễ hưởng nguyên lương theo BLLĐ 2019)
            var tongNgayHuongLuong = soNgayDiLam + soNgayPhep + soNgayLe;
            var luongThoiGian = Math.Round(tongNgayHuongLuong * donGiaNgay, 0, MidpointRounding.AwayFromZero);

            // 2. Lương làm thêm giờ OT
            var tienOtThuong = Math.Round(otThuong * donGiaGio * 1.5m, 0, MidpointRounding.AwayFromZero);
            var tienOtNghi = Math.Round(otNghi * donGiaGio * 2.0m, 0, MidpointRounding.AwayFromZero);
            var tienOtLe = Math.Round(otLe * donGiaGio * 3.0m, 0, MidpointRounding.AwayFromZero);
            var luongLamThemGio = tienOtThuong + tienOtNghi + tienOtLe;

            // Phần tiền làm thêm giờ vượt mức 100% được miễn thuế TNCN
            var otMienThue = Math.Round((otThuong * 0.5m + otNghi * 1.0m + otLe * 2.0m) * donGiaGio, 0, MidpointRounding.AwayFromZero);

            // 3. Phụ cấp và tiền thưởng
            var phuCapAnTruaMienThue = Math.Min(nv.PhuCapAnTrua, PhuCapAnTruaMienThueMax);
            var phuCapAnTruaChiuThue = Math.Max(0m, nv.PhuCapAnTrua - PhuCapAnTruaMienThueMax);

            var phuCapTrangPhucMienThue = Math.Min(nv.PhuCapTrangPhuc, PhuCapTrangPhucMienThueThangMax);
            var phuCapTrangPhucChiuThue = Math.Max(0m, nv.PhuCapTrangPhuc - PhuCapTrangPhucMienThueThangMax);

            var phuCapChiuThue = nv.PhuCapTrachNhiem + nv.PhuCapDienThoai + phuCapAnTruaChiuThue + phuCapTrangPhucChiuThue;
            var phuCapMienThue = phuCapAnTruaMienThue + phuCapTrangPhucMienThue;

            var tongThuNhap = luongThoiGian + luongLamThemGio + nv.PhuCapAnTrua + nv.PhuCapTrachNhiem + nv.PhuCapDienThoai + nv.PhuCapTrangPhuc;

            // 4. Bảo hiểm bắt buộc
            decimal bhxhDn = 0m, bhytDn = 0m, bhtnDn = 0m, kpcdDn = 0m;
            decimal bhxhNld = 0m, bhytNld = 0m, bhtnNld = 0m;

            if (nv.DongBaoHiem && nv.LuongDongBaoHiem > 0)
            {
                var dn = TinhBaoHiemDoanhNghiep(nv.LuongDongBaoHiem, 1);
                bhxhDn = dn.BhxhDn;
                bhytDn = dn.BhytDn;
                bhtnDn = dn.BhtnDn;
                kpcdDn = dn.KpcdDn;

                var nld = TinhBaoHiemNguoiLaoDong(nv.LuongDongBaoHiem, 1);
                bhxhNld = nld.BhxhNld;
                bhytNld = nld.BhytNld;
                bhtnNld = nld.BhtnNld;
            }

            var tongBaoHiemNld = bhxhNld + bhytNld + bhtnNld;
            var tongBaoHiemDn = bhxhDn + bhytDn + bhtnDn + kpcdDn;

            // 5. Thuế TNCN
            var thueTncn = TinhThueTncn(nv, tongThuNhap, tongBaoHiemNld, otMienThue, phuCapMienThue);

            var giamTruBanThan = nv.LoaiHopDong == LoaiHopDongLaoDong.HopDongDaiHan ? GiamTruBanThanDinhMuc : 0m;
            var giamTruNpt = nv.LoaiHopDong == LoaiHopDongLaoDong.HopDongDaiHan ? nv.SoNguoiPhuThuoc * GiamTruPhuThuocDinhMuc : 0m;
            var thuNhapTinhThue = Math.Max(0m, tongThuNhap - otMienThue - phuCapMienThue - giamTruBanThan - giamTruNpt - tongBaoHiemNld);

            // 6. Thực lĩnh
            var thucLinh = tongThuNhap - tongBaoHiemNld - thueTncn - tamUng;

            var chiTiet = new ChiTietLuongNhanVien
            {
                BangLuongThangId = bangLuong.Id,
                NhanVienId = nv.Id,
                LuongThoiGian = luongThoiGian,
                LuongLamThemGio = luongLamThemGio,
                LuongOtMienThue = otMienThue,
                PhuCapChiuThue = phuCapChiuThue,
                PhuCapMienThue = phuCapMienThue,
                TienThuong = 0m,
                TongThuNhap = tongThuNhap,
                LuongDongBaoHiem = nv.LuongDongBaoHiem,
                BhxhNld = bhxhNld,
                BhytNld = bhytNld,
                BhtnNld = bhtnNld,
                TongBaoHiemNld = tongBaoHiemNld,
                BhxhDn = bhxhDn,
                BhytDn = bhytDn,
                BhtnDn = bhtnDn,
                KpcdDn = kpcdDn,
                TongBaoHiemDn = tongBaoHiemDn,
                GiamTruBanThan = giamTruBanThan,
                GiamTruNguoiPhuThuoc = giamTruNpt,
                ThuNhapTinhThue = thuNhapTinhThue,
                ThueTncnKhauTru = thueTncn,
                TamUng = tamUng,
                ThucLinh = thucLinh
            };

            chiTietList.Add(chiTiet);

            tongQuyLuong += tongThuNhap;
            tongBhDn += tongBaoHiemDn;
            tongBhNld += tongBaoHiemNld;
            tongThue += thueTncn;
            tongThucLinh += thucLinh;
        }

        await _context.ChiTietLuongNhanViens.AddRangeAsync(chiTietList);

        bangLuong.TongQuyLuong = tongQuyLuong;
        bangLuong.TongBaoHiemDnGanh = tongBhDn;
        bangLuong.TongBaoHiemNldGanh = tongBhNld;
        bangLuong.TongThueTncn = tongThue;
        bangLuong.TongThucLinh = tongThucLinh;
        bangLuong.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Tính lương thành công kỳ {KyKeToan}: {Count} nhân viên, Quỹ lương: {Tong:N0} VNĐ, Thực lĩnh: {ThucLinh:N0} VNĐ",
            kyKeToan, chiTietList.Count, tongQuyLuong, tongThucLinh);

        return await MapToViewModelAsync(bangLuong.Id);
    }

    public async Task<BangLuongThang> GhiSoBangLuongAsync(long bangLuongId)
    {
        var bangLuong = await _context.BangLuongThangs
            .Include(bl => bl.ChiTiets)
            .ThenInclude(ct => ct.NhanVien)
                .ThenInclude(nv => nv!.PhongBanEntity)
            .FirstOrDefaultAsync(bl => bl.Id == bangLuongId);

        if (bangLuong == null)
        {
            throw new InvalidOperationException($"Không tìm thấy bảng lương Id = {bangLuongId}.");
        }

        if (bangLuong.TrangThai == TrangThaiBangLuong.DaGhiSo)
        {
            throw new InvalidOperationException($"Bảng lương kỳ {bangLuong.KyKeToan} đã được ghi sổ trước đó.");
        }

        // Lấy danh mục tài khoản TT99
        var tk642 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "642");
        var tk6421 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "6421");
        var tk6422 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "6422");
        var tk154 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "154");
        var tk334 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "334");
        var tk3383 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "3383");
        var tk3384 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "3384");
        var tk3386 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "3386");
        var tk3382 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "3382");
        var tk3335 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "3335");

        if (tk642 == null || tk334 == null || tk3383 == null || tk3384 == null || tk3386 == null || tk3382 == null || tk3335 == null)
        {
            throw new InvalidOperationException("Hệ thống thiếu các tài khoản kế toán tiền lương chuẩn TT99 (642, 334, 3383, 3384, 3386, 3382, 3335). Vui lòng kiểm tra DbInitializer.");
        }

        var parts = bangLuong.KyKeToan.Split('-');
        var year = int.Parse(parts[0]);
        var month = int.Parse(parts[1]);
        var ngayCuoiThang = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        var phongBans = await _context.PhongBans.ToDictionaryAsync(p => p.Id);

        // Nhóm chi tiết bảng lương theo phòng ban để phân bổ chi phí theo bộ phận
        var chiTietGroups = bangLuong.ChiTiets
            .GroupBy(c =>
            {
                var pbId = c.NhanVien?.PhongBanId ?? c.NhanVien?.PhongBanEntity?.Id;
                PhongBan? pb = null;
                if (pbId.HasValue && phongBans.TryGetValue(pbId.Value, out var foundPb))
                {
                    pb = foundPb;
                }
                else
                {
                    pb = c.NhanVien?.PhongBanEntity;
                }

                return new
                {
                    PhongBanId = pbId,
                    TenPhongBan = pb?.TenPhongBan ?? (string.IsNullOrWhiteSpace(c.NhanVien?.PhongBan) ? "Bộ phận chung" : c.NhanVien.PhongBan),
                    LoaiPhongBan = pb?.LoaiPhongBan ?? LoaiPhongBan.QuanLy
                };
            })
            .OrderBy(g => g.Key.PhongBanId ?? 0)
            .ToList();

        // 1. Bút toán 1: Chi phí tiền lương theo phòng ban/bộ phận (Nợ 154 / 6421 / 6422 / 642 - Có 334)
        var chiTietsBtLuong = new List<ChiTietButToan>();
        foreach (var grp in chiTietGroups)
        {
            var tongLuongNhom = grp.Sum(x => x.TongThuNhap);
            if (tongLuongNhom <= 0) continue;

            var tkChiPhi = grp.Key.LoaiPhongBan switch
            {
                LoaiPhongBan.SanXuat or LoaiPhongBan.KhoaChuyenMon => tk154 ?? tk6422 ?? tk642,
                LoaiPhongBan.BanHang => tk6421 ?? tk642,
                _ => tk6422 ?? tk642
            };

            chiTietsBtLuong.Add(new ChiTietButToan
            {
                DienGiai = $"Chi phí tiền lương - {grp.Key.TenPhongBan} kỳ {bangLuong.KyKeToan}",
                TaiKhoanNoId = tkChiPhi.Id,
                TaiKhoanCoId = tk334.Id,
                SoTien = tongLuongNhom,
                PhongBanId = grp.Key.PhongBanId
            });
        }

        if (!chiTietsBtLuong.Any())
        {
            chiTietsBtLuong.Add(new ChiTietButToan
            {
                DienGiai = $"Chi phí tiền lương nhân viên kỳ {bangLuong.KyKeToan}",
                TaiKhoanNoId = (tk6422 ?? tk642).Id,
                TaiKhoanCoId = tk334.Id,
                SoTien = bangLuong.TongQuyLuong
            });
        }

        var btLuong = new ButToan
        {
            SoChungTu = $"PKT-L-{bangLuong.KyKeToan.Replace("-", "")}",
            SoChungTuGoc = bangLuong.SoChungTu,
            NgayChungTu = ngayCuoiThang,
            NgayHachToan = ngayCuoiThang,
            DienGiai = $"Hạch toán chi phí tiền lương kỳ {bangLuong.KyKeToan}",
            TongTien = bangLuong.TongQuyLuong,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = chiTietsBtLuong
        };

        var (ok1, err1, bt1) = await _butToanService.TaoMoiAsync(btLuong);
        if (!ok1 || bt1 == null) throw new InvalidOperationException($"Lỗi tạo bút toán chi phí lương: {err1}");
        var (gso1, errGso1) = await _butToanService.GhiSoAsync(bt1.Id);
        if (!gso1) throw new InvalidOperationException($"Lỗi ghi sổ bút toán chi phí lương: {errGso1}");

        // 2. Bút toán 2: Bảo hiểm & KPCĐ Doanh nghiệp gánh 23.5% phân bổ theo phòng ban
        var tongBhxhDn = bangLuong.ChiTiets.Sum(c => c.BhxhDn);
        var tongBhytDn = bangLuong.ChiTiets.Sum(c => c.BhytDn);
        var tongBhtnDn = bangLuong.ChiTiets.Sum(c => c.BhtnDn);
        var tongKpcdDn = bangLuong.ChiTiets.Sum(c => c.KpcdDn);
        var tongBhDn = tongBhxhDn + tongBhytDn + tongBhtnDn + tongKpcdDn;

        ButToan? bt2 = null;
        if (tongBhDn > 0)
        {
            var chiTietsBt2 = new List<ChiTietButToan>();
            foreach (var grp in chiTietGroups)
            {
                var tkChiPhi = grp.Key.LoaiPhongBan switch
                {
                    LoaiPhongBan.SanXuat or LoaiPhongBan.KhoaChuyenMon => tk154 ?? tk6422 ?? tk642,
                    LoaiPhongBan.BanHang => tk6421 ?? tk642,
                    _ => tk6422 ?? tk642
                };

                var bhxhNhom = grp.Sum(c => c.BhxhDn);
                var bhytNhom = grp.Sum(c => c.BhytDn);
                var bhtnNhom = grp.Sum(c => c.BhtnDn);
                var kpcdNhom = grp.Sum(c => c.KpcdDn);

                if (bhxhNhom > 0)
                    chiTietsBt2.Add(new() { DienGiai = $"Trích BHXH DN gánh (17.5%) - {grp.Key.TenPhongBan} kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tkChiPhi.Id, TaiKhoanCoId = tk3383.Id, SoTien = bhxhNhom, PhongBanId = grp.Key.PhongBanId });
                if (bhytNhom > 0)
                    chiTietsBt2.Add(new() { DienGiai = $"Trích BHYT DN gánh (3.0%) - {grp.Key.TenPhongBan} kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tkChiPhi.Id, TaiKhoanCoId = tk3384.Id, SoTien = bhytNhom, PhongBanId = grp.Key.PhongBanId });
                if (bhtnNhom > 0)
                    chiTietsBt2.Add(new() { DienGiai = $"Trích BHTN DN gánh (1.0%) - {grp.Key.TenPhongBan} kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tkChiPhi.Id, TaiKhoanCoId = tk3386.Id, SoTien = bhtnNhom, PhongBanId = grp.Key.PhongBanId });
                if (kpcdNhom > 0)
                    chiTietsBt2.Add(new() { DienGiai = $"Trích KPCĐ DN gánh (2.0%) - {grp.Key.TenPhongBan} kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tkChiPhi.Id, TaiKhoanCoId = tk3382.Id, SoTien = kpcdNhom, PhongBanId = grp.Key.PhongBanId });
            }

            var btBhdn = new ButToan
            {
                SoChungTu = $"PKT-BHDN-{bangLuong.KyKeToan.Replace("-", "")}",
                SoChungTuGoc = bangLuong.SoChungTu,
                NgayChungTu = ngayCuoiThang,
                NgayHachToan = ngayCuoiThang,
                DienGiai = $"Trích bảo hiểm và KPCĐ phần DN gánh kỳ {bangLuong.KyKeToan}",
                TongTien = tongBhDn,
                TrangThai = TrangThaiButToan.ChuaGhiSo,
                ChiTietButToans = chiTietsBt2
            };

            var (ok2, err2, resBt2) = await _butToanService.TaoMoiAsync(btBhdn);
            if (!ok2 || resBt2 == null) throw new InvalidOperationException($"Lỗi tạo bút toán bảo hiểm DN: {err2}");
            var (gso2, errGso2) = await _butToanService.GhiSoAsync(resBt2.Id);
            if (!gso2) throw new InvalidOperationException($"Lỗi ghi sổ bút toán bảo hiểm DN: {errGso2}");
            bt2 = resBt2;
        }

        // 3. Bút toán 3: Khấu trừ lương NLĐ (Bảo hiểm 10.5% & Thuế TNCN) (Nợ 334 / Có 3383, 3384, 3386, 3335)
        var tongBhxhNld = bangLuong.ChiTiets.Sum(c => c.BhxhNld);
        var tongBhytNld = bangLuong.ChiTiets.Sum(c => c.BhytNld);
        var tongBhtnNld = bangLuong.ChiTiets.Sum(c => c.BhtnNld);
        var tongThueTncn = bangLuong.TongThueTncn;
        var tongKhauTru = tongBhxhNld + tongBhytNld + tongBhtnNld + tongThueTncn;

        ButToan? bt3 = null;
        if (tongKhauTru > 0)
        {
            var chiTietsBt3 = new List<ChiTietButToan>();
            if (tongBhxhNld > 0)
                chiTietsBt3.Add(new() { DienGiai = $"Khấu trừ BHXH NLĐ (8.0%) kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tk334.Id, TaiKhoanCoId = tk3383.Id, SoTien = tongBhxhNld });
            if (tongBhytNld > 0)
                chiTietsBt3.Add(new() { DienGiai = $"Khấu trừ BHYT NLĐ (1.5%) kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tk334.Id, TaiKhoanCoId = tk3384.Id, SoTien = tongBhytNld });
            if (tongBhtnNld > 0)
                chiTietsBt3.Add(new() { DienGiai = $"Khấu trừ BHTN NLĐ (1.0%) kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tk334.Id, TaiKhoanCoId = tk3386.Id, SoTien = tongBhtnNld });
            if (tongThueTncn > 0)
                chiTietsBt3.Add(new() { DienGiai = $"Khấu trừ thuế TNCN tại nguồn kỳ {bangLuong.KyKeToan}", TaiKhoanNoId = tk334.Id, TaiKhoanCoId = tk3335.Id, SoTien = tongThueTncn });

            var btKhauTru = new ButToan
            {
                SoChungTu = $"PKT-KT-{bangLuong.KyKeToan.Replace("-", "")}",
                SoChungTuGoc = bangLuong.SoChungTu,
                NgayChungTu = ngayCuoiThang,
                NgayHachToan = ngayCuoiThang,
                DienGiai = $"Khấu trừ bảo hiểm và thuế TNCN vào lương kỳ {bangLuong.KyKeToan}",
                TongTien = tongKhauTru,
                TrangThai = TrangThaiButToan.ChuaGhiSo,
                ChiTietButToans = chiTietsBt3
            };

            var (ok3, err3, resBt3) = await _butToanService.TaoMoiAsync(btKhauTru);
            if (!ok3 || resBt3 == null) throw new InvalidOperationException($"Lỗi tạo bút toán khấu trừ lương: {err3}");
            var (gso3, errGso3) = await _butToanService.GhiSoAsync(resBt3.Id);
            if (!gso3) throw new InvalidOperationException($"Lỗi ghi sổ bút toán khấu trừ lương: {errGso3}");
            bt3 = resBt3;
        }

        bangLuong.ButToanChiPhiLuongId = bt1.Id;
        bangLuong.ButToanBaoHiemDnId = bt2?.Id;
        bangLuong.ButToanKhauTruLuongId = bt3?.Id;
        bangLuong.TrangThai = TrangThaiBangLuong.DaGhiSo;
        bangLuong.NgayGhiSo = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ghi sổ thành công bảng lương kỳ {KyKeToan}: BT Lương = {Bt1}, BT BH DN = {Bt2}, BT Khấu trừ = {Bt3}",
            bangLuong.KyKeToan, bt1.SoChungTu, bt2?.SoChungTu, bt3?.SoChungTu);

        return bangLuong;
    }

    public async Task<PayslipViewModel> LayPhieuLuongNhanVienAsync(long bangLuongId, long nhanVienId)
    {
        var bangLuong = await _context.BangLuongThangs
            .Include(bl => bl.ChiTiets)
            .ThenInclude(ct => ct.NhanVien)
            .FirstOrDefaultAsync(bl => bl.Id == bangLuongId);

        if (bangLuong == null)
        {
            throw new InvalidOperationException($"Không tìm thấy bảng lương Id = {bangLuongId}.");
        }

        var ct = bangLuong.ChiTiets.FirstOrDefault(c => c.NhanVienId == nhanVienId);
        if (ct == null || ct.NhanVien == null)
        {
            throw new InvalidOperationException($"Không tìm thấy chi tiết lương của nhân viên Id = {nhanVienId} trong kỳ {bangLuong.KyKeToan}.");
        }

        var cc = await _context.ChiTietChamCongs
            .Include(c => c.BangChamCongThang)
            .FirstOrDefaultAsync(c => c.BangChamCongThang!.KyKeToan == bangLuong.KyKeToan && c.NhanVienId == nhanVienId);

        return new PayslipViewModel
        {
            KyKeToan = bangLuong.KyKeToan,
            MaNhanVien = ct.NhanVien.MaNhanVien,
            HoTen = ct.NhanVien.HoTen,
            PhongBan = ct.NhanVien.PhongBan,
            ChucVu = ct.NhanVien.ChucVu,
            SoNguoiPhuThuoc = ct.NhanVien.SoNguoiPhuThuoc,
            SoNgayCongThucTe = cc?.SoNgayDiLam ?? 0m,
            SoNgayCongChuan = bangLuong.SoNgayCongChuan,
            LuongCoBan = ct.NhanVien.LuongCoBan,
            LuongThoiGian = ct.LuongThoiGian,
            LuongLamThemGio = ct.LuongLamThemGio,
            LuongOtMienThue = ct.LuongOtMienThue,
            PhuCapAnTrua = ct.NhanVien.PhuCapAnTrua,
            PhuCapKhac = ct.NhanVien.PhuCapTrachNhiem + ct.NhanVien.PhuCapDienThoai + ct.NhanVien.PhuCapTrangPhuc,
            TienThuong = ct.TienThuong,
            TongThuNhap = ct.TongThuNhap,
            BhxhNld = ct.BhxhNld,
            BhytNld = ct.BhytNld,
            BhtnNld = ct.BhtnNld,
            TongBaoHiemNld = ct.TongBaoHiemNld,
            ThueTncnKhauTru = ct.ThueTncnKhauTru,
            TamUng = ct.TamUng,
            ThucLinh = ct.ThucLinh,
            SoTaiKhoanNganHang = ct.NhanVien.SoTaiKhoanNganHang,
            TenNganHang = ct.NhanVien.TenNganHang
        };
    }

    public async Task<List<BangLuongThangItemViewModel>> LayDanhSachBangLuongAsync()
    {
        return await _context.BangLuongThangs
            .OrderByDescending(bl => bl.KyKeToan)
            .Select(bl => new BangLuongThangItemViewModel
            {
                Id = bl.Id,
                SoChungTu = bl.SoChungTu,
                KyKeToan = bl.KyKeToan,
                NgayLap = bl.NgayLap,
                TongQuyLuong = bl.TongQuyLuong,
                TongBaoHiemDnGanh = bl.TongBaoHiemDnGanh,
                TongBaoHiemNldGanh = bl.TongBaoHiemNldGanh,
                TongThueTncn = bl.TongThueTncn,
                TongThucLinh = bl.TongThucLinh,
                TrangThai = bl.TrangThai,
                SoNhanVien = bl.ChiTiets.Count
            })
            .ToListAsync();
    }

    private async Task<BangLuongChiTietViewModel> MapToViewModelAsync(long bangLuongId)
    {
        var bl = await _context.BangLuongThangs
            .Include(b => b.ButToanChiPhiLuong)
            .Include(b => b.ButToanBaoHiemDn)
            .Include(b => b.ButToanKhauTruLuong)
            .Include(b => b.ChiTiets)
            .ThenInclude(ct => ct.NhanVien)
                .ThenInclude(nv => nv!.PhongBanEntity)
            .FirstAsync(b => b.Id == bangLuongId);

        return MapToViewModel(bl);
    }

    private static BangLuongChiTietViewModel MapToViewModel(BangLuongThang bl)
    {
        return new BangLuongChiTietViewModel
        {
            BangLuongId = bl.Id,
            SoChungTu = bl.SoChungTu,
            KyKeToan = bl.KyKeToan,
            NgayLap = bl.NgayLap,
            SoNgayCongChuan = bl.SoNgayCongChuan,
            TrangThai = bl.TrangThai,
            TongQuyLuong = bl.TongQuyLuong,
            TongBaoHiemDnGanh = bl.TongBaoHiemDnGanh,
            TongBaoHiemNldGanh = bl.TongBaoHiemNldGanh,
            TongThueTncn = bl.TongThueTncn,
            TongThucLinh = bl.TongThucLinh,
            SoButToanLuong = bl.ButToanChiPhiLuong?.SoChungTu,
            SoButToanBhDn = bl.ButToanBaoHiemDn?.SoChungTu,
            SoButToanKhauTru = bl.ButToanKhauTruLuong?.SoChungTu,
            ChiTiets = bl.ChiTiets.Select(ct => new DongLuongNhanVienViewModel
            {
                NhanVienId = ct.NhanVienId,
                MaNhanVien = ct.NhanVien?.MaNhanVien ?? string.Empty,
                HoTen = ct.NhanVien?.HoTen ?? string.Empty,
                PhongBan = ct.NhanVien?.PhongBanEntity?.TenPhongBan ?? ct.NhanVien?.PhongBan,
                ChucVu = ct.NhanVien?.ChucVu,
                LoaiHopDong = ct.NhanVien?.LoaiHopDong ?? LoaiHopDongLaoDong.HopDongDaiHan,
                LuongCoBan = ct.NhanVien?.LuongCoBan ?? 0m,
                LuongThoiGian = ct.LuongThoiGian,
                LuongLamThemGio = ct.LuongLamThemGio,
                LuongOtMienThue = ct.LuongOtMienThue,
                PhuCapChiuThue = ct.PhuCapChiuThue,
                PhuCapMienThue = ct.PhuCapMienThue,
                TienThuong = ct.TienThuong,
                TongThuNhap = ct.TongThuNhap,
                LuongDongBaoHiem = ct.LuongDongBaoHiem,
                BhxhNld = ct.BhxhNld,
                BhytNld = ct.BhytNld,
                BhtnNld = ct.BhtnNld,
                TongBaoHiemNld = ct.TongBaoHiemNld,
                BhxhDn = ct.BhxhDn,
                BhytDn = ct.BhytDn,
                BhtnDn = ct.BhtnDn,
                KpcdDn = ct.KpcdDn,
                TongBaoHiemDn = ct.TongBaoHiemDn,
                GiamTruBanThan = ct.GiamTruBanThan,
                GiamTruNguoiPhuThuoc = ct.GiamTruNguoiPhuThuoc,
                ThuNhapTinhThue = ct.ThuNhapTinhThue,
                ThueTncnKhauTru = ct.ThueTncnKhauTru,
                TamUng = ct.TamUng,
                ThucLinh = ct.ThucLinh
            }).ToList()
        };
    }
}
