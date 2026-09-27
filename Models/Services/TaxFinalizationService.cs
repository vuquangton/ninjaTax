using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class TaxFinalizationService : ITaxFinalizationService
{
    private readonly AppDbContext _context;
    private readonly IFinancialReportService _financialReportService;
    private readonly ILogger<TaxFinalizationService> _logger;

    public TaxFinalizationService(
        AppDbContext context,
        IFinancialReportService financialReportService,
        ILogger<TaxFinalizationService> logger)
    {
        _context = context;
        _financialReportService = financialReportService;
        _logger = logger;
    }

    public async Task<QuyetToanTndnViewModel> LapQuyetToanTndnAsync(int namTaiChinh)
    {
        var b02 = await _financialReportService.LapBaoCaoB02Async(namTaiChinh);
        var tuNgay = new DateTime(namTaiChinh, 1, 1);
        var denNgay = new DateTime(namTaiChinh, 12, 31, 23, 59, 59);

        var a1 = b02.LoiNhuanTruocThue;

        // B4: Quét các khoản chi không được trừ
        var danhSachB4 = new List<DongChiPhiB4ViewModel>();

        // 1. Mua hàng >= 20 triệu thanh toán tiền mặt
        var muaHangTienMatLon = await _context.ChiTietButToans
            .Include(c => c.ButToan)
            .Include(c => c.TaiKhoanNo)
            .Include(c => c.TaiKhoanCo)
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("1111") &&
                        c.TaiKhoanNo != null && (c.TaiKhoanNo.MaTaiKhoan.StartsWith("15") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("21") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("6")) &&
                        c.SoTien >= 20_000_000m)
            .ToListAsync();

        foreach (var item in muaHangTienMatLon)
        {
            danhSachB4.Add(new DongChiPhiB4ViewModel
            {
                LoaiViPham = LoaiViPhamB4.HoaDonTren20TrTienMat,
                MoTa = $"Chi tiền mặt >= 20 triệu không qua ngân hàng: {item.DienGiai}",
                SoTien = item.SoTien,
                SoChungTuLienQuan = item.ButToan?.SoChungTu,
                NgayChungTu = item.ButToan?.NgayChungTu,
                CanCuPhapLy = "Điều 4 Thông tư 96/2015/TT-BTC & Điều 15 Thông tư 219/2013/TT-BTC"
            });
        }

        // 2. Chi phí CCDC vượt quá 36 tháng
        var ccdcVuotKhung = await _context.TaiSanCoDinhs
            .Where(t => t.LoaiTaiSan == LoaiTaiSan.CongCuDungCu && t.ThoiGianSuDungThang > 36)
            .ToListAsync();

        foreach (var ccdc in ccdcVuotKhung)
        {
            var chiPhiPhanBo = ccdc.NguyenGia / ccdc.ThoiGianSuDungThang * 12;
            danhSachB4.Add(new DongChiPhiB4ViewModel
            {
                LoaiViPham = LoaiViPhamB4.KhauHaoVuotKhung,
                MoTa = $"CCDC {ccdc.TenTaiSan} có thời gian phân bổ {ccdc.ThoiGianSuDungThang} tháng vượt trần 36 tháng",
                SoTien = chiPhiPhanBo,
                SoChungTuLienQuan = ccdc.MaTaiSan,
                NgayChungTu = ccdc.NgayGhiTang,
                CanCuPhapLy = "Điều 4 Thông tư 96/2015/TT-BTC sửa đổi TT 78/2014"
            });
        }

        var b4 = danhSachB4.Sum(x => x.SoTien);
        var b7 = 0m; // Thu nhập miễn thuế mặc định 0
        var b14 = Math.Max(0m, a1 + b4 - b7);
        var c1 = b14; // Thu nhập tính thuế sau bù lỗ
        var c7 = Math.Round(c1 * 0.20m, 0); // Thuế TNDN 20%

        // Tạm nộp 4 quý
        var tamNopQ1 = await TinhTndnDaNopTrongKhoangAsync(new DateTime(namTaiChinh, 1, 1), new DateTime(namTaiChinh, 3, 31, 23, 59, 59));
        var tamNopQ2 = await TinhTndnDaNopTrongKhoangAsync(new DateTime(namTaiChinh, 4, 1), new DateTime(namTaiChinh, 6, 30, 23, 59, 59));
        var tamNopQ3 = await TinhTndnDaNopTrongKhoangAsync(new DateTime(namTaiChinh, 7, 1), new DateTime(namTaiChinh, 9, 30, 23, 59, 59));
        var tamNopQ4 = await TinhTndnDaNopTrongKhoangAsync(new DateTime(namTaiChinh, 10, 1), new DateTime(namTaiChinh + 1, 1, 31, 23, 59, 59));

        var tongTamNop = tamNopQ1 + tamNopQ2 + tamNopQ3 + tamNopQ4;
        var nguong80 = Math.Round(c7 * 0.80m, 0);
        var tyLe = c7 > 0 ? Math.Round((tongTamNop / c7) * 100m, 2) : 100m;
        var viPham = tongTamNop < nguong80;
        var soTienThieu = viPham ? (nguong80 - tongTamNop) : 0m;

        // Tính tiền phạt nộp chậm theo Nghị định 91/2022/NĐ-CP (0.03%/ngày)
        var ngayHetHanTamNop = new DateTime(namTaiChinh + 1, 1, 31);
        var ngayQuyetToan = new DateTime(namTaiChinh + 1, 3, 31);
        var soNgayChamNop = (DateTime.Today > ngayHetHanTamNop)
            ? (int)(DateTime.Today - ngayHetHanTamNop).TotalDays
            : (int)(ngayQuyetToan - ngayHetHanTamNop).TotalDays;

        if (soNgayChamNop <= 0) soNgayChamNop = 59; // Ước tính từ 01/02 đến 31/03 theo chuẩn QT năm
        var tienPhat = viPham ? Math.Round(soTienThieu * 0.0003m * soNgayChamNop, 0) : 0m;

        return new QuyetToanTndnViewModel
        {
            NamQuyetToan = namTaiChinh,
            SoChungTu = $"QTT-TNDN-{namTaiChinh}",
            NgayLap = DateTime.Today,
            TrangThai = TrangThaiQuyetToan.DangLap,
            ChiTieuA1_LoiNhuanKeToan = a1,
            ChiTieuB4_ChiPhiKhongDuocTru = b4,
            ChiTieuB7_ThuNhapMienThue = b7,
            ChiTieuB14_ThuNhapChiuThue = b14,
            ChiTieuC1_ThuNhapTinhThue = c1,
            ChiTieuC4_LoKetChuyen = 0m,
            ThueSuat = 20.0m,
            ChiTieuC7_ThueTndnPhaiNop = c7,
            TamNopQ1 = tamNopQ1,
            TamNopQ2 = tamNopQ2,
            TamNopQ3 = tamNopQ3,
            TamNopQ4 = tamNopQ4,
            TongTamNop4Quy = tongTamNop,
            Nguong80PhanTram = nguong80,
            TyLeTamNop = tyLe,
            ViPham80PhanTram = viPham,
            SoTienNopThieu = soTienThieu,
            SoNgayChamNop = viPham ? soNgayChamNop : 0,
            TienPhatChamNopDuKien = tienPhat,
            DanhSachChiPhiB4 = danhSachB4
        };
    }

    private async Task<decimal> TinhTndnDaNopTrongKhoangAsync(DateTime tuNgay, DateTime denNgay)
    {
        return await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("3334"))
            .SumAsync(c => c.SoTien);
    }

    public async Task<QuyetToanTncnViewModel> LapQuyetToanTncnAsync(int namTaiChinh)
    {
        var nhanViens = await _context.NhanViens
            .Where(nv => nv.DangLamViec)
            .ToListAsync();

        var bangLuongs = await _context.BangLuongThangs
            .Include(bl => bl.ChiTietLuongNhanViens)
            .Where(bl => bl.KyKeToan.StartsWith(namTaiChinh.ToString()))
            .ToListAsync();

        var bangKe051 = new List<Dong051ViewModel>();
        var bangKe052 = new List<Dong052ViewModel>();

        foreach (var nv in nhanViens)
        {
            var chiTietsLuong = bangLuongs
                .SelectMany(b => b.ChiTietLuongNhanViens)
                .Where(c => c.NhanVienId == nv.Id)
                .ToList();

            var tongThuNhap = chiTietsLuong.Sum(c => c.TongThuNhap);
            var thueDaKhauTru = chiTietsLuong.Sum(c => c.ThueTncnKhauTru);
            var baoHiem = chiTietsLuong.Sum(c => c.BhxhNld + c.BhytNld + c.BhtnNld);

            if (nv.LoaiHopDong == LoaiHopDongLaoDong.HopDongDaiHan)
            {
                var giamTruBanThan = 132_000_000m; // 11M x 12 tháng
                var giamTruNpt = nv.SoNguoiPhuThuoc * 4_400_000m * 12; // 4.4M x 12
                var tntt = Math.Max(0m, tongThuNhap - baoHiem - giamTruBanThan - giamTruNpt);

                // Tính thuế quyết toán theo biểu lũy tiến từng phần năm
                var thueQtt = TinhThueLuyTienNam(tntt);
                var thueNopThua = Math.Max(0m, thueDaKhauTru - thueQtt);
                var thueConPhaiNop = Math.Max(0m, thueQtt - thueDaKhauTru);

                bangKe051.Add(new Dong051ViewModel
                {
                    NhanVienId = nv.Id,
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    MaSoThue = nv.MaSoThue,
                    SoCccd = nv.SoCccd,
                    CaNhanUyQuyenQuyetToan = true,
                    TongThuNhapChiuThue = tongThuNhap,
                    ThuNhapMienThue = 0m,
                    GiamTruBanThan = giamTruBanThan,
                    SoNguoiPhuThuoc = nv.SoNguoiPhuThuoc,
                    GiamTruNguoiPhuThuoc = giamTruNpt,
                    BaoHiemBatBuoc = baoHiem,
                    ThuNhapTinhThue = tntt,
                    ThueDaKhauTru = thueDaKhauTru,
                    ThuePhaiNopSauQuyetToan = thueQtt,
                    ThueNopThua = thueNopThua,
                    ThueConPhaiNop = thueConPhaiNop
                });
            }
            else
            {
                bangKe052.Add(new Dong052ViewModel
                {
                    NhanVienId = nv.Id,
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    MaSoThue = nv.MaSoThue,
                    SoCccd = nv.SoCccd,
                    CoCamKet08 = nv.CoCamKet08,
                    TongThuNhapChiuThue = tongThuNhap,
                    ThueTncnDaKhauTru10 = thueDaKhauTru
                });
            }
        }

        var tongTnct = bangKe051.Sum(x => x.TongThuNhapChiuThue) + bangKe052.Sum(x => x.TongThuNhapChiuThue);
        var tongThueKhauTru = bangKe051.Sum(x => x.ThueDaKhauTru) + bangKe052.Sum(x => x.ThueTncnDaKhauTru10);
        var tongThuePhaiNop = bangKe051.Sum(x => x.ThuePhaiNopSauQuyetToan) + bangKe052.Sum(x => x.ThueTncnDaKhauTru10);

        return new QuyetToanTncnViewModel
        {
            NamQuyetToan = namTaiChinh,
            SoChungTu = $"QTT-TNCN-{namTaiChinh}",
            NgayLap = DateTime.Today,
            TrangThai = TrangThaiQuyetToan.DangLap,
            TongSoNhanVienQuyetToan = nhanViens.Count,
            SoNhanVienUyQuyen = bangKe051.Count(x => x.CaNhanUyQuyenQuyetToan),
            TongThuNhapChiuThue = tongTnct,
            TongThuNhapMienThue = 0m,
            TongGiamTruGiaCanh = bangKe051.Sum(x => x.GiamTruBanThan + x.GiamTruNguoiPhuThuoc),
            TongBaoHiemBatBuoc = bangKe051.Sum(x => x.BaoHiemBatBuoc),
            TongThuNhapTinhThue = bangKe051.Sum(x => x.ThuNhapTinhThue),
            TongThueDaKhauTru = tongThueKhauTru,
            TongThuePhaiNopSauQtt = tongThuePhaiNop,
            TongThueNopThua = bangKe051.Sum(x => x.ThueNopThua),
            TongThueConPhaiNopThem = bangKe051.Sum(x => x.ThueConPhaiNop),
            BangKe051 = bangKe051,
            BangKe052 = bangKe052
        };
    }

    private static decimal TinhThueLuyTienNam(decimal tnttNam)
    {
        if (tnttNam <= 0) return 0m;
        // Biểu thuế lũy tiến từng phần theo năm (TT 111/2013)
        // Bậc 1: đến 60 tr (5%)
        // Bậc 2: trên 60 đến 120 tr (10%)
        // Bậc 3: trên 120 đến 216 tr (15%)
        // Bậc 4: trên 216 đến 384 tr (20%)
        // Bậc 5: trên 384 đến 624 tr (25%)
        // Bậc 6: trên 624 đến 960 tr (30%)
        // Bậc 7: trên 960 tr (35%)
        decimal thue = 0m;
        if (tnttNam <= 60_000_000m)
            thue = tnttNam * 0.05m;
        else if (tnttNam <= 120_000_000m)
            thue = 3_000_000m + (tnttNam - 60_000_000m) * 0.10m;
        else if (tnttNam <= 216_000_000m)
            thue = 9_000_000m + (tnttNam - 120_000_000m) * 0.15m;
        else if (tnttNam <= 384_000_000m)
            thue = 23_400_000m + (tnttNam - 216_000_000m) * 0.20m;
        else if (tnttNam <= 624_000_000m)
            thue = 57_000_000m + (tnttNam - 384_000_000m) * 0.25m;
        else if (tnttNam <= 960_000_000m)
            thue = 117_000_000m + (tnttNam - 624_000_000m) * 0.30m;
        else
            thue = 217_800_000m + (tnttNam - 960_000_000m) * 0.35m;

        return Math.Round(thue, 0);
    }

    public async Task LuuQuyetToanTndnAsync(QuyetToanTndnViewModel model)
    {
        var existing = await _context.QuyetToanThueTndns
            .Include(q => q.ChiPhiKhongHopLies)
            .FirstOrDefaultAsync(q => q.NamQuyetToan == model.NamQuyetToan);

        if (existing == null)
        {
            existing = new QuyetToanThueTndn
            {
                NamQuyetToan = model.NamQuyetToan,
                SoChungTu = model.SoChungTu,
                NgayLap = model.NgayLap,
                TrangThai = TrangThaiQuyetToan.DaDuyet,
                ChiTieuA1_LoiNhuanKeToan = model.ChiTieuA1_LoiNhuanKeToan,
                ChiTieuB4_ChiPhiKhongDuocTru = model.ChiTieuB4_ChiPhiKhongDuocTru,
                ChiTieuB7_ThuNhapMienThue = model.ChiTieuB7_ThuNhapMienThue,
                ChiTieuB14_ThuNhapChiuThue = model.ChiTieuB14_ThuNhapChiuThue,
                ChiTieuC1_ThuNhapTinhThue = model.ChiTieuC1_ThuNhapTinhThue,
                ChiTieuC4_LoKetChuyen = model.ChiTieuC4_LoKetChuyen,
                ThueSuat = model.ThueSuat,
                ChiTieuC7_ThueTndnPhaiNop = model.ChiTieuC7_ThueTndnPhaiNop,
                TamNopQ1 = model.TamNopQ1,
                TamNopQ2 = model.TamNopQ2,
                TamNopQ3 = model.TamNopQ3,
                TamNopQ4 = model.TamNopQ4,
                TongTamNop4Quy = model.TongTamNop4Quy,
                Nguong80PhanTram = model.Nguong80PhanTram,
                TyLeTamNop = model.TyLeTamNop,
                ViPham80PhanTram = model.ViPham80PhanTram,
                SoTienNopThieu = model.SoTienNopThieu,
                SoNgayChamNop = model.SoNgayChamNop,
                TienPhatChamNopDuKien = model.TienPhatChamNopDuKien
            };
            _context.QuyetToanThueTndns.Add(existing);
        }
        else
        {
            existing.TrangThai = TrangThaiQuyetToan.DaDuyet;
            existing.ChiTieuA1_LoiNhuanKeToan = model.ChiTieuA1_LoiNhuanKeToan;
            existing.ChiTieuB4_ChiPhiKhongDuocTru = model.ChiTieuB4_ChiPhiKhongDuocTru;
            existing.ChiTieuB14_ThuNhapChiuThue = model.ChiTieuB14_ThuNhapChiuThue;
            existing.ChiTieuC1_ThuNhapTinhThue = model.ChiTieuC1_ThuNhapTinhThue;
            existing.ChiTieuC7_ThueTndnPhaiNop = model.ChiTieuC7_ThueTndnPhaiNop;
            existing.TongTamNop4Quy = model.TongTamNop4Quy;
            existing.ViPham80PhanTram = model.ViPham80PhanTram;
            existing.SoTienNopThieu = model.SoTienNopThieu;
            existing.TienPhatChamNopDuKien = model.TienPhatChamNopDuKien;
        }

        await _context.SaveChangesAsync();
    }

    public async Task LuuQuyetToanTncnAsync(QuyetToanTncnViewModel model)
    {
        var existing = await _context.QuyetToanThueTncns
            .Include(q => q.BangKe051s)
            .Include(q => q.BangKe052s)
            .FirstOrDefaultAsync(q => q.NamQuyetToan == model.NamQuyetToan);

        if (existing == null)
        {
            existing = new QuyetToanThueTncn
            {
                NamQuyetToan = model.NamQuyetToan,
                SoChungTu = model.SoChungTu,
                NgayLap = model.NgayLap,
                TrangThai = TrangThaiQuyetToan.DaDuyet,
                TongSoNhanVienQuyetToan = model.TongSoNhanVienQuyetToan,
                SoNhanVienUyQuyen = model.SoNhanVienUyQuyen,
                TongThuNhapChiuThue = model.TongThuNhapChiuThue,
                TongThuNhapMienThue = model.TongThuNhapMienThue,
                TongGiamTruGiaCanh = model.TongGiamTruGiaCanh,
                TongBaoHiemBatBuoc = model.TongBaoHiemBatBuoc,
                TongThuNhapTinhThue = model.TongThuNhapTinhThue,
                TongThueDaKhauTru = model.TongThueDaKhauTru,
                TongThuePhaiNopSauQtt = model.TongThuePhaiNopSauQtt,
                TongThueNopThua = model.TongThueNopThua,
                TongThueConPhaiNopThem = model.TongThueConPhaiNopThem
            };
            _context.QuyetToanThueTncns.Add(existing);
        }
        else
        {
            existing.TrangThai = TrangThaiQuyetToan.DaDuyet;
            existing.TongThuNhapChiuThue = model.TongThuNhapChiuThue;
            existing.TongThueDaKhauTru = model.TongThueDaKhauTru;
            existing.TongThuePhaiNopSauQtt = model.TongThuePhaiNopSauQtt;
        }

        await _context.SaveChangesAsync();
    }
}
