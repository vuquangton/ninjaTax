using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public class CommercialAdjustmentService : ICommercialAdjustmentService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ILogger<CommercialAdjustmentService> _logger;

    public CommercialAdjustmentService(
        AppDbContext context,
        IButToanService butToanService,
        ILogger<CommercialAdjustmentService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _logger = logger;
    }

    public async Task<ChungTuDieuChinhThuongMai> TaoChungTuAsync(ChungTuDieuChinhThuongMai chungTu)
    {
        chungTu.SoChungTu = (chungTu.SoChungTu ?? string.Empty).Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(chungTu.SoChungTu))
            throw new ArgumentException("Số chứng từ không được để trống.");

        if (chungTu.ChiTietDieuChinhs == null || !chungTu.ChiTietDieuChinhs.Any())
            throw new ArgumentException("Chứng từ điều chỉnh phải có ít nhất 1 dòng chi tiết.");

        // Kiểm tra trùng số chứng từ
        var exists = await _context.ChungTuDieuChinhThuongMais
            .AnyAsync(c => c.SoChungTu == chungTu.SoChungTu && c.Id != chungTu.Id);
        if (exists)
            throw new InvalidOperationException($"Số chứng từ {chungTu.SoChungTu} đã tồn tại trong hệ thống.");

        // Tính toán tổng số tiền
        int dongSo = 1;
        foreach (var item in chungTu.ChiTietDieuChinhs)
        {
            item.DongSo = dongSo++;
            item.ThanhTien = Math.Round(item.SoLuong * item.DonGia, 4);
            item.TienThueVat = Math.Round(item.ThanhTien * (item.ThueSuatVat / 100m), 4);
            item.TienGiaVonNhapLai = Math.Round(item.SoLuong * item.DonGiaVonNhapLai, 4);
        }

        chungTu.TongTienHang = chungTu.ChiTietDieuChinhs.Sum(c => c.ThanhTien);
        chungTu.TongTienThueVat = chungTu.ChiTietDieuChinhs.Sum(c => c.TienThueVat);
        chungTu.TongThanhToan = chungTu.TongTienHang + chungTu.TongTienThueVat;
        chungTu.TongGiaTriNhapLaiKho = chungTu.ChiTietDieuChinhs.Sum(c => c.TienGiaVonNhapLai);

        _context.ChungTuDieuChinhThuongMais.Add(chungTu);
        await _context.SaveChangesAsync();

        // Ghi sổ tự động
        await GhiSoAsync(chungTu.Id);

        return chungTu;
    }

    public async Task<bool> GhiSoAsync(long id)
    {
        var chungTu = await _context.ChungTuDieuChinhThuongMais
            .Include(c => c.ChiTietDieuChinhs)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chungTu == null) return false;
        if (chungTu.TrangThai == TrangThaiDieuChinhThuongMai.DaGhiSo) return true;

        // 1. Lấy các tài khoản chuẩn TT99
        var tk131 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "131") ?? await _context.TaiKhoans.FirstAsync();
        var tk331 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "331") ?? await _context.TaiKhoans.FirstAsync();
        var tk1111 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1111") ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "111") ?? await _context.TaiKhoans.FirstAsync();
        var tk1121 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1121") ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "112") ?? await _context.TaiKhoans.FirstAsync();

        var tk5212 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "5212") ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "521");
        var tk5211 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "5211") ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "521");
        var tk33311 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "33311") ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "3331");
        var tk1331 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1331") ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "133");
        var tk1561 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1561") ?? await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "156");
        var tk632 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "632");

        long taiKhoanCongNoHoacTienId = chungTu.HinhThucXuLy switch
        {
            HinhThucXuLyDieuChinh.TienMat => tk1111.Id,
            HinhThucXuLyDieuChinh.TienGuiNganHang => tk1121.Id,
            _ => (chungTu.LoaiDieuChinh == LoaiDieuChinhThuongMai.HangMuaTraLai || chungTu.LoaiDieuChinh == LoaiDieuChinhThuongMai.GiamGiaHangMua) ? tk331.Id : tk131.Id
        };

        // 2. Sinh bút toán hạch toán theo nghiệp vụ
        if (chungTu.LoaiDieuChinh == LoaiDieuChinhThuongMai.HangBanTraLai)
        {
            // Bút toán 1: Giảm trừ doanh thu
            // Nợ TK 5212 (Doanh thu hàng bán bị trả lại)
            // Nợ TK 33311 (Thuế GTGT đầu ra giảm trừ)
            // Có TK 131 (hoặc 111/112)
            var bt1 = new ButToan
            {
                SoChungTu = $"PKT-{chungTu.SoChungTu}-DT",
                NgayHachToan = chungTu.NgayHachToan,
                NgayChungTu = chungTu.NgayChungTu,
                SoChungTuGoc = chungTu.SoChungTu,
                NgayChungTuGoc = chungTu.NgayChungTu,
                DienGiai = $"Hàng bán bị trả lại - Giảm trừ doanh thu {chungTu.SoChungTu}",
                TrangThai = TrangThaiButToan.DaGhiSo,
                ChiTietButToans = new List<ChiTietButToan>()
            };

            int dongNo = 1;
            // Dòng giảm doanh thu
            bt1.ChiTietButToans.Add(new ChiTietButToan
            {
                DongSo = dongNo++,
                TaiKhoanNoId = tk5212?.Id ?? tk131.Id,
                TaiKhoanCoId = taiKhoanCongNoHoacTienId,
                SoTien = chungTu.TongTienHang,
                DienGiai = "Giảm doanh thu hàng bán bị trả lại (TK 5212)",
                DoiTuongId = chungTu.DoiTuongId
            });

            // Dòng thuế GTGT nếu có
            if (chungTu.TongTienThueVat > 0 && tk33311 != null)
            {
                bt1.ChiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = dongNo++,
                    TaiKhoanNoId = tk33311.Id,
                    TaiKhoanCoId = taiKhoanCongNoHoacTienId,
                    SoTien = chungTu.TongTienThueVat,
                    DienGiai = "Giảm thuế GTGT đầu ra (TK 33311)",
                    DoiTuongId = chungTu.DoiTuongId
                });
            }

            var (ok1, msg1, createdBt1) = await _butToanService.TaoMoiAsync(bt1);
            if (!ok1 || createdBt1 == null)
                throw new InvalidOperationException($"Lỗi hạch toán giảm trừ doanh thu: {msg1}");
            chungTu.ButToanDoanhThuCongNoId = createdBt1.Id;

            // Bút toán 2: Nhập lại kho giảm giá vốn (Nợ 1561 / Có 632) nếu có giá vốn
            if (chungTu.TongGiaTriNhapLaiKho > 0 && tk1561 != null && tk632 != null)
            {
                var bt2 = new ButToan
                {
                    SoChungTu = $"PKT-{chungTu.SoChungTu}-GV",
                    NgayHachToan = chungTu.NgayHachToan,
                    NgayChungTu = chungTu.NgayChungTu,
                    SoChungTuGoc = chungTu.SoChungTu,
                    NgayChungTuGoc = chungTu.NgayChungTu,
                    DienGiai = $"Hàng bán bị trả lại - Nhập lại kho giảm giá vốn {chungTu.SoChungTu}",
                    TrangThai = TrangThaiButToan.DaGhiSo,
                    ChiTietButToans = new List<ChiTietButToan>
                    {
                        new ChiTietButToan
                        {
                            DongSo = 1,
                            TaiKhoanNoId = tk1561.Id,
                            TaiKhoanCoId = tk632.Id,
                            SoTien = chungTu.TongGiaTriNhapLaiKho,
                            DienGiai = "Nhập lại kho giảm giá vốn hàng bán (Nợ 1561 / Có 632)"
                        }
                    }
                };

                var (ok2, msg2, createdBt2) = await _butToanService.TaoMoiAsync(bt2);
                if (!ok2 || createdBt2 == null)
                    throw new InvalidOperationException($"Lỗi hạch toán nhập kho giá vốn: {msg2}");
                chungTu.ButToanGiaVonKhoId = createdBt2.Id;

                // Tự động sinh Phiếu Nhập Kho nhập lại hàng trả
                if (chungTu.KhoId.HasValue)
                {
                    var pnk = new PhieuNhapKho
                    {
                        ChiNhanhId = chungTu.ChiNhanhId,
                        KhoId = chungTu.KhoId.Value,
                        SoPhieu = $"PNK-{chungTu.SoChungTu}",
                        NgayNhap = chungTu.NgayChungTu,
                        NgayHachToan = chungTu.NgayHachToan,
                        LoaiNhapKho = LoaiNhapKho.NhapKhac,
                        DienGiai = $"Nhập lại hàng bán bị trả lại theo {chungTu.SoChungTu}",
                        TongSoLuong = chungTu.ChiTietDieuChinhs.Sum(c => c.SoLuong),
                        TongTienHang = chungTu.TongGiaTriNhapLaiKho,
                        TrangThai = TrangThaiPhieuKho.DaGhiSo,
                        ChiTietNhapKhos = chungTu.ChiTietDieuChinhs
                            .Where(c => c.VatTuHangHoaId.HasValue)
                            .Select(c => new ChiTietNhapKho
                            {
                                VatTuHangHoaId = c.VatTuHangHoaId!.Value,
                                SoLuong = c.SoLuong,
                                DonGia = c.DonGiaVonNhapLai,
                                ThanhTien = c.TienGiaVonNhapLai,
                                TaiKhoanNoId = tk1561.Id,
                                TaiKhoanCoId = tk632.Id
                            }).ToList()
                    };
                    _context.PhieuNhapKhos.Add(pnk);
                }
            }
        }
        else if (chungTu.LoaiDieuChinh == LoaiDieuChinhThuongMai.ChietKhauThuongMaiBan)
        {
            // Nợ TK 5211 (Chiết khấu thương mại)
            // Nợ TK 33311 (Thuế GTGT nếu có)
            // Có TK 131 (Giảm trừ công nợ)
            var bt = new ButToan
            {
                SoChungTu = $"PKT-{chungTu.SoChungTu}",
                NgayHachToan = chungTu.NgayHachToan,
                NgayChungTu = chungTu.NgayChungTu,
                SoChungTuGoc = chungTu.SoChungTu,
                NgayChungTuGoc = chungTu.NgayChungTu,
                DienGiai = $"Chiết khấu thương mại bán hàng {chungTu.SoChungTu}",
                TrangThai = TrangThaiButToan.DaGhiSo,
                ChiTietButToans = new List<ChiTietButToan>()
            };

            int dong = 1;
            bt.ChiTietButToans.Add(new ChiTietButToan
            {
                DongSo = dong++,
                TaiKhoanNoId = tk5211?.Id ?? tk131.Id,
                TaiKhoanCoId = taiKhoanCongNoHoacTienId,
                SoTien = chungTu.TongTienHang,
                DienGiai = "Chiết khấu thương mại bán hàng (TK 5211)",
                DoiTuongId = chungTu.DoiTuongId
            });

            if (chungTu.TongTienThueVat > 0 && tk33311 != null)
            {
                bt.ChiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = dong++,
                    TaiKhoanNoId = tk33311.Id,
                    TaiKhoanCoId = taiKhoanCongNoHoacTienId,
                    SoTien = chungTu.TongTienThueVat,
                    DienGiai = "Giảm thuế GTGT chiết khấu thương mại (TK 33311)",
                    DoiTuongId = chungTu.DoiTuongId
                });
            }

            var (ok, msg, createdBt) = await _butToanService.TaoMoiAsync(bt);
            if (!ok || createdBt == null)
                throw new InvalidOperationException($"Lỗi hạch toán chiết khấu thương mại: {msg}");
            chungTu.ButToanDoanhThuCongNoId = createdBt.Id;
        }
        else if (chungTu.LoaiDieuChinh == LoaiDieuChinhThuongMai.HangMuaTraLai || chungTu.LoaiDieuChinh == LoaiDieuChinhThuongMai.GiamGiaHangMua)
        {
            // Hàng mua trả lại: Nợ TK 331 / Có TK 1561, Có TK 1331
            var bt = new ButToan
            {
                SoChungTu = $"PKT-{chungTu.SoChungTu}",
                NgayHachToan = chungTu.NgayHachToan,
                NgayChungTu = chungTu.NgayChungTu,
                SoChungTuGoc = chungTu.SoChungTu,
                NgayChungTuGoc = chungTu.NgayChungTu,
                DienGiai = $"Hàng mua trả lại / giảm giá {chungTu.SoChungTu}",
                TrangThai = TrangThaiButToan.DaGhiSo,
                ChiTietButToans = new List<ChiTietButToan>()
            };

            int dong = 1;
            // Dòng giá trị hàng mua trả lại
            bt.ChiTietButToans.Add(new ChiTietButToan
            {
                DongSo = dong++,
                TaiKhoanNoId = taiKhoanCongNoHoacTienId, // 331 hoặc 111/112
                TaiKhoanCoId = tk1561?.Id ?? tk331.Id,
                SoTien = chungTu.TongTienHang,
                DienGiai = "Giảm giá trị mua hàng / Hàng mua trả lại (Có TK 1561)",
                DoiTuongId = chungTu.DoiTuongId
            });

            // Dòng thuế GTGT đầu vào giảm khấu trừ (Có TK 1331)
            if (chungTu.TongTienThueVat > 0 && tk1331 != null)
            {
                bt.ChiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = dong++,
                    TaiKhoanNoId = taiKhoanCongNoHoacTienId,
                    TaiKhoanCoId = tk1331.Id,
                    SoTien = chungTu.TongTienThueVat,
                    DienGiai = "Giảm thuế GTGT đầu vào được khấu trừ (Có TK 1331)",
                    DoiTuongId = chungTu.DoiTuongId
                });
            }

            var (ok, msg, createdBt) = await _butToanService.TaoMoiAsync(bt);
            if (!ok || createdBt == null)
                throw new InvalidOperationException($"Lỗi hạch toán hàng mua trả lại: {msg}");
            chungTu.ButToanDoanhThuCongNoId = createdBt.Id;

            // Nếu là Hàng Mua Trả Lại và có chỉ định Kho -> Tự động sinh Phiếu Xuất Kho trả lại NCC
            if (chungTu.LoaiDieuChinh == LoaiDieuChinhThuongMai.HangMuaTraLai && chungTu.KhoId.HasValue)
            {
                var pxk = new PhieuXuatKho
                {
                    ChiNhanhId = chungTu.ChiNhanhId,
                    KhoId = chungTu.KhoId.Value,
                    SoPhieu = $"PXK-{chungTu.SoChungTu}",
                    NgayXuat = chungTu.NgayChungTu,
                    NgayHachToan = chungTu.NgayHachToan,
                    LoaiXuatKho = LoaiXuatKho.TraHangNhaCungCap,
                    DienGiai = $"Xuất kho trả lại hàng cho nhà cung cấp theo {chungTu.SoChungTu}",
                    TongSoLuong = chungTu.ChiTietDieuChinhs.Sum(c => c.SoLuong),
                    TongTienGiaVon = chungTu.TongTienHang,
                    TrangThai = TrangThaiPhieuKho.DaGhiSo,
                    ChiTietXuatKhos = chungTu.ChiTietDieuChinhs
                        .Where(c => c.VatTuHangHoaId.HasValue)
                        .Select(c => new ChiTietXuatKho
                        {
                            VatTuHangHoaId = c.VatTuHangHoaId!.Value,
                            SoLuong = c.SoLuong,
                            DonGiaVon = c.DonGia,
                            TienGiaVon = c.ThanhTien,
                            TaiKhoanNoId = taiKhoanCongNoHoacTienId,
                            TaiKhoanCoId = tk1561?.Id ?? tk331.Id
                        }).ToList()
                };
                _context.PhieuXuatKhos.Add(pxk);
            }
        }

        chungTu.TrangThai = TrangThaiDieuChinhThuongMai.DaGhiSo;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Ghi sổ thành công chứng từ điều chỉnh thương mại {SoChungTu}", chungTu.SoChungTu);
        return true;
    }

    public async Task<bool> HuyGhiSoAsync(long id)
    {
        var chungTu = await _context.ChungTuDieuChinhThuongMais.FindAsync(id);
        if (chungTu == null) return false;
        if (chungTu.TrangThai == TrangThaiDieuChinhThuongMai.DaHuy) return true;

        if (chungTu.ButToanDoanhThuCongNoId.HasValue)
        {
            await _butToanService.XoaAsync(chungTu.ButToanDoanhThuCongNoId.Value);
            chungTu.ButToanDoanhThuCongNoId = null;
        }

        if (chungTu.ButToanGiaVonKhoId.HasValue)
        {
            await _butToanService.XoaAsync(chungTu.ButToanGiaVonKhoId.Value);
            chungTu.ButToanGiaVonKhoId = null;
        }

        // Hủy phiếu nhập kho hoặc xuất kho liên quan
        var pnkSoPhieu = $"PNK-{chungTu.SoChungTu}";
        var pnk = await _context.PhieuNhapKhos.FirstOrDefaultAsync(p => p.SoPhieu == pnkSoPhieu);
        if (pnk != null) pnk.TrangThai = TrangThaiPhieuKho.DaHuy;

        var pxkSoPhieu = $"PXK-{chungTu.SoChungTu}";
        var pxk = await _context.PhieuXuatKhos.FirstOrDefaultAsync(p => p.SoPhieu == pxkSoPhieu);
        if (pxk != null) pxk.TrangThai = TrangThaiPhieuKho.DaHuy;

        chungTu.TrangThai = TrangThaiDieuChinhThuongMai.DaHuy;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã hủy chứng từ điều chỉnh thương mại {SoChungTu}", chungTu.SoChungTu);
        return true;
    }

    public async Task<List<ChungTuDieuChinhThuongMai>> LayDanhSachAsync(LoaiDieuChinhThuongMai? loai = null, DateTime? tuNgay = null, DateTime? denNgay = null, long? branchId = null)
    {
        var query = _context.ChungTuDieuChinhThuongMais
            .Include(c => c.DoiTuong)
            .Include(c => c.Kho)
            .Include(c => c.ChiNhanh)
            .AsNoTracking()
            .AsQueryable();

        if (loai.HasValue) query = query.Where(c => c.LoaiDieuChinh == loai.Value);
        if (branchId.HasValue && branchId.Value > 0) query = query.Where(c => c.ChiNhanhId == branchId.Value);
        if (tuNgay.HasValue) query = query.Where(c => c.NgayHachToan >= tuNgay.Value.Date);
        if (denNgay.HasValue)
        {
            var endOfDay = denNgay.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.NgayHachToan <= endOfDay);
        }

        return await query.OrderByDescending(c => c.NgayHachToan).ThenByDescending(c => c.Id).ToListAsync();
    }

    public async Task<ChungTuDieuChinhThuongMai?> LayChiTietAsync(long id)
    {
        return await _context.ChungTuDieuChinhThuongMais
            .Include(c => c.DoiTuong)
            .Include(c => c.Kho)
            .Include(c => c.ChiNhanh)
            .Include(c => c.ButToanDoanhThuCongNo)
                .ThenInclude(b => b!.ChiTietButToans)
            .Include(c => c.ButToanGiaVonKho)
                .ThenInclude(b => b!.ChiTietButToans)
            .Include(c => c.ChiTietDieuChinhs)
                .ThenInclude(d => d.VatTuHangHoa)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
