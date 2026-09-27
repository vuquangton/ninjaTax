using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class TaxAuditShieldService : ITaxAuditShieldService
{
    private readonly AppDbContext _context;
    private readonly ITaxFinalizationService _taxFinalizationService;
    private readonly IFinancialReportService _financialReportService;
    private readonly ILogger<TaxAuditShieldService> _logger;

    public TaxAuditShieldService(
        AppDbContext context,
        ITaxFinalizationService taxFinalizationService,
        IFinancialReportService financialReportService,
        ILogger<TaxAuditShieldService> logger)
    {
        _context = context;
        _taxFinalizationService = taxFinalizationService;
        _financialReportService = financialReportService;
        _logger = logger;
    }

    public async Task<TaxAuditShieldReportViewModel> QuetToanBoBayThueAsync(int namTaiChinh)
    {
        var phatHiens = new List<TaxRiskItemViewModel>();
        var tuNgay = new DateTime(namTaiChinh, 1, 1);
        var denNgay = new DateTime(namTaiChinh, 12, 31, 23, 59, 59);

        // ========================================================
        // [BẪY 1] Hóa đơn trên 20 triệu thanh toán bằng tiền mặt
        // ========================================================
        var hoaDonTienMatTren20Tr = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Include(c => c.TaiKhoanNo)
            .Include(c => c.TaiKhoanCo)
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("1111") &&
                        c.TaiKhoanNo != null && (c.TaiKhoanNo.MaTaiKhoan.StartsWith("15") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("21") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("6")) &&
                        c.SoTien >= 20_000_000m)
            .ToListAsync();

        foreach (var c in hoaDonTienMatTren20Tr)
        {
            var thueRuiRo = Math.Round(c.SoTien * 0.20m, 0); // Thuế TNDN 20%
            phatHiens.Add(new TaxRiskItemViewModel
            {
                LoaiBay = LoaiBayThue.Bay1_HoaDonTren20TrTienMat,
                MucDo = MucDoRuiRo.Cao_CanhBaoDo,
                TieuDe = $"BẪY 1: Thanh toán tiền mặt {c.SoTien:N0} VNĐ (>= 20 triệu)",
                MoTaChiTiet = $"Nghiệp vụ hạch toán Nợ {c.TaiKhoanNo?.MaTaiKhoan} / Có {c.TaiKhoanCo?.MaTaiKhoan} số tiền {c.SoTien:N0} VNĐ thanh toán tiền mặt. Khoản chi này sẽ bị loại khỏi chi phí được trừ khi tính thuế TNDN và không được khấu trừ thuế GTGT.",
                MaChungTuLienQuan = c.ButToan?.SoChungTu,
                NgayPhatSinh = c.ButToan?.NgayHachToan,
                SoTienViPham = c.SoTien,
                SoTienThueRuiRo = thueRuiRo,
                SoTienPhatDuKien = Math.Round(thueRuiRo * 0.20m, 0),
                CanCuPhapLy = "Điều 15 Thông tư 219/2013/TT-BTC, Thông tư 26/2015/TT-BTC & Điều 4 Thông tư 96/2015/TT-BTC",
                BienPhapKhacPhuc = "Lập tức thu hồi tiền mặt và chuyển khoản bổ sung qua tài khoản ngân hàng của người bán, hoặc loại khỏi chi phí khi quyết toán thuế TNDN."
            });
        }

        // ========================================================
        // [BẪY 2] Âm quỹ tiền mặt thời điểm (Negative Cash Balance)
        // ========================================================
        var allTienMatEntries = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Include(c => c.TaiKhoanNo)
            .Include(c => c.TaiKhoanCo)
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan.Year == namTaiChinh &&
                        (c.TaiKhoanNo!.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanCo!.MaTaiKhoan.StartsWith("1111")))
            .OrderBy(c => c.ButToan!.NgayHachToan)
            .ToListAsync();

        decimal tonQuyChay = 0m;
        var ngayAmDaGhiNhan = new HashSet<DateTime>();

        foreach (var e in allTienMatEntries)
        {
            if (e.TaiKhoanNo!.MaTaiKhoan.StartsWith("1111"))
                tonQuyChay += e.SoTien;
            if (e.TaiKhoanCo!.MaTaiKhoan.StartsWith("1111"))
                tonQuyChay -= e.SoTien;

            if (tonQuyChay < 0)
            {
                var ngayAm = e.ButToan!.NgayHachToan.Date;
                if (!ngayAmDaGhiNhan.Contains(ngayAm))
                {
                    ngayAmDaGhiNhan.Add(ngayAm);
                    phatHiens.Add(new TaxRiskItemViewModel
                    {
                        LoaiBay = LoaiBayThue.Bay2_AmQuyTienMatThoiDiem,
                        MucDo = MucDoRuiRo.Cao_CanhBaoDo,
                        TieuDe = $"BẪY 2: Âm quỹ tiền mặt tại ngày {ngayAm:dd/MM/yyyy} ({tonQuyChay:N0} VNĐ)",
                        MoTaChiTiet = $"Số dư tồn quỹ tiền mặt TK 1111 bị âm thời điểm xuống mức {tonQuyChay:N0} VNĐ sau chứng từ {e.ButToan?.SoChungTu}. Thanh tra thuế sẽ ấn định doanh thu hoặc bác bỏ tính hợp pháp của các phiếu chi liên quan.",
                        MaChungTuLienQuan = e.ButToan?.SoChungTu,
                        NgayPhatSinh = ngayAm,
                        SoTienViPham = Math.Abs(tonQuyChay),
                        SoTienThueRuiRo = Math.Round(Math.Abs(tonQuyChay) * 0.20m, 0),
                        SoTienPhatDuKien = 5_000_000m,
                        CanCuPhapLy = "Điều 16 Nghị định 125/2020/NĐ-CP & Luật Quản lý Thuế số 38/2019/QH14",
                        BienPhapKhacPhuc = "Lập hợp đồng vay mượn ngắn hạn từ Giám đốc/Cổ đông (TK 341/3388) trước ngày phát sinh chi tiền để cân bằng quỹ tiền mặt."
                    });
                }
            }
        }

        // ========================================================
        // [BẪY 3] Tạm nộp thiếu 80% thuế TNDN 4 quý
        // ========================================================
        var qttTndn = await _taxFinalizationService.LapQuyetToanTndnAsync(namTaiChinh);
        if (qttTndn.ViPham80PhanTram)
        {
            phatHiens.Add(new TaxRiskItemViewModel
            {
                LoaiBay = LoaiBayThue.Bay3_TamNopThieu80PhanTramTndn,
                MucDo = MucDoRuiRo.Cao_CanhBaoDo,
                TieuDe = $"BẪY 3: Vi phạm quy tắc tạm nộp 80% Thuế TNDN 4 quý ({qttTndn.TyLeTamNop:N1}%)",
                MoTaChiTiet = $"Tổng thuế TNDN đã tạm nộp 4 quý là {qttTndn.TongTamNop4Quy:N0} VNĐ, thấp hơn ngưỡng 80% nghĩa vụ cả năm ({qttTndn.Nguong80PhanTram:N0} VNĐ). Doanh nghiệp thiếu hụt {qttTndn.SoTienNopThieu:N0} VNĐ.",
                MaChungTuLienQuan = "QTT-TNDN",
                NgayPhatSinh = new DateTime(namTaiChinh + 1, 1, 31),
                SoTienViPham = qttTndn.SoTienNopThieu,
                SoTienThueRuiRo = qttTndn.SoTienNopThieu,
                SoTienPhatDuKien = qttTndn.TienPhatChamNopDuKien,
                CanCuPhapLy = "Khoản 3 Điều 1 Nghị định 91/2022/NĐ-CP sửa đổi Nghị định 126/2020/NĐ-CP",
                BienPhapKhacPhuc = $"Nộp bổ sung ngay {qttTndn.SoTienNopThieu:N0} VNĐ vào Kho bạc Nhà nước trước khi nộp tờ khai Quyết toán năm để ngắt chu kỳ tính tiền chậm nộp 0.03%/ngày."
            });
        }

        // ========================================================
        // [BẪY 4] Nợ lương người lao động quá hạn ngày 30/03 năm sau
        // ========================================================
        var noLuongCuoiNam = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan.Year == namTaiChinh &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("334"))
            .SumAsync(c => c.SoTien) -
            await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan.Year == namTaiChinh &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("334"))
            .SumAsync(c => c.SoTien);

        if (noLuongCuoiNam > 0 && DateTime.Today > new DateTime(namTaiChinh + 1, 3, 30))
        {
            phatHiens.Add(new TaxRiskItemViewModel
            {
                LoaiBay = LoaiBayThue.Bay4_NoLuongQua30Thang3,
                MucDo = MucDoRuiRo.Cao_CanhBaoDo,
                TieuDe = $"BẪY 4: Nợ lương nhân viên {noLuongCuoiNam:N0} VNĐ quá hạn ngày 30/03",
                MoTaChiTiet = $"Số dư nợ tiền lương TK 334 chưa chi trả thực tế quá hạn ngày 30/03 năm kế tiếp ({namTaiChinh + 1}). Toàn bộ số tiền lương này sẽ bị loại khỏi chi phí được trừ khi quyết toán thuế TNDN nếu không trích lập quỹ dự phòng.",
                MaChungTuLienQuan = "TK-334",
                NgayPhatSinh = new DateTime(namTaiChinh + 1, 3, 30),
                SoTienViPham = noLuongCuoiNam,
                SoTienThueRuiRo = Math.Round(noLuongCuoiNam * 0.20m, 0),
                SoTienPhatDuKien = 0m,
                CanCuPhapLy = "Điều 4 Thông tư 96/2015/TT-BTC sửa đổi Thông tư 78/2014/TT-BTC",
                BienPhapKhacPhuc = "Thực hiện chi trả lương ngay bằng tiền mặt hoặc chuyển khoản và trích lập Quỹ dự phòng tiền lương (tối đa 17% quỹ lương thực hiện)."
            });
        }

        // ========================================================
        // [BẪY 5] Khấu hao TSCĐ hoặc phân bổ CCDC vượt quá 36 tháng
        // ========================================================
        var ccdcViPham = await _context.TaiSanCoDinhs
            .Where(t => t.LoaiTaiSan == LoaiTaiSan.CongCuDungCu && t.ThoiGianSuDungThang > 36)
            .ToListAsync();

        foreach (var ccdc in ccdcViPham)
        {
            var chiPhiNam = ccdc.NguyenGia / ccdc.ThoiGianSuDungThang * 12;
            phatHiens.Add(new TaxRiskItemViewModel
            {
                LoaiBay = LoaiBayThue.Bay5_KhauHaoVuotKhungHoacCcdcQua36Thang,
                MucDo = MucDoRuiRo.TrungBinh,
                TieuDe = $"BẪY 5: CCDC {ccdc.MaTaiSan} phân bổ {ccdc.ThoiGianSuDungThang} tháng (> 36 tháng)",
                MoTaChiTiet = $"Công cụ dụng cụ {ccdc.TenTaiSan} có thời gian phân bổ {ccdc.ThoiGianSuDungThang} tháng vượt mức tối đa 36 tháng quy định. Phần chi phí phân bổ tương ứng có nguy cơ bị loại khi thanh tra.",
                MaChungTuLienQuan = ccdc.MaTaiSan,
                NgayPhatSinh = ccdc.NgayGhiTang,
                SoTienViPham = chiPhiNam,
                SoTienThueRuiRo = Math.Round(chiPhiNam * 0.20m, 0),
                SoTienPhatDuKien = 0m,
                CanCuPhapLy = "Điều 4 Thông tư 96/2015/TT-BTC",
                BienPhapKhacPhuc = "Điều chỉnh lại thời gian phân bổ về đúng tối đa 36 tháng trong danh mục tài sản."
            });
        }

        // ========================================================
        // [BẪY 6] Thiếu khấu trừ 10% thuế TNCN thời vụ không có MST / Cam kết 08
        // ========================================================
        var nvThoiVuThieuThue = await _context.ChiTietLuongNhanViens
            .Include(c => c.NhanVien)
            .Include(c => c.BangLuongThang)
            .Where(c => c.BangLuongThang != null && c.BangLuongThang.KyKeToan.StartsWith(namTaiChinh.ToString()) &&
                        c.NhanVien != null && c.NhanVien.LoaiHopDong == LoaiHopDongLaoDong.ThuViecThoiVu &&
                        c.TongThuNhap >= 2_000_000m &&
                        !c.NhanVien.CoCamKet08 &&
                        c.ThueTncnKhauTru < (c.TongThuNhap * 0.10m - 1000m))
            .ToListAsync();

        foreach (var nv in nvThoiVuThieuThue)
        {
            var thieu = Math.Round(nv.TongThuNhap * 0.10m, 0) - nv.ThueTncnKhauTru;
            phatHiens.Add(new TaxRiskItemViewModel
            {
                LoaiBay = LoaiBayThue.Bay6_ThieuKhauTru10PhanTramThoiVu,
                MucDo = MucDoRuiRo.Cao_CanhBaoDo,
                TieuDe = $"BẪY 6: Nhân viên thời vụ {nv.NhanVien?.MaNhanVien} thiếu khấu trừ 10% thuế TNCN",
                MoTaChiTiet = $"Nhân sự {nv.NhanVien?.HoTen} ký HĐ thời vụ có thu nhập {nv.TongThuNhap:N0} VNĐ (>= 2M) trong kỳ {nv.BangLuongThang?.KyKeToan} không có Cam kết 08 nhưng chỉ khấu trừ {nv.ThueTncnKhauTru:N0} VNĐ (thiếu {thieu:N0} VNĐ).",
                MaChungTuLienQuan = nv.BangLuongThang?.SoChungTu,
                NgayPhatSinh = nv.BangLuongThang?.NgayLap,
                SoTienViPham = nv.TongThuNhap,
                SoTienThueRuiRo = thieu,
                SoTienPhatDuKien = Math.Round(thieu * 0.20m, 0),
                CanCuPhapLy = "Điểm i Khoản 1 Điều 25 Thông tư 111/2013/TT-BTC",
                BienPhapKhacPhuc = "Thu thập bổ sung Cam kết Mẫu 08/CK-TNCN kèm mã số thuế cá nhân hoặc khấu trừ truy thu bổ sung 10%."
            });
        }

        // ========================================================
        // [BẪY 7] Bán hàng dưới giá vốn (Selling Below Cost)
        // ========================================================
        var b02 = await _financialReportService.LapBaoCaoB02Async(namTaiChinh);
        if (b02.LoiNhuanGop < 0)
        {
            var loGop = Math.Abs(b02.LoiNhuanGop);
            phatHiens.Add(new TaxRiskItemViewModel
            {
                LoaiBay = LoaiBayThue.Bay7_BanHangDuoiGiaVon,
                MucDo = MucDoRuiRo.Cao_CanhBaoDo,
                TieuDe = $"BẪY 7: Bán hàng dưới giá vốn (Lỗ gộp {loGop:N0} VNĐ)",
                MoTaChiTiet = $"Doanh thu thuần ({b02.DoanhThuThuan:N0} VNĐ) nhỏ hơn Giá vốn hàng bán ({b02.DoanhThuThuan - b02.LoiNhuanGop:N0} VNĐ), phát sinh lỗ gộp {loGop:N0} VNĐ. Thanh tra thuế sẽ yêu cầu giải trình và có thể ấn định lại giá bán.",
                MaChungTuLienQuan = "B02-DN",
                NgayPhatSinh = new DateTime(namTaiChinh, 12, 31),
                SoTienViPham = loGop,
                SoTienThueRuiRo = Math.Round(loGop * 0.20m, 0),
                SoTienPhatDuKien = 0m,
                CanCuPhapLy = "Khoản 1 Điều 50 Luật Quản lý Thuế số 38/2019/QH14",
                BienPhapKhacPhuc = "Chuẩn bị đầy đủ hồ sơ thanh lý, hàng hóa suy giảm phẩm chất hoặc chính sách khuyến mãi đã đăng ký với Sở Công Thương."
            });
        }

        // ========================================================
        // [BẪY 8] Chi phí lãi vay vượt mức khi vốn điều lệ chưa góp đủ
        // ========================================================
        var chiPhiLaiVay = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan.Year == namTaiChinh &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("635"))
            .SumAsync(c => c.SoTien);

        var vonGopThucTe = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("411"))
            .SumAsync(c => c.SoTien);

        // Giả sử vốn điều lệ cam kết trên GPKD (nếu vốn góp thực tế = 0 mà có lãi vay)
        if (chiPhiLaiVay > 0 && vonGopThucTe == 0)
        {
            phatHiens.Add(new TaxRiskItemViewModel
            {
                LoaiBay = LoaiBayThue.Bay8_LaiVayVuot30EbitdaLienKet,
                MucDo = MucDoRuiRo.Cao_CanhBaoDo,
                TieuDe = $"BẪY 8: Chi phí lãi vay {chiPhiLaiVay:N0} VNĐ khi chưa góp đủ vốn điều lệ",
                MoTaChiTiet = $"Doanh nghiệp phát sinh chi phí lãi vay TK 635 trong khi số dư vốn góp thực tế TK 411 chưa hoàn tất. Chi phí lãi vay tương ứng với phần vốn điều lệ còn thiếu sẽ bị loại trừ hoàn toàn.",
                MaChungTuLienQuan = "TK-635",
                NgayPhatSinh = new DateTime(namTaiChinh, 12, 31),
                SoTienViPham = chiPhiLaiVay,
                SoTienThueRuiRo = Math.Round(chiPhiLaiVay * 0.20m, 0),
                SoTienPhatDuKien = 0m,
                CanCuPhapLy = "Điểm 2.18 Khoản 2 Điều 6 Thông tư 78/2014/TT-BTC",
                BienPhapKhacPhuc = "Hoàn tất việc góp đủ vốn điều lệ theo đúng tiến độ trên Giấy chứng nhận đăng ký kinh doanh."
            });
        }

        var tongPhatHien = phatHiens.Count;
        var soDo = phatHiens.Count(p => p.MucDo == MucDoRuiRo.Cao_CanhBaoDo);
        var soVang = phatHiens.Count(p => p.MucDo == MucDoRuiRo.TrungBinh);
        var tongChiPhi = phatHiens.Sum(p => p.SoTienViPham);
        var tongThue = phatHiens.Sum(p => p.SoTienThueRuiRo);
        var tongPhat = phatHiens.Sum(p => p.SoTienPhatDuKien);

        return new TaxAuditShieldReportViewModel
        {
            NamTaiChinh = namTaiChinh,
            NgayQuet = DateTime.Now,
            TongSoPhatHien = tongPhatHien,
            SoCanhBaoDoNghiemTrong = soDo,
            SoCanhBaoVangChuY = soVang,
            TongTienChiPhiRuiRo = tongChiPhi,
            TongTienThueTruyThuUocTinh = tongThue,
            TongTienPhatChamNopUocTinh = tongPhat,
            PhatHiens = phatHiens
        };
    }

    public async Task<TaxAuditShieldReportViewModel> LuuBaoCaoTaxShieldAsync(int namTaiChinh)
    {
        var model = await QuetToanBoBayThueAsync(namTaiChinh);

        var existing = await _context.TaxAuditRiskShieldReports
            .Include(r => r.ChiTietRuiRos)
            .FirstOrDefaultAsync(r => r.NamTaiChinh == namTaiChinh);

        if (existing == null)
        {
            existing = new TaxAuditRiskShieldReport
            {
                NamTaiChinh = namTaiChinh,
                NgayQuet = DateTime.UtcNow,
                TongSoPhatHien = model.TongSoPhatHien,
                SoCanhBaoDoNghiemTrong = model.SoCanhBaoDoNghiemTrong,
                SoCanhBaoVangChuY = model.SoCanhBaoVangChuY,
                TongTienChiPhiRuiRo = model.TongTienChiPhiRuiRo,
                TongTienThueTruyThuUocTinh = model.TongTienThueTruyThuUocTinh,
                TongTienPhatChamNopUocTinh = model.TongTienPhatChamNopUocTinh,
                ChiTietRuiRos = model.PhatHiens.Select(p => new TaxRiskFinding
                {
                    LoaiBay = p.LoaiBay,
                    MucDo = p.MucDo,
                    TieuDe = p.TieuDe,
                    MoTaChiTiet = p.MoTaChiTiet,
                    MaChungTuLienQuan = p.MaChungTuLienQuan,
                    NgayPhatSinh = p.NgayPhatSinh,
                    SoTienViPham = p.SoTienViPham,
                    SoTienThueRuiRo = p.SoTienThueRuiRo,
                    SoTienPhatDuKien = p.SoTienPhatDuKien,
                    CanCuPhapLy = p.CanCuPhapLy,
                    BienPhapKhacPhuc = p.BienPhapKhacPhuc
                }).ToList()
            };
            _context.TaxAuditRiskShieldReports.Add(existing);
        }
        else
        {
            existing.NgayQuet = DateTime.UtcNow;
            existing.TongSoPhatHien = model.TongSoPhatHien;
            existing.SoCanhBaoDoNghiemTrong = model.SoCanhBaoDoNghiemTrong;
            existing.SoCanhBaoVangChuY = model.SoCanhBaoVangChuY;
            existing.TongTienChiPhiRuiRo = model.TongTienChiPhiRuiRo;
            existing.TongTienThueTruyThuUocTinh = model.TongTienThueTruyThuUocTinh;
            existing.TongTienPhatChamNopUocTinh = model.TongTienPhatChamNopUocTinh;
            existing.ChiTietRuiRos = model.PhatHiens.Select(p => new TaxRiskFinding
            {
                LoaiBay = p.LoaiBay,
                MucDo = p.MucDo,
                TieuDe = p.TieuDe,
                MoTaChiTiet = p.MoTaChiTiet,
                MaChungTuLienQuan = p.MaChungTuLienQuan,
                NgayPhatSinh = p.NgayPhatSinh,
                SoTienViPham = p.SoTienViPham,
                SoTienThueRuiRo = p.SoTienThueRuiRo,
                SoTienPhatDuKien = p.SoTienPhatDuKien,
                CanCuPhapLy = p.CanCuPhapLy,
                BienPhapKhacPhuc = p.BienPhapKhacPhuc
            }).ToList();
        }

        await _context.SaveChangesAsync();
        return model;
    }
}
