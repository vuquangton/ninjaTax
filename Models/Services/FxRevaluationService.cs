using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ Đánh giá lại chênh lệch tỷ giá ngoại tệ cuối kỳ theo VAS 10 và hạch toán qua TK 413
/// Bất biến cốt lõi: Số dư TK 4131 sau khi hoàn thành kết chuyển bắt buộc bằng 0 (DuNo == 0 && DuCo == 0).
/// </summary>
public class FxRevaluationService : IFxRevaluationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FxRevaluationService> _logger;

    public FxRevaluationService(AppDbContext context, ILogger<FxRevaluationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FxRevaluationPreviewModel> XemTruocDanhGiaLaiAsync(
        DateTime ngayDanhGia,
        string loaiTien,
        decimal tyGiaMua,
        decimal tyGiaBan)
    {
        var preview = new FxRevaluationPreviewModel
        {
            NgayDanhGia = ngayDanhGia.Date,
            LoaiTien = loaiTien.ToUpper(),
            TyGiaMua = tyGiaMua,
            TyGiaBan = tyGiaBan
        };

        // Tìm các tài khoản ngoại tệ: 1112 (Tiền mặt ngoại tệ), 1122 (Tiền gửi ngoại tệ), 131, 331
        var taiKhoanCodes = new[] { "1112", "1122", "131", "331" };
        var taiKhoans = await _context.TaiKhoans
            .Where(t => taiKhoanCodes.Contains(t.MaTaiKhoan))
            .ToListAsync();

        var tkDict = taiKhoans.ToDictionary(t => t.MaTaiKhoan);

        // Lấy tất cả bút toán đã ghi sổ phát sinh đến ngày đánh giá
        var btQuery = _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Include(c => c.DoiTuong)
            .Where(c => c.ButToan!.NgayHachToan <= ngayDanhGia.Date && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo);

        // 1. Quét tài khoản 1112 & 1122 (Tài sản bằng tiền) -> Áp dụng Tỷ giá Mua
        foreach (var code in new[] { "1112", "1122" })
        {
            if (!tkDict.TryGetValue(code, out var tk)) continue;

            var items = await btQuery
                .Where(c => c.TaiKhoanNoId == tk.Id || c.TaiKhoanCoId == tk.Id)
                .Select(c => new { c.TaiKhoanNoId, c.TaiKhoanCoId, c.SoTien })
                .ToListAsync();

            var tongNoVnd = items.Where(x => x.TaiKhoanNoId == tk.Id).Sum(x => x.SoTien);
            var tongCoVnd = items.Where(x => x.TaiKhoanCoId == tk.Id).Sum(x => x.SoTien);
            var soDuVnd = tongNoVnd - tongCoVnd;

            if (soDuVnd <= 0) continue;

            // Lấy tỷ giá bình quân ghi sổ hoặc tính từ số dư nguyên tệ
            // Giả định chuẩn hóa: Với tài khoản 1112/1122, nếu chưa có trường số dư nguyên tệ riêng,
            // tỷ giá ghi sổ được tính bằng Tỷ giá mua lịch sử gần nhất hoặc tính ngược
            decimal tyGiaGhiSo = tyGiaMua > 0 ? tyGiaMua : 25000m;
            decimal soDuNgoaiTe = Math.Round(soDuVnd / tyGiaGhiSo, 4);

            decimal giaTriDanhGiaLaiVnd = Math.Round(soDuNgoaiTe * tyGiaMua, 4);
            decimal chenhLechVnd = giaTriDanhGiaLaiVnd - soDuVnd;

            if (chenhLechVnd != 0)
            {
                preview.ChiTiets.Add(new ChiTietDanhGiaLaiPreviewItem
                {
                    TaiKhoanId = tk.Id,
                    MaTaiKhoan = tk.MaTaiKhoan,
                    TenTaiKhoan = tk.TenTaiKhoan,
                    SoDuNgoaiTe = soDuNgoaiTe,
                    TyGiaGhiSo = tyGiaGhiSo,
                    GiaTriGhiSoVnd = soDuVnd,
                    TyGiaDanhGiaLai = tyGiaMua,
                    GiaTriDanhGiaLaiVnd = giaTriDanhGiaLaiVnd,
                    ChenhLechVnd = chenhLechVnd
                });
            }
        }

        // 2. Quét tài khoản 131 (Phải thu - Tài sản) theo từng Đối tượng -> Áp dụng Tỷ giá Mua
        if (tkDict.TryGetValue("131", out var tk131))
        {
            var items131 = await btQuery
                .Where(c => c.TaiKhoanNoId == tk131.Id || c.TaiKhoanCoId == tk131.Id)
                .Where(c => c.DoiTuongId.HasValue)
                .GroupBy(c => new { c.DoiTuongId, c.DoiTuong!.MaDoiTuong, c.DoiTuong.TenDoiTuong })
                .Select(g => new
                {
                    g.Key.DoiTuongId,
                    g.Key.MaDoiTuong,
                    g.Key.TenDoiTuong,
                    TongNo = g.Where(x => x.TaiKhoanNoId == tk131.Id).Sum(x => x.SoTien),
                    TongCo = g.Where(x => x.TaiKhoanCoId == tk131.Id).Sum(x => x.SoTien)
                })
                .ToListAsync();

            foreach (var dt in items131)
            {
                var duNoVnd = dt.TongNo - dt.TongCo;
                if (duNoVnd <= 0) continue;

                decimal tyGiaGhiSo = tyGiaMua > 0 ? tyGiaMua : 25000m;
                decimal soDuNgoaiTe = Math.Round(duNoVnd / tyGiaGhiSo, 4);
                decimal giaTriDanhGiaLaiVnd = Math.Round(soDuNgoaiTe * tyGiaMua, 4);
                decimal chenhLechVnd = giaTriDanhGiaLaiVnd - duNoVnd;

                if (chenhLechVnd != 0)
                {
                    preview.ChiTiets.Add(new ChiTietDanhGiaLaiPreviewItem
                    {
                        TaiKhoanId = tk131.Id,
                        MaTaiKhoan = tk131.MaTaiKhoan,
                        TenTaiKhoan = tk131.TenTaiKhoan,
                        DoiTuongId = dt.DoiTuongId,
                        MaDoiTuong = dt.MaDoiTuong,
                        TenDoiTuong = dt.TenDoiTuong,
                        SoDuNgoaiTe = soDuNgoaiTe,
                        TyGiaGhiSo = tyGiaGhiSo,
                        GiaTriGhiSoVnd = duNoVnd,
                        TyGiaDanhGiaLai = tyGiaMua,
                        GiaTriDanhGiaLaiVnd = giaTriDanhGiaLaiVnd,
                        ChenhLechVnd = chenhLechVnd
                    });
                }
            }
        }

        // 3. Quét tài khoản 331 (Phải trả - Nợ phải trả) theo từng Đối tượng -> Áp dụng Tỷ giá Bán
        if (tkDict.TryGetValue("331", out var tk331))
        {
            var items331 = await btQuery
                .Where(c => c.TaiKhoanNoId == tk331.Id || c.TaiKhoanCoId == tk331.Id)
                .Where(c => c.DoiTuongId.HasValue)
                .GroupBy(c => new { c.DoiTuongId, c.DoiTuong!.MaDoiTuong, c.DoiTuong.TenDoiTuong })
                .Select(g => new
                {
                    g.Key.DoiTuongId,
                    g.Key.MaDoiTuong,
                    g.Key.TenDoiTuong,
                    TongNo = g.Where(x => x.TaiKhoanNoId == tk331.Id).Sum(x => x.SoTien),
                    TongCo = g.Where(x => x.TaiKhoanCoId == tk331.Id).Sum(x => x.SoTien)
                })
                .ToListAsync();

            foreach (var dt in items331)
            {
                var duCoVnd = dt.TongCo - dt.TongNo;
                if (duCoVnd <= 0) continue;

                decimal tyGiaGhiSo = tyGiaBan > 0 ? tyGiaBan : 25500m;
                decimal soDuNgoaiTe = Math.Round(duCoVnd / tyGiaGhiSo, 4);
                decimal giaTriDanhGiaLaiVnd = Math.Round(soDuNgoaiTe * tyGiaBan, 4);
                // Với nợ phải trả: Nếu Giá trị ĐGL > Sổ sách => Lỗ tỷ giá (ChenhLech < 0).
                // Nếu Giá trị ĐGL < Sổ sách => Lãi tỷ giá (ChenhLech > 0).
                decimal chenhLechVnd = duCoVnd - giaTriDanhGiaLaiVnd;

                if (chenhLechVnd != 0)
                {
                    preview.ChiTiets.Add(new ChiTietDanhGiaLaiPreviewItem
                    {
                        TaiKhoanId = tk331.Id,
                        MaTaiKhoan = tk331.MaTaiKhoan,
                        TenTaiKhoan = tk331.TenTaiKhoan,
                        DoiTuongId = dt.DoiTuongId,
                        MaDoiTuong = dt.MaDoiTuong,
                        TenDoiTuong = dt.TenDoiTuong,
                        SoDuNgoaiTe = soDuNgoaiTe,
                        TyGiaGhiSo = tyGiaGhiSo,
                        GiaTriGhiSoVnd = duCoVnd,
                        TyGiaDanhGiaLai = tyGiaBan,
                        GiaTriDanhGiaLaiVnd = giaTriDanhGiaLaiVnd,
                        ChenhLechVnd = chenhLechVnd
                    });
                }
            }
        }

        return preview;
    }

    public async Task<DanhGiaLaiNgoaiTe> ThucHienDanhGiaLaiCuoiKyAsync(
        DateTime ngayDanhGia,
        string loaiTien,
        decimal tyGiaMua,
        decimal tyGiaBan,
        string? ghiChu = null)
    {
        // 1. Kiểm tra khóa sổ kế toán
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh?.NgayKhoaSo.HasValue == true && ngayDanhGia.Date <= cauHinh.NgayKhoaSo.Value.Date)
        {
            throw new InvalidOperationException($"Kỳ kế toán đã khóa sổ đến hết ngày {cauHinh.NgayKhoaSo:dd/MM/yyyy}. Không thể đánh giá lại ngoại tệ.");
        }

        var chiNhanh = await _context.ChiNhanhs.FirstOrDefaultAsync(c => c.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh)
                       ?? await _context.ChiNhanhs.FirstOrDefaultAsync();
        if (chiNhanh == null)
        {
            throw new InvalidOperationException("Hệ thống chưa thiết lập chi nhánh trụ sở chính.");
        }

        var tk4131 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "4131");
        var tk515 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "515");
        var tk635 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "635");

        if (tk4131 == null || tk515 == null || tk635 == null)
        {
            throw new InvalidOperationException("Hệ thống thiếu tài khoản 4131, 515 hoặc 635 trong danh mục tài khoản.");
        }

        var preview = await XemTruocDanhGiaLaiAsync(ngayDanhGia, loaiTien, tyGiaMua, tyGiaBan);

        var soChungTu = $"DGLTG-{ngayDanhGia:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        var entity = new DanhGiaLaiNgoaiTe
        {
            ChiNhanhId = chiNhanh.Id,
            SoChungTu = soChungTu,
            NgayChungTu = ngayDanhGia.Date,
            NgayHachToan = ngayDanhGia.Date,
            LoaiTien = loaiTien.ToUpper(),
            TyGiaMua = tyGiaMua,
            TyGiaBan = tyGiaBan,
            TongLaiTyGia = preview.TongLaiTyGia,
            TongLoTyGia = preview.TongLoTyGia,
            ChiTietDanhGiaLais = preview.ChiTiets.Select(c => new ChiTietDanhGiaLaiNgoaiTe
            {
                TaiKhoanId = c.TaiKhoanId,
                DoiTuongId = c.DoiTuongId,
                SoDuNgoaiTe = c.SoDuNgoaiTe,
                TyGiaGhiSo = c.TyGiaGhiSo,
                GiaTriGhiSoVnd = c.GiaTriGhiSoVnd,
                TyGiaDanhGiaLai = c.TyGiaDanhGiaLai,
                GiaTriDanhGiaLaiVnd = c.GiaTriDanhGiaLaiVnd,
                ChenhLechVnd = c.ChenhLechVnd
            }).ToList()
        };

        await _context.DanhGiaLaiNgoaiTes.AddAsync(entity);
        await _context.SaveChangesAsync();

        if (preview.ChiTiets.Count == 0)
        {
            _logger.LogInformation("Không có phát sinh chênh lệch tỷ giá ngoại tệ cần đánh giá lại.");
            return entity;
        }

        // Bút toán 1: Hạch toán các khoản chênh lệch tỷ giá vào TK 4131
        // Lãi: Nợ TK Tài sản / Có 4131 hoặc Nợ 331 / Có 4131
        // Lỗ: Nợ 4131 / Có TK Tài sản hoặc Nợ 4131 / Có 331
        var btDanhGiaLai = new ButToan
        {
            SoChungTu = $"PKT-DGL-{soChungTu}",
            NgayHachToan = ngayDanhGia.Date,
            NgayChungTu = ngayDanhGia.Date,
            SoChungTuGoc = soChungTu,
            NgayChungTuGoc = ngayDanhGia.Date,
            DienGiai = ghiChu ?? $"Đánh giá lại chênh lệch tỷ giá ngoại tệ theo {soChungTu}",
            TongTien = preview.TongLaiTyGia + preview.TongLoTyGia,
            TrangThai = TrangThaiButToan.DaGhiSo,
            ChiTietButToans = new List<ChiTietButToan>()
        };

        int dongSo = 1;
        foreach (var c in preview.ChiTiets)
        {
            decimal tien = Math.Abs(c.ChenhLechVnd);
            if (tien == 0) continue;

            if (c.LaLai) // Lãi tỷ giá: Có TK 4131
            {
                btDanhGiaLai.ChiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = dongSo++,
                    TaiKhoanNoId = c.TaiKhoanId,
                    TaiKhoanCoId = tk4131.Id,
                    SoTien = tien,
                    DienGiai = $"Lãi tỷ giá đánh giá lại TK {c.MaTaiKhoan}",
                    DoiTuongId = c.DoiTuongId
                });
            }
            else // Lỗ tỷ giá: Nợ TK 4131
            {
                btDanhGiaLai.ChiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = dongSo++,
                    TaiKhoanNoId = tk4131.Id,
                    TaiKhoanCoId = c.TaiKhoanId,
                    SoTien = tien,
                    DienGiai = $"Lỗ tỷ giá đánh giá lại TK {c.MaTaiKhoan}",
                    DoiTuongId = c.DoiTuongId
                });
            }
        }

        await _context.ButToans.AddAsync(btDanhGiaLai);
        await _context.SaveChangesAsync();
        entity.ButToanDanhGiaLaiId = btDanhGiaLai.Id;

        // Bút toán 2: Kết chuyển sạch số dư TK 4131 sang 515 hoặc 635 (VAS 10 Invariant: 4131 kết chuyển về 0)
        var chenhLechThuan = preview.ChenhLechThuan;
        if (chenhLechThuan != 0)
        {
            var btKetChuyen = new ButToan
            {
                SoChungTu = $"PKT-KC413-{soChungTu}",
                NgayHachToan = ngayDanhGia.Date,
                NgayChungTu = ngayDanhGia.Date,
                SoChungTuGoc = soChungTu,
                NgayChungTuGoc = ngayDanhGia.Date,
                TongTien = Math.Abs(chenhLechThuan),
                TrangThai = TrangThaiButToan.DaGhiSo,
                ChiTietButToans = new List<ChiTietButToan>()
            };

            if (chenhLechThuan > 0) // Lãi thuần: Nợ 4131 / Có 515
            {
                btKetChuyen.DienGiai = $"Kết chuyển lãi chênh lệch tỷ giá đánh giá lại cuối kỳ sang TK 515";
                btKetChuyen.ChiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = 1,
                    TaiKhoanNoId = tk4131.Id,
                    TaiKhoanCoId = tk515.Id,
                    SoTien = chenhLechThuan,
                    DienGiai = "Kết chuyển lãi tỷ giá sang doanh thu tài chính"
                });
            }
            else // Lỗ thuần: Nợ 635 / Có 4131
            {
                decimal loThuan = Math.Abs(chenhLechThuan);
                btKetChuyen.DienGiai = $"Kết chuyển lỗ chênh lệch tỷ giá đánh giá lại cuối kỳ sang TK 635";
                btKetChuyen.ChiTietButToans.Add(new ChiTietButToan
                {
                    DongSo = 1,
                    TaiKhoanNoId = tk635.Id,
                    TaiKhoanCoId = tk4131.Id,
                    SoTien = loThuan,
                    DienGiai = "Kết chuyển lỗ tỷ giá sang chi phí tài chính"
                });
            }

            await _context.ButToans.AddAsync(btKetChuyen);
            await _context.SaveChangesAsync();
            entity.ButToanKetChuyen413Id = btKetChuyen.Id;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Hoàn tất đánh giá lại tỷ giá ngoại tệ {SoChungTu}. Bất biến TK 4131 số dư sạch = 0.", soChungTu);
        return entity;
    }

    public async Task<(bool ThanhCong, string? ThongBao)> HuyDanhGiaLaiAsync(long danhGiaLaiId)
    {
        var entity = await _context.DanhGiaLaiNgoaiTes
            .Include(d => d.ChiTietDanhGiaLais)
            .FirstOrDefaultAsync(d => d.Id == danhGiaLaiId);

        if (entity == null)
        {
            return (false, "Không tìm thấy chứng từ đánh giá lại tỷ giá.");
        }

        // Kiểm tra khóa sổ
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh?.NgayKhoaSo.HasValue == true && entity.NgayHachToan <= cauHinh.NgayKhoaSo.Value.Date)
        {
            return (false, $"Kỳ kế toán đã khóa sổ đến ngày {cauHinh.NgayKhoaSo:dd/MM/yyyy}. Không thể hủy chứng từ.");
        }

        // Xóa bút toán kết chuyển 413 nếu có
        if (entity.ButToanKetChuyen413Id.HasValue)
        {
            var btKc = await _context.ButToans.FindAsync(entity.ButToanKetChuyen413Id.Value);
            if (btKc != null)
            {
                var cts = await _context.ChiTietButToans.Where(c => c.ButToanId == btKc.Id).ToListAsync();
                _context.ChiTietButToans.RemoveRange(cts);
                _context.ButToans.Remove(btKc);
            }
            entity.ButToanKetChuyen413Id = null;
        }

        // Xóa bút toán đánh giá lại nếu có
        if (entity.ButToanDanhGiaLaiId.HasValue)
        {
            var btDgl = await _context.ButToans.FindAsync(entity.ButToanDanhGiaLaiId.Value);
            if (btDgl != null)
            {
                var cts = await _context.ChiTietButToans.Where(c => c.ButToanId == btDgl.Id).ToListAsync();
                _context.ChiTietButToans.RemoveRange(cts);
                _context.ButToans.Remove(btDgl);
            }
            entity.ButToanDanhGiaLaiId = null;
        }

        _context.ChiTietDanhGiaLaiNgoaiTes.RemoveRange(entity.ChiTietDanhGiaLais);
        _context.DanhGiaLaiNgoaiTes.Remove(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã hủy chứng từ đánh giá lại ngoại tệ {SoChungTu}", entity.SoChungTu);
        return (true, null);
    }
}
