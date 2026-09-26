using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class TaiSanService : ITaiSanService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ILogger<TaiSanService> _logger;

    public TaiSanService(
        AppDbContext context,
        IButToanService butToanService,
        ILogger<TaiSanService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _logger = logger;
    }

    public async Task<TaiSanCoDinh> KhaiBaoTaiSanAsync(TaiSanCreateViewModel model)
    {
        // Chốt chặn 3: Tiêu chuẩn ghi nhận TSCĐ theo TT 45/2013/TT-BTC
        // Điều kiện: Nguyên giá >= 30.000.000 VNĐ và Thời gian sử dụng > 1 năm (12 tháng)
        if (model.LoaiTaiSan == LoaiTaiSan.TaiSanCoDinh)
        {
            if (model.NguyenGia < 30_000_000m)
            {
                throw new InvalidOperationException(
                    $"Tài sản '{model.TenTaiSan}' có nguyên giá {model.NguyenGia:N0} VNĐ (< 30.000.000 VNĐ), không đủ tiêu chuẩn ghi nhận TSCĐ hữu hình (TK 211) theo Điều 3 Thông tư 45/2013/TT-BTC. Bắt buộc phải chuyển sang phân loại Công cụ dụng cụ (TK 242)!");
            }

            if (model.ThoiGianSuDungThang <= 12)
            {
                throw new InvalidOperationException(
                    $"Thời gian sử dụng của TSCĐ hữu hình phải trên 12 tháng theo Thông tư 45/2013/TT-BTC. Giá trị khai báo: {model.ThoiGianSuDungThang} tháng.");
            }
        }

        // Chốt chặn 4: Giới hạn thời gian phân bổ CCDC theo TT 78/2014 & TT 96/2015/TT-BTC
        // Thời gian phân bổ chi phí trả trước tối đa không quá 36 tháng (3 năm)
        if (model.LoaiTaiSan == LoaiTaiSan.CongCuDungCu)
        {
            if (model.ThoiGianSuDungThang > 36)
            {
                throw new InvalidOperationException(
                    $"Thời gian phân bổ Công cụ dụng cụ / Chi phí trả trước tối đa không quá 36 tháng theo quy định tại Thông tư 96/2015/TT-BTC và Thông tư 78/2014/TT-BTC. Khai báo: {model.ThoiGianSuDungThang} tháng.");
            }
        }

        var tonTaiMa = await _context.TaiSanCoDinhs.AnyAsync(t => t.MaTaiSan == model.MaTaiSan);
        if (tonTaiMa)
        {
            throw new InvalidOperationException($"Mã tài sản '{model.MaTaiSan}' đã tồn tại trong hệ thống.");
        }

        var mucKhauHaoThang = Math.Round(model.NguyenGia / model.ThoiGianSuDungThang, 4);

        var taiSan = new TaiSanCoDinh
        {
            MaTaiSan = model.MaTaiSan.Trim(),
            TenTaiSan = model.TenTaiSan.Trim(),
            LoaiTaiSan = model.LoaiTaiSan,
            NgayGhiTang = model.NgayGhiTang,
            NgayBatDauKhauHao = model.NgayBatDauKhauHao,
            NguyenGia = model.NguyenGia,
            ThoiGianSuDungThang = model.ThoiGianSuDungThang,
            GiaTriDaKhauHao = 0m,
            GiaTriConLai = model.NguyenGia,
            MucKhauHaoThang = mucKhauHaoThang,
            TaiKhoanNguyenGiaId = model.TaiKhoanNguyenGiaId,
            TaiKhoanKhauHaoId = model.TaiKhoanKhauHaoId,
            TaiKhoanChiPhiId = model.TaiKhoanChiPhiId,
            BoPhanSuDung = model.BoPhanSuDung,
            TrangThai = TrangThaiTaiSan.DangSuDung,
            GhiChu = model.GhiChu,
            NgayTao = DateTime.UtcNow
        };

        await _context.TaiSanCoDinhs.AddAsync(taiSan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Khai báo thành công {Loai} {MaTaiSan} - {TenTaiSan}, Nguyên giá: {NguyenGia:N0} VNĐ",
            taiSan.LoaiTaiSan, taiSan.MaTaiSan, taiSan.TenTaiSan, taiSan.NguyenGia);

        return taiSan;
    }

    public async Task<TaiSanListViewModel> TimKiemTaiSanAsync(LoaiTaiSan? loai, TrangThaiTaiSan? trangThai, string? tuKhoa)
    {
        var query = _context.TaiSanCoDinhs
            .Include(t => t.TaiKhoanNguyenGia)
            .Include(t => t.TaiKhoanKhauHao)
            .Include(t => t.TaiKhoanChiPhi)
            .AsQueryable();

        if (loai.HasValue)
        {
            query = query.Where(t => t.LoaiTaiSan == loai.Value);
        }

        if (trangThai.HasValue)
        {
            query = query.Where(t => t.TrangThai == trangThai.Value);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            query = query.Where(t => t.MaTaiSan.Contains(tuKhoa) || t.TenTaiSan.Contains(tuKhoa));
        }

        var items = await query
            .OrderBy(t => t.LoaiTaiSan)
            .ThenBy(t => t.MaTaiSan)
            .Select(t => new TaiSanItemViewModel
            {
                Id = t.Id,
                MaTaiSan = t.MaTaiSan,
                TenTaiSan = t.TenTaiSan,
                LoaiTaiSan = t.LoaiTaiSan,
                NgayGhiTang = t.NgayGhiTang,
                NgayBatDauKhauHao = t.NgayBatDauKhauHao,
                NguyenGia = t.NguyenGia,
                ThoiGianSuDungThang = t.ThoiGianSuDungThang,
                GiaTriDaKhauHao = t.GiaTriDaKhauHao,
                GiaTriConLai = t.GiaTriConLai,
                MucKhauHaoThang = t.MucKhauHaoThang,
                MaTaiKhoanNguyenGia = t.TaiKhoanNguyenGia != null ? t.TaiKhoanNguyenGia.MaTaiKhoan : null,
                MaTaiKhoanKhauHao = t.TaiKhoanKhauHao != null ? t.TaiKhoanKhauHao.MaTaiKhoan : null,
                MaTaiKhoanChiPhi = t.TaiKhoanChiPhi != null ? t.TaiKhoanChiPhi.MaTaiKhoan : null,
                BoPhanSuDung = t.BoPhanSuDung,
                TrangThai = t.TrangThai
            })
            .ToListAsync();

        return new TaiSanListViewModel
        {
            LoaiTaiSan = loai,
            TrangThai = trangThai,
            TuKhoa = tuKhoa,
            DanhSach = items
        };
    }

    public async Task<TaiSanCoDinh?> LayChiTietTaiSanAsync(long id)
    {
        return await _context.TaiSanCoDinhs
            .Include(t => t.TaiKhoanNguyenGia)
            .Include(t => t.TaiKhoanKhauHao)
            .Include(t => t.TaiKhoanChiPhi)
            .Include(t => t.BangTinhKhauHaos)
                .ThenInclude(b => b.ButToan)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<BangKhauHaoKyViewModel> XemBangKhauHaoKyAsync(string kyKeToan)
    {
        var (year, month, tuNgay, denNgay) = ParseKyKeToan(kyKeToan);

        // Kiểm tra xem kỳ này đã được chạy và lưu vào BangTinhKhauHao chưa
        var bangDaLuu = await _context.BangTinhKhauHaos
            .Include(b => b.TaiSanCoDinh)
                .ThenInclude(t => t!.TaiKhoanChiPhi)
            .Include(b => b.TaiSanCoDinh)
                .ThenInclude(t => t!.TaiKhoanKhauHao)
            .Include(b => b.ButToan)
            .Where(b => b.KyKeToan == kyKeToan)
            .ToListAsync();

        if (bangDaLuu.Any())
        {
            var chiTiets = bangDaLuu.Select(b => new DongKhauHaoViewModel
            {
                TaiSanCoDinhId = b.TaiSanCoDinhId,
                MaTaiSan = b.TaiSanCoDinh?.MaTaiSan ?? "",
                TenTaiSan = b.TaiSanCoDinh?.TenTaiSan ?? "",
                LoaiTaiSan = b.TaiSanCoDinh?.LoaiTaiSan ?? LoaiTaiSan.TaiSanCoDinh,
                NguyenGia = b.NguyenGia,
                KhauHaoKyNay = b.SoTienKhauHao,
                LuyKeKhauHao = b.LuyKeKhauHao,
                GiaTriConLai = b.GiaTriConLai,
                MaTaiKhoanChiPhi = b.TaiSanCoDinh?.TaiKhoanChiPhi?.MaTaiKhoan ?? "642",
                MaTaiKhoanKhauHao = b.TaiSanCoDinh?.TaiKhoanKhauHao?.MaTaiKhoan ?? (b.TaiSanCoDinh?.LoaiTaiSan == LoaiTaiSan.TaiSanCoDinh ? "2141" : "242"),
                SoButToan = b.ButToan?.SoChungTu
            }).ToList();

            return new BangKhauHaoKyViewModel
            {
                KyKeToan = kyKeToan,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                DaChayKhauHao = true,
                TongKhauHaoKy = chiTiets.Sum(c => c.KhauHaoKyNay),
                ChiTiets = chiTiets
            };
        }

        // Nếu chưa chạy, mô phỏng dự tính số liệu khấu hao của kỳ
        var taiSans = await _context.TaiSanCoDinhs
            .Include(t => t.TaiKhoanChiPhi)
            .Include(t => t.TaiKhoanKhauHao)
            .Where(t => t.TrangThai == TrangThaiTaiSan.DangSuDung
                        && t.NgayBatDauKhauHao <= denNgay
                        && t.GiaTriConLai > 0)
            .OrderBy(t => t.MaTaiSan)
            .ToListAsync();

        var danhSachDuTinh = new List<DongKhauHaoViewModel>();

        foreach (var ts in taiSans)
        {
            var khauHao = TinhSoTienKhauHaoTrongKy(ts, year, month, denNgay);
            var luyKeMoi = ts.GiaTriDaKhauHao + khauHao;
            var conLaiMoi = Math.Max(0m, ts.GiaTriConLai - khauHao);

            danhSachDuTinh.Add(new DongKhauHaoViewModel
            {
                TaiSanCoDinhId = ts.Id,
                MaTaiSan = ts.MaTaiSan,
                TenTaiSan = ts.TenTaiSan,
                LoaiTaiSan = ts.LoaiTaiSan,
                NguyenGia = ts.NguyenGia,
                KhauHaoKyNay = khauHao,
                LuyKeKhauHao = luyKeMoi,
                GiaTriConLai = conLaiMoi,
                MaTaiKhoanChiPhi = ts.TaiKhoanChiPhi?.MaTaiKhoan ?? "642",
                MaTaiKhoanKhauHao = ts.TaiKhoanKhauHao?.MaTaiKhoan ?? (ts.LoaiTaiSan == LoaiTaiSan.TaiSanCoDinh ? "2141" : "242"),
                SoButToan = null
            });
        }

        return new BangKhauHaoKyViewModel
        {
            KyKeToan = kyKeToan,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            DaChayKhauHao = false,
            TongKhauHaoKy = danhSachDuTinh.Sum(c => c.KhauHaoKyNay),
            ChiTiets = danhSachDuTinh
        };
    }

    public async Task<List<BangTinhKhauHao>> ChayVaGhiSoKhauHaoKyAsync(string kyKeToan)
    {
        var daTonTai = await _context.BangTinhKhauHaos.AnyAsync(b => b.KyKeToan == kyKeToan);
        if (daTonTai)
        {
            throw new InvalidOperationException($"Kỳ kế toán {kyKeToan} đã được trích khấu hao trước đó!");
        }

        var (year, month, tuNgay, denNgay) = ParseKyKeToan(kyKeToan);

        var taiSans = await _context.TaiSanCoDinhs
            .Include(t => t.TaiKhoanChiPhi)
            .Include(t => t.TaiKhoanKhauHao)
            .Include(t => t.TaiKhoanNguyenGia)
            .Where(t => t.TrangThai == TrangThaiTaiSan.DangSuDung
                        && t.NgayBatDauKhauHao <= denNgay
                        && t.GiaTriConLai > 0)
            .OrderBy(t => t.MaTaiSan)
            .ToListAsync();

        if (!taiSans.Any())
        {
            throw new InvalidOperationException($"Không có tài sản cố định hoặc CCDC nào cần trích khấu hao trong kỳ {kyKeToan}.");
        }

        // Lấy tài khoản kế toán mặc định nếu chưa gán
        var tk2141 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "2141")
                     ?? await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "214");
        var tk242 = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "242");
        var tk642 = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == "642");

        var bangList = new List<BangTinhKhauHao>();
        var chiTietButToans = new List<ChiTietButToan>();

        foreach (var ts in taiSans)
        {
            var khauHao = TinhSoTienKhauHaoTrongKy(ts, year, month, denNgay);
            if (khauHao <= 0) continue;

            ts.GiaTriDaKhauHao += khauHao;
            ts.GiaTriConLai = Math.Max(0m, ts.GiaTriConLai - khauHao);

            if (ts.GiaTriConLai <= 0)
            {
                ts.TrangThai = TrangThaiTaiSan.DaKhauHaoHet;
            }

            var bang = new BangTinhKhauHao
            {
                TaiSanCoDinhId = ts.Id,
                KyKeToan = kyKeToan,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                NguyenGia = ts.NguyenGia,
                SoTienKhauHao = khauHao,
                LuyKeKhauHao = ts.GiaTriDaKhauHao,
                GiaTriConLai = ts.GiaTriConLai,
                NgayTao = DateTime.UtcNow
            };
            bangList.Add(bang);

            // Xác định cặp tài khoản Nợ / Có
            // TSCĐ: Nợ 642 / Có 2141 (hoặc TK khấu hao đã chọn)
            // CCDC: Nợ 642 / Có 242
            long tkNoId = ts.TaiKhoanChiPhiId > 0 ? ts.TaiKhoanChiPhiId : tk642.Id;
            long tkCoId;

            if (ts.LoaiTaiSan == LoaiTaiSan.TaiSanCoDinh)
            {
                tkCoId = ts.TaiKhoanKhauHaoId.HasValue && ts.TaiKhoanKhauHaoId.Value > 0
                    ? ts.TaiKhoanKhauHaoId.Value
                    : tk2141.Id;
            }
            else
            {
                // CCDC: Có 242 (Chi phí trả trước)
                tkCoId = ts.TaiKhoanNguyenGiaId > 0 ? ts.TaiKhoanNguyenGiaId : tk242.Id;
            }

            chiTietButToans.Add(new ChiTietButToan
            {
                DienGiai = ts.LoaiTaiSan == LoaiTaiSan.TaiSanCoDinh
                    ? $"Trích khấu hao TSCĐ {ts.MaTaiSan} - {ts.TenTaiSan} kỳ {kyKeToan}"
                    : $"Phân bổ CCDC {ts.MaTaiSan} - {ts.TenTaiSan} kỳ {kyKeToan}",
                TaiKhoanNoId = tkNoId,
                TaiKhoanCoId = tkCoId,
                SoTien = khauHao
            });
        }

        if (!bangList.Any())
        {
            throw new InvalidOperationException($"Tổng số tiền trích khấu hao trong kỳ {kyKeToan} bằng 0.");
        }

        // Sinh Bút toán Sổ Cái Core GL (Nợ 642 / Có 2141 & Có 242)
        var tongKhauHao = chiTietButToans.Sum(c => c.SoTien);
        var soChungTu = $"KH-{kyKeToan.Replace("-", "")}";

        var butToan = new ButToan
        {
            SoChungTu = soChungTu,
            SoChungTuGoc = $"BKH-{kyKeToan}",
            NgayChungTu = denNgay,
            NgayHachToan = denNgay,
            DienGiai = $"Bảng trích khấu hao TSCĐ & phân bổ CCDC kỳ {kyKeToan}",
            TongTien = tongKhauHao,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = chiTietButToans
        };

        var (taoBtOk, loiTaoBt, btTao) = await _butToanService.TaoMoiAsync(butToan);
        if (!taoBtOk || btTao == null)
        {
            throw new InvalidOperationException($"Lỗi tạo bút toán khấu hao: {loiTaoBt}");
        }

        var (ghiSoOk, loiGhiSo) = await _butToanService.GhiSoAsync(btTao.Id);
        if (!ghiSoOk)
        {
            throw new InvalidOperationException($"Lỗi ghi sổ bút toán khấu hao: {loiGhiSo}");
        }

        foreach (var b in bangList)
        {
            b.ButToanId = btTao.Id;
        }

        await _context.BangTinhKhauHaos.AddRangeAsync(bangList);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Hoàn tất trích khấu hao kỳ {KyKeToan}: {Count} tài sản, Tổng tiền: {Tong:N0} VNĐ, Bút toán: {SoChungTu}",
            kyKeToan, bangList.Count, tongKhauHao, btTao.SoChungTu);

        return bangList;
    }

    private static (int Year, int Month, DateTime TuNgay, DateTime DenNgay) ParseKyKeToan(string kyKeToan)
    {
        var parts = kyKeToan.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var month) || month < 1 || month > 12)
        {
            throw new ArgumentException($"Định dạng kỳ kế toán '{kyKeToan}' không hợp lệ. Kỳ kế toán phải có định dạng YYYY-MM (Ví dụ: 2026-09).");
        }

        var tuNgay = new DateTime(year, month, 1);
        var denNgay = tuNgay.AddMonths(1).AddDays(-1);
        return (year, month, tuNgay, denNgay);
    }

    /// <summary>
    /// Thuật toán Khấu hao Đường thẳng (Straight-line) theo TT 45/2013/TT-BTC:
    /// - Tháng tròn: NguyenGia / ThoiGianSuDungThang
    /// - Tháng đầu lẻ ngày: Tính chính xác theo số ngày sử dụng thực tế từ NgayBatDauKhauHao đến cuối tháng
    /// - Điểm dừng: Không trích vượt quá GiaTriConLai
    /// </summary>
    private static decimal TinhSoTienKhauHaoTrongKy(TaiSanCoDinh ts, int year, int month, DateTime denNgay)
    {
        if (ts.GiaTriConLai <= 0) return 0m;
        if (ts.NgayBatDauKhauHao > denNgay) return 0m;

        decimal mucKhauHaoThang = ts.MucKhauHaoThang;
        if (mucKhauHaoThang <= 0 && ts.ThoiGianSuDungThang > 0)
        {
            mucKhauHaoThang = Math.Round(ts.NguyenGia / ts.ThoiGianSuDungThang, 4);
        }

        decimal khauHao = mucKhauHaoThang;

        // Xử lý tháng đầu tiên nếu bắt đầu trích lẻ ngày
        if (ts.NgayBatDauKhauHao.Year == year && ts.NgayBatDauKhauHao.Month == month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var usedDays = daysInMonth - ts.NgayBatDauKhauHao.Day + 1;
            if (usedDays < daysInMonth)
            {
                khauHao = Math.Round(mucKhauHaoThang * usedDays / daysInMonth, 4);
            }
        }

        // Không bao giờ trích vượt quá giá trị còn lại
        khauHao = Math.Min(khauHao, ts.GiaTriConLai);

        return khauHao;
    }
}
