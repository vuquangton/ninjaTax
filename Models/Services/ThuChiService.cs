using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class ThuChiService : IThuChiService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ICongNoService _congNoService;
    private readonly ILogger<ThuChiService> _logger;

    public ThuChiService(
        AppDbContext context,
        IButToanService butToanService,
        ICongNoService congNoService,
        ILogger<ThuChiService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _congNoService = congNoService;
        _logger = logger;
    }

    public async Task<string> SinhSoChungTuAsync(LoaiChungTuThuChi loaiChungTu, int nam)
    {
        var prefix = loaiChungTu switch
        {
            LoaiChungTuThuChi.ThuTienMat => $"PT-{nam}-",
            LoaiChungTuThuChi.ChiTienMat => $"PC-{nam}-",
            LoaiChungTuThuChi.BaoCoNganHang => $"BC-{nam}-",
            LoaiChungTuThuChi.UyNhiemChi => $"UNC-{nam}-",
            _ => $"TC-{nam}-"
        };

        var maxNumber = await _context.ChungTuThuChis
            .Where(c => c.SoChungTu.StartsWith(prefix))
            .Select(c => c.SoChungTu.Substring(prefix.Length))
            .ToListAsync();

        var maxSeq = 0;
        foreach (var numStr in maxNumber)
        {
            if (int.TryParse(numStr, out var seq) && seq > maxSeq)
            {
                maxSeq = seq;
            }
        }

        return $"{prefix}{(maxSeq + 1):D5}";
    }

    public async Task<decimal> TinhTonQuyKhaDungAsync(DateTime denNgay, long? taiKhoanNganHangId = null)
    {
        var endOfDay = denNgay.Date.AddDays(1).AddTicks(-1);

        if (taiKhoanNganHangId.HasValue)
        {
            var tk = await _context.TaiKhoanNganHangs.FindAsync(taiKhoanNganHangId.Value);
            var soDuDau = tk?.SoDuBanDau ?? 0m;

            var tongThu = await _context.ChungTuThuChis
                .Where(c => c.TaiKhoanNganHangId == taiKhoanNganHangId.Value
                            && c.LoaiChungTu == LoaiChungTuThuChi.BaoCoNganHang
                            && c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan <= endOfDay)
                .SumAsync(c => c.TongTien);

            var tongChi = await _context.ChungTuThuChis
                .Where(c => c.TaiKhoanNganHangId == taiKhoanNganHangId.Value
                            && c.LoaiChungTu == LoaiChungTuThuChi.UyNhiemChi
                            && c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan <= endOfDay)
                .SumAsync(c => c.TongTien);

            return soDuDau + tongThu - tongChi;
        }
        else
        {
            // Tiền mặt (TK 1111)
            var tongThu = await _context.ChungTuThuChis
                .Where(c => c.LoaiChungTu == LoaiChungTuThuChi.ThuTienMat
                            && c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan <= endOfDay)
                .SumAsync(c => c.TongTien);

            var tongChi = await _context.ChungTuThuChis
                .Where(c => c.LoaiChungTu == LoaiChungTuThuChi.ChiTienMat
                            && c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan <= endOfDay)
                .SumAsync(c => c.TongTien);

            return tongThu - tongChi;
        }
    }

    public async Task<ChungTuThuChi> TaoChungTuThuChiAsync(ThuChiCreateViewModel model)
    {
        if (model.ChiTiets == null || !model.ChiTiets.Any())
        {
            throw new ArgumentException("Chứng từ phải có ít nhất một dòng hạch toán định khoản.");
        }

        var nam = model.NgayChungTu.Year;
        var soChungTu = await SinhSoChungTuAsync(model.LoaiChungTu, nam);
        var tongTien = model.ChiTiets.Sum(c => c.SoTien);

        var viPham20Tr = false;

        // Bẫy 20 Triệu: Thanh toán hóa đơn mua hàng >= 20.000.000 VNĐ bằng tiền mặt
        if (model.LoaiChungTu == LoaiChungTuThuChi.ChiTienMat)
        {
            if (model.HoaDonMuaHangId.HasValue)
            {
                var hdMua = await _context.HoaDonMuaHangs.FindAsync(model.HoaDonMuaHangId.Value);
                if (hdMua != null && hdMua.TongThanhToan >= 20_000_000m)
                {
                    viPham20Tr = true;
                }
            }
            else if (tongTien >= 20_000_000m)
            {
                // Kiểm tra nếu chi trả tiền mua hàng / nhà cung cấp >= 20tr bằng tiền mặt
                viPham20Tr = true;
            }
        }

        var chungTu = new ChungTuThuChi
        {
            SoChungTu = soChungTu,
            LoaiChungTu = model.LoaiChungTu,
            NgayChungTu = model.NgayChungTu,
            NgayHachToan = model.NgayHachToan,
            SoChungTuGoc = model.SoChungTuGoc ?? string.Empty,
            DoiTuongId = model.DoiTuongId,
            NguoiGiaoNopNhan = model.NguoiGiaoNopNhan,
            DiaChi = model.DiaChi,
            LyDo = model.LyDo,
            TaiKhoanNganHangId = model.TaiKhoanNganHangId,
            HoaDonBanHangId = model.HoaDonBanHangId,
            HoaDonMuaHangId = model.HoaDonMuaHangId,
            TongTien = tongTien,
            ViPhamQuyTac20Tr = viPham20Tr,
            TrangThai = TrangThaiThuChi.ChoGhiSo,
            NgayTao = DateTime.UtcNow
        };

        foreach (var ct in model.ChiTiets)
        {
            chungTu.ChiTietThuChis.Add(new ChiTietChungTuThuChi
            {
                DienGiai = string.IsNullOrWhiteSpace(ct.DienGiai) ? model.LyDo : ct.DienGiai,
                TaiKhoanNoId = ct.TaiKhoanNoId,
                TaiKhoanCoId = ct.TaiKhoanCoId,
                SoTien = ct.SoTien,
                DoiTuongId = ct.DoiTuongId ?? model.DoiTuongId
            });
        }

        await _context.ChungTuThuChis.AddAsync(chungTu);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã tạo chứng từ thu chi {SoChungTu}, Loại: {Loai}, Tổng tiền: {TongTien:N0}",
            chungTu.SoChungTu, chungTu.LoaiChungTu, chungTu.TongTien);

        return chungTu;
    }

    public async Task<ChungTuThuChi> GhiSoChungTuAsync(long chungTuId)
    {
        var chungTu = await _context.ChungTuThuChis
            .Include(c => c.ChiTietThuChis)
            .Include(c => c.HoaDonBanHang)
            .Include(c => c.HoaDonMuaHang)
            .FirstOrDefaultAsync(c => c.Id == chungTuId);

        if (chungTu == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy chứng từ thu chi id {chungTuId}");
        }

        if (chungTu.TrangThai == TrangThaiThuChi.DaGhiSo)
        {
            return chungTu;
        }

        // Chốt chặn 1: Bẫy Âm Quỹ Tiền Mặt (Negative Cash Prevention)
        if (chungTu.LoaiChungTu == LoaiChungTuThuChi.ChiTienMat)
        {
            var tonKhaDung = await TinhTonQuyKhaDungAsync(chungTu.NgayHachToan);
            if (chungTu.TongTien > tonKhaDung)
            {
                throw new InvalidOperationException(
                    $"Không thể ghi sổ: Tồn quỹ tiền mặt không đủ để chi! Tồn hiện tại: {tonKhaDung:N0} VNĐ, Số tiền yêu cầu chi: {chungTu.TongTien:N0} VNĐ. Nghiêm cấm xuất quỹ âm theo quy tắc tài chính!");
            }
        }
        else if (chungTu.LoaiChungTu == LoaiChungTuThuChi.UyNhiemChi)
        {
            if (!chungTu.TaiKhoanNganHangId.HasValue)
            {
                throw new InvalidOperationException("Ủy nhiệm chi bắt buộc phải chọn Tài khoản ngân hàng chi trả.");
            }

            var tonNganHang = await TinhTonQuyKhaDungAsync(chungTu.NgayHachToan, chungTu.TaiKhoanNganHangId.Value);
            if (chungTu.TongTien > tonNganHang)
            {
                throw new InvalidOperationException(
                    $"Không thể ghi sổ: Số dư tài khoản ngân hàng không đủ! Số dư hiện tại: {tonNganHang:N0} VNĐ, Số tiền yêu cầu chuyển: {chungTu.TongTien:N0} VNĐ.");
            }
        }

        // Tạo Bút toán Sổ Cái Core GL
        var butToan = new ButToan
        {
            SoChungTu = chungTu.SoChungTu,
            SoChungTuGoc = string.IsNullOrWhiteSpace(chungTu.SoChungTuGoc) ? chungTu.SoChungTu : chungTu.SoChungTuGoc,
            NgayChungTu = chungTu.NgayChungTu,
            NgayHachToan = chungTu.NgayHachToan,
            DienGiai = chungTu.LyDo ?? "Hạch toán thu chi",
            TongTien = chungTu.TongTien,
            TrangThai = TrangThaiButToan.ChuaGhiSo,
            ChiTietButToans = chungTu.ChiTietThuChis.Select(d => new ChiTietButToan
            {
                DienGiai = d.DienGiai,
                TaiKhoanNoId = d.TaiKhoanNoId,
                TaiKhoanCoId = d.TaiKhoanCoId,
                SoTien = d.SoTien,
                DoiTuongId = d.DoiTuongId
            }).ToList()
        };

        var (thanhCongTao, loiTao, btTao) = await _butToanService.TaoMoiAsync(butToan);
        if (!thanhCongTao || btTao == null)
        {
            throw new InvalidOperationException($"Lỗi tạo bút toán sổ cái: {loiTao}");
        }

        var (thanhCongGhi, loiGhi) = await _butToanService.GhiSoAsync(btTao.Id);
        if (!thanhCongGhi)
        {
            throw new InvalidOperationException($"Lỗi ghi sổ bút toán: {loiGhi}");
        }

        chungTu.ButToanId = btTao.Id;

        // Đối trừ công nợ nếu có hóa đơn liên kết
        if (chungTu.HoaDonBanHangId.HasValue)
        {
            await _congNoService.DoiTruHoaDonBanAsync(
                chungTu.HoaDonBanHangId.Value,
                chungTu.TongTien,
                btTao.Id,
                $"Thu tiền qua chứng từ {chungTu.SoChungTu}");
        }

        if (chungTu.HoaDonMuaHangId.HasValue)
        {
            await _congNoService.DoiTruHoaDonMuaAsync(
                chungTu.HoaDonMuaHangId.Value,
                chungTu.TongTien,
                btTao.Id,
                $"Thanh toán qua chứng từ {chungTu.SoChungTu}");
        }

        chungTu.TrangThai = TrangThaiThuChi.DaGhiSo;
        chungTu.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã ghi sổ thành công chứng từ thu chi {SoChungTu}, ButToanId: {ButToanId}",
            chungTu.SoChungTu, btTao.Id);

        return chungTu;
    }

    public async Task<ChungTuThuChi> HuyChungTuAsync(long chungTuId, string lyDoHuy)
    {
        var chungTu = await _context.ChungTuThuChis
            .Include(c => c.HoaDonBanHang)
            .Include(c => c.HoaDonMuaHang)
            .FirstOrDefaultAsync(c => c.Id == chungTuId);

        if (chungTu == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy chứng từ id {chungTuId}");
        }

        if (chungTu.TrangThai == TrangThaiThuChi.DaGhiSo)
        {
            // Bỏ ghi sổ và xóa bút toán sổ cái
            if (chungTu.ButToanId.HasValue)
            {
                await _butToanService.BoGhiSoAsync(chungTu.ButToanId.Value);
                await _butToanService.XoaAsync(chungTu.ButToanId.Value);
                chungTu.ButToanId = null;
            }

            // Hoàn lại công nợ hóa đơn bán
            if (chungTu.HoaDonBanHangId.HasValue && chungTu.HoaDonBanHang != null)
            {
                chungTu.HoaDonBanHang.DaThuTien = Math.Max(0, chungTu.HoaDonBanHang.DaThuTien - chungTu.TongTien);
            }

            // Hoàn lại công nợ hóa đơn mua
            if (chungTu.HoaDonMuaHangId.HasValue && chungTu.HoaDonMuaHang != null)
            {
                chungTu.HoaDonMuaHang.DaThanhToan = Math.Max(0, chungTu.HoaDonMuaHang.DaThanhToan - chungTu.TongTien);
            }
        }

        chungTu.TrangThai = TrangThaiThuChi.DaHuy;
        chungTu.LyDo = $"{chungTu.LyDo} [ĐÃ HỦY: {lyDoHuy}]";
        chungTu.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return chungTu;
    }

    public async Task<ThuChiFilterViewModel> TimKiemChungTuAsync(LoaiChungTuThuChi? loaiChungTu, DateTime? tuNgay, DateTime? denNgay, string? tuKhoa)
    {
        var query = _context.ChungTuThuChis
            .Include(c => c.DoiTuong)
            .Include(c => c.TaiKhoanNganHang)
            .Include(c => c.ButToan)
            .AsQueryable();

        if (loaiChungTu.HasValue)
        {
            query = query.Where(c => c.LoaiChungTu == loaiChungTu.Value);
        }

        if (tuNgay.HasValue)
        {
            query = query.Where(c => c.NgayChungTu >= tuNgay.Value.Date);
        }

        if (denNgay.HasValue)
        {
            var endOfDay = denNgay.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.NgayChungTu <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            query = query.Where(c => c.SoChungTu.Contains(tuKhoa)
                                     || (c.NguoiGiaoNopNhan != null && c.NguoiGiaoNopNhan.Contains(tuKhoa))
                                     || (c.LyDo != null && c.LyDo.Contains(tuKhoa))
                                     || (c.DoiTuong != null && c.DoiTuong.TenDoiTuong.Contains(tuKhoa)));
        }

        var items = await query
            .OrderByDescending(c => c.NgayChungTu)
            .ThenByDescending(c => c.Id)
            .Select(c => new ChungTuThuChiItemViewModel
            {
                Id = c.Id,
                SoChungTu = c.SoChungTu,
                LoaiChungTu = c.LoaiChungTu,
                NgayChungTu = c.NgayChungTu,
                TenDoiTuong = c.DoiTuong != null ? c.DoiTuong.TenDoiTuong : null,
                NguoiGiaoNopNhan = c.NguoiGiaoNopNhan,
                LyDo = c.LyDo,
                TongTien = c.TongTien,
                ViPhamQuyTac20Tr = c.ViPhamQuyTac20Tr,
                TrangThai = c.TrangThai,
                SoTaiKhoanNganHang = c.TaiKhoanNganHang != null ? c.TaiKhoanNganHang.SoTaiKhoan : null,
                SoButToan = c.ButToan != null ? c.ButToan.SoChungTu : null
            })
            .ToListAsync();

        return new ThuChiFilterViewModel
        {
            LoaiChungTu = loaiChungTu,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TuKhoa = tuKhoa,
            DanhSach = items
        };
    }

    public async Task<ChungTuThuChi?> LayChiTietChungTuAsync(long id)
    {
        return await _context.ChungTuThuChis
            .Include(c => c.DoiTuong)
            .Include(c => c.TaiKhoanNganHang)
            .Include(c => c.HoaDonBanHang)
            .Include(c => c.HoaDonMuaHang)
            .Include(c => c.ButToan)
            .Include(c => c.ChiTietThuChis)
                .ThenInclude(ct => ct.TaiKhoanNo)
            .Include(c => c.ChiTietThuChis)
                .ThenInclude(ct => ct.TaiKhoanCo)
            .Include(c => c.ChiTietThuChis)
                .ThenInclude(ct => ct.DoiTuong)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<SoQuyBaoCaoViewModel> LayBaoCaoSoQuyAsync(
        string loaiSo,
        DateTime tuNgay,
        DateTime denNgay,
        long? taiKhoanNganHangId = null)
    {
        var tuNgayDate = tuNgay.Date;
        var denNgayDate = denNgay.Date.AddDays(1).AddTicks(-1);

        var isNganHang = loaiSo == "1121" || taiKhoanNganHangId.HasValue;
        TaiKhoanNganHang? tkNh = null;

        if (isNganHang && taiKhoanNganHangId.HasValue)
        {
            tkNh = await _context.TaiKhoanNganHangs.FindAsync(taiKhoanNganHangId.Value);
        }

        // Tính số dư đầu kỳ (trước tuNgayDate)
        decimal soDuDauKy = 0;
        if (isNganHang)
        {
            var initial = tkNh?.SoDuBanDau ?? 0m;
            var thuTruoc = await _context.ChungTuThuChis
                .Where(c => c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan < tuNgayDate
                            && (taiKhoanNganHangId == null || c.TaiKhoanNganHangId == taiKhoanNganHangId)
                            && c.LoaiChungTu == LoaiChungTuThuChi.BaoCoNganHang)
                .SumAsync(c => c.TongTien);

            var chiTruoc = await _context.ChungTuThuChis
                .Where(c => c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan < tuNgayDate
                            && (taiKhoanNganHangId == null || c.TaiKhoanNganHangId == taiKhoanNganHangId)
                            && c.LoaiChungTu == LoaiChungTuThuChi.UyNhiemChi)
                .SumAsync(c => c.TongTien);

            soDuDauKy = initial + thuTruoc - chiTruoc;
        }
        else
        {
            var thuTruoc = await _context.ChungTuThuChis
                .Where(c => c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan < tuNgayDate
                            && c.LoaiChungTu == LoaiChungTuThuChi.ThuTienMat)
                .SumAsync(c => c.TongTien);

            var chiTruoc = await _context.ChungTuThuChis
                .Where(c => c.TrangThai == TrangThaiThuChi.DaGhiSo
                            && c.NgayHachToan < tuNgayDate
                            && c.LoaiChungTu == LoaiChungTuThuChi.ChiTienMat)
                .SumAsync(c => c.TongTien);

            soDuDauKy = thuTruoc - chiTruoc;
        }

        // Lấy phát sinh trong kỳ
        var query = _context.ChungTuThuChis
            .Include(c => c.ChiTietThuChis)
                .ThenInclude(ct => ct.TaiKhoanNo)
            .Include(c => c.ChiTietThuChis)
                .ThenInclude(ct => ct.TaiKhoanCo)
            .Where(c => c.TrangThai == TrangThaiThuChi.DaGhiSo
                        && c.NgayHachToan >= tuNgayDate
                        && c.NgayHachToan <= denNgayDate);

        if (isNganHang)
        {
            query = query.Where(c => (c.LoaiChungTu == LoaiChungTuThuChi.BaoCoNganHang || c.LoaiChungTu == LoaiChungTuThuChi.UyNhiemChi)
                                     && (taiKhoanNganHangId == null || c.TaiKhoanNganHangId == taiKhoanNganHangId));
        }
        else
        {
            query = query.Where(c => c.LoaiChungTu == LoaiChungTuThuChi.ThuTienMat || c.LoaiChungTu == LoaiChungTuThuChi.ChiTienMat);
        }

        var chungTus = await query
            .OrderBy(c => c.NgayHachToan)
            .ThenBy(c => c.Id)
            .ToListAsync();

        var dongs = new List<DongSoQuyViewModel>();
        var luyKe = soDuDauKy;
        decimal tongThuKy = 0;
        decimal tongChiKy = 0;

        foreach (var c in chungTus)
        {
            var isThu = c.LoaiChungTu == LoaiChungTuThuChi.ThuTienMat || c.LoaiChungTu == LoaiChungTuThuChi.BaoCoNganHang;
            var tienThu = isThu ? c.TongTien : 0m;
            var tienChi = !isThu ? c.TongTien : 0m;

            tongThuKy += tienThu;
            tongChiKy += tienChi;
            luyKe = luyKe + tienThu - tienChi;

            // Lấy mã tài khoản đối ứng từ các dòng chi tiết
            var doiUngList = isThu
                ? c.ChiTietThuChis.Select(ct => ct.TaiKhoanCo?.MaTaiKhoan ?? "").Distinct()
                : c.ChiTietThuChis.Select(ct => ct.TaiKhoanNo?.MaTaiKhoan ?? "").Distinct();
            var tkDoiUng = string.Join(", ", doiUngList.Where(x => !string.IsNullOrEmpty(x)));

            dongs.Add(new DongSoQuyViewModel
            {
                NgayGhiSo = c.NgayHachToan,
                NgayChungTu = c.NgayChungTu,
                SoChungTuThu = isThu ? c.SoChungTu : string.Empty,
                SoChungTuChi = !isThu ? c.SoChungTu : string.Empty,
                DienGiai = c.LyDo ?? string.Empty,
                TaiKhoanDoiUng = tkDoiUng,
                SoTienThu = tienThu,
                SoTienChi = tienChi,
                SoDuLuyKe = luyKe
            });
        }

        return new SoQuyBaoCaoViewModel
        {
            LoaiSo = isNganHang ? "1121" : "1111",
            TenSo = isNganHang ? "SỔ TIỀN GỬI NGÂN HÀNG" : "SỔ QUỸ TIỀN MẶT",
            TuNgay = tuNgayDate,
            DenNgay = denNgay.Date,
            TaiKhoanNganHangId = taiKhoanNganHangId,
            TenNganHang = tkNh?.TenNganHang,
            SoTaiKhoanNganHang = tkNh?.SoTaiKhoan,
            SoDuDauKy = soDuDauKy,
            TongThuTrongKy = tongThuKy,
            TongChiTrongKy = tongChiKy,
            SoDuCuoiKy = luyKe,
            DongSoQuys = dongs
        };
    }

    public async Task<List<TaiKhoanNganHang>> LayDanhSachTaiKhoanNganHangAsync()
    {
        return await _context.TaiKhoanNganHangs
            .Include(t => t.TaiKhoanKeToan)
            .OrderBy(t => t.TenNganHang)
            .ToListAsync();
    }

    public async Task<TaiKhoanNganHang> TaoTaiKhoanNganHangAsync(TaiKhoanNganHang taiKhoan)
    {
        await _context.TaiKhoanNganHangs.AddAsync(taiKhoan);
        await _context.SaveChangesAsync();
        return taiKhoan;
    }
}
