using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class FinancialReportService : IFinancialReportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinancialReportService> _logger;

    public FinancialReportService(
        AppDbContext context,
        ILogger<FinancialReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BctcDashboardViewModel> LayDashboardAsync(int namTaiChinh)
    {
        var b01 = await LapBaoCaoB01Async(namTaiChinh);
        var b02 = await LapBaoCaoB02Async(namTaiChinh);
        var b03 = await LapBaoCaoB03Async(namTaiChinh);

        var existingBctc = await _context.BaoCaoTaiChinhNams
            .FirstOrDefaultAsync(b => b.NamTaiChinh == namTaiChinh);

        return new BctcDashboardViewModel
        {
            NamTaiChinh = namTaiChinh,
            TrangThai = existingBctc?.TrangThai ?? TrangThaiBaoCaoTaiChinh.DangLap,
            TongTaiSan = b01.TongTaiSanCuoiNam,
            TongNguonVon = b01.TongNguonVonCuoiNam,
            DoanhThuThuan = b02.DoanhThuThuan,
            LoiNhuanGop = b02.LoiNhuanGop,
            LoiNhuanTruocThue = b02.LoiNhuanTruocThue,
            ThueTndnHienHanh = b02.ThueTndnHienHanh,
            LoiNhuanSauThue = b02.LoiNhuanSauThue,
            LuuChuyenHdkd = b03.LuuChuyenHdkd,
            LuuChuyenHddt = b03.LuuChuyenHddt,
            LuuChuyenHdtc = b03.LuuChuyenHdtc,
            TienDauKy = b03.TienDauKy,
            TienCuoiKy = b03.TienCuoiKy
        };
    }

    public async Task<BaoCaoTinhHinhTaiChinhViewModel> LapBaoCaoB01Async(int namTaiChinh)
    {
        var tuNgay = new DateTime(namTaiChinh, 1, 1);
        var denNgay = new DateTime(namTaiChinh, 12, 31, 23, 59, 59);

        // Tính B02 trước để lấy Lợi nhuận sau thuế năm nay kết chuyển vào 4212
        var b02 = await LapBaoCaoB02Async(namTaiChinh);

        // 1. Tiền và tương đương tiền (Mã 110: TK 1111 + 1121)
        var tienMatCuoiNam = await TinhSoDuNoTaiKhoanAsync("1111", denNgay);
        var tienGuiCuoiNam = await TinhSoDuNoTaiKhoanAsync("1121", denNgay);
        var ma110_CuoiNam = tienMatCuoiNam + tienGuiCuoiNam;

        var tienMatDauNam = await TinhSoDuNoTaiKhoanAsync("1111", tuNgay.AddDays(-1));
        var tienGuiDauNam = await TinhSoDuNoTaiKhoanAsync("1121", tuNgay.AddDays(-1));
        var ma110_DauNam = tienMatDauNam + tienGuiDauNam;

        // 2. Phải thu khách hàng (Mã 131: Dư Nợ TK 131 theo từng đối tượng)
        var ma131_CuoiNam = await TinhSoDuNoTaiKhoanLuongTinhAsync("131", denNgay);
        var ma131_DauNam = await TinhSoDuNoTaiKhoanLuongTinhAsync("131", tuNgay.AddDays(-1));

        // 3. Trả trước người bán (Mã 132: Dư Nợ TK 331 theo từng đối tượng)
        var ma132_CuoiNam = await TinhSoDuNoTaiKhoanLuongTinhAsync("331", denNgay);
        var ma132_DauNam = await TinhSoDuNoTaiKhoanLuongTinhAsync("331", tuNgay.AddDays(-1));

        // 4. Hàng tồn kho (Mã 140: Nhóm TK 15)
        var ma140_CuoiNam = await TinhSoDuNoTaiKhoanAsync("15", denNgay);
        var ma140_DauNam = await TinhSoDuNoTaiKhoanAsync("15", tuNgay.AddDays(-1));

        // TỔNG TÀI SẢN NGẮN HẠN (MÃ 100)
        var ma100_CuoiNam = ma110_CuoiNam + ma131_CuoiNam + ma132_CuoiNam + ma140_CuoiNam;
        var ma100_DauNam = ma110_DauNam + ma131_DauNam + ma132_DauNam + ma140_DauNam;

        // 5. Tài sản cố định hữu hình (Mã 220 = Nguyên giá 221 - Hao mòn 222)
        var ma221_CuoiNam = await TinhSoDuNoTaiKhoanAsync("211", denNgay);
        var ma221_DauNam = await TinhSoDuNoTaiKhoanAsync("211", tuNgay.AddDays(-1));

        var ma222_CuoiNam = await TinhSoDuCoTaiKhoanAsync("214", denNgay);
        var ma222_DauNam = await TinhSoDuCoTaiKhoanAsync("214", tuNgay.AddDays(-1));

        var ma220_CuoiNam = ma221_CuoiNam - ma222_CuoiNam;
        var ma220_DauNam = ma221_DauNam - ma222_DauNam;

        // 6. Chi phí trả trước dài hạn (Mã 260: TK 242)
        var ma260_CuoiNam = await TinhSoDuNoTaiKhoanAsync("242", denNgay);
        var ma260_DauNam = await TinhSoDuNoTaiKhoanAsync("242", tuNgay.AddDays(-1));

        // TỔNG TÀI SẢN DÀI HẠN (MÃ 200)
        var ma200_CuoiNam = ma220_CuoiNam + ma260_CuoiNam;
        var ma200_DauNam = ma220_DauNam + ma260_DauNam;

        // TỔNG CỘNG TÀI SẢN (MÃ 270)
        var ma270_CuoiNam = ma100_CuoiNam + ma200_CuoiNam;
        var ma270_DauNam = ma100_DauNam + ma200_DauNam;

        // 7. Nợ phải trả (Mã 300)
        // 7.1. Phải trả người bán (Mã 311: Dư Có TK 331 theo từng đối tượng)
        var ma311_CuoiNam = await TinhSoDuCoTaiKhoanLuongTinhAsync("331", denNgay);
        var ma311_DauNam = await TinhSoDuCoTaiKhoanLuongTinhAsync("331", tuNgay.AddDays(-1));

        // 7.2. Người mua trả tiền trước (Mã 312: Dư Có TK 131 theo từng đối tượng)
        var ma312_CuoiNam = await TinhSoDuCoTaiKhoanLuongTinhAsync("131", denNgay);
        var ma312_DauNam = await TinhSoDuCoTaiKhoanLuongTinhAsync("131", tuNgay.AddDays(-1));

        // 7.3. Thuế và các khoản phải nộp Nhà nước (Mã 313: Dư Có TK 333 + Thuế TNDN hiện hành B02 chưa hạch toán)
        var thueSoCaiCuoiNam = await TinhSoDuCoTaiKhoanAsync("333", denNgay);
        var thueSoCaiDauNam = await TinhSoDuCoTaiKhoanAsync("333", tuNgay.AddDays(-1));

        var thueTndnChuaGhiSoCuoiNam = Math.Max(0m, b02.ThueTndnHienHanh - await TinhSoDuCoTaiKhoanAsync("3334", denNgay));
        var ma313_CuoiNam = thueSoCaiCuoiNam + thueTndnChuaGhiSoCuoiNam;
        var ma313_DauNam = thueSoCaiDauNam;

        // 7.4. Phải trả người lao động (Mã 314: Dư Có TK 334)
        var ma314_CuoiNam = await TinhSoDuCoTaiKhoanAsync("334", denNgay);
        var ma314_DauNam = await TinhSoDuCoTaiKhoanAsync("334", tuNgay.AddDays(-1));

        // 7.5. Phải trả khác (Bảo hiểm, KPCĐ) (Mã 319: Dư Có TK 338)
        var ma319_CuoiNam = await TinhSoDuCoTaiKhoanAsync("338", denNgay);
        var ma319_DauNam = await TinhSoDuCoTaiKhoanAsync("338", tuNgay.AddDays(-1));

        var ma300_CuoiNam = ma311_CuoiNam + ma312_CuoiNam + ma313_CuoiNam + ma314_CuoiNam + ma319_CuoiNam;
        var ma300_DauNam = ma311_DauNam + ma312_DauNam + ma313_DauNam + ma314_DauNam + ma319_DauNam;

        // 8. Vốn chủ sở hữu (Mã 400)
        // 8.1. Vốn đầu tư của chủ sở hữu (Mã 411: Dư Có TK 411)
        var ma411_CuoiNam = await TinhSoDuCoTaiKhoanAsync("411", denNgay);
        var ma411_DauNam = await TinhSoDuCoTaiKhoanAsync("411", tuNgay.AddDays(-1));

        // 8.2. Lợi nhuận sau thuế chưa phân phối (Mã 421)
        // 4211: Lũy kế các năm trước
        var ma4211_CuoiNam = await TinhSoDuCoTaiKhoanAsync("4211", denNgay) - await TinhSoDuNoTaiKhoanAsync("4211", denNgay);
        var ma4211_DauNam = await TinhSoDuCoTaiKhoanAsync("4211", tuNgay.AddDays(-1)) - await TinhSoDuNoTaiKhoanAsync("4211", tuNgay.AddDays(-1));

        // 4212: Năm nay = Số dư trên sổ cái + Lợi nhuận sau thuế B02 chưa kết chuyển
        var soDu4212SoCai = await TinhSoDuCoTaiKhoanAsync("4212", denNgay) - await TinhSoDuNoTaiKhoanAsync("4212", denNgay);
        var ma4212_CuoiNam = soDu4212SoCai != 0 ? soDu4212SoCai : b02.LoiNhuanSauThue;
        var ma4212_DauNam = await TinhSoDuCoTaiKhoanAsync("4212", tuNgay.AddDays(-1)) - await TinhSoDuNoTaiKhoanAsync("4212", tuNgay.AddDays(-1));

        var ma421_CuoiNam = ma4211_CuoiNam + ma4212_CuoiNam;
        var ma421_DauNam = ma4211_DauNam + ma4212_DauNam;

        var ma400_CuoiNam = ma411_CuoiNam + ma421_CuoiNam;
        var ma400_DauNam = ma411_DauNam + ma421_DauNam;

        // TỔNG CỘNG NGUỒN VỐN (MÃ 440)
        var ma440_CuoiNam = ma300_CuoiNam + ma400_CuoiNam;
        var ma440_DauNam = ma300_DauNam + ma400_DauNam;

        var chiTiets = new List<DongChiTieuB01ViewModel>
        {
            new() { MaSo = "100", ChiTieu = "A. TÀI SẢN NGẮN HẠN", SoDauNam = ma100_DauNam, SoCuoiNam = ma100_CuoiNam, InDam = true, CapDo = 1 },
            new() { MaSo = "110", ChiTieu = "I. Tiền và các khoản tương đương tiền", SoDauNam = ma110_DauNam, SoCuoiNam = ma110_CuoiNam, InDam = true, CapDo = 2 },
            new() { MaSo = "111", ChiTieu = "1. Tiền", SoDauNam = ma110_DauNam, SoCuoiNam = ma110_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "130", ChiTieu = "II. Các khoản phải thu ngắn hạn", SoDauNam = ma131_DauNam + ma132_DauNam, SoCuoiNam = ma131_CuoiNam + ma132_CuoiNam, InDam = true, CapDo = 2 },
            new() { MaSo = "131", ChiTieu = "1. Phải thu ngắn hạn của khách hàng", SoDauNam = ma131_DauNam, SoCuoiNam = ma131_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "132", ChiTieu = "2. Trả trước cho người bán ngắn hạn", SoDauNam = ma132_DauNam, SoCuoiNam = ma132_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "140", ChiTieu = "III. Hàng tồn kho", SoDauNam = ma140_DauNam, SoCuoiNam = ma140_CuoiNam, InDam = true, CapDo = 2 },
            new() { MaSo = "141", ChiTieu = "1. Hàng tồn kho", SoDauNam = ma140_DauNam, SoCuoiNam = ma140_CuoiNam, InDam = false, CapDo = 3 },

            new() { MaSo = "200", ChiTieu = "B. TÀI SẢN DÀI HẠN", SoDauNam = ma200_DauNam, SoCuoiNam = ma200_CuoiNam, InDam = true, CapDo = 1 },
            new() { MaSo = "220", ChiTieu = "I. Tài sản cố định", SoDauNam = ma220_DauNam, SoCuoiNam = ma220_CuoiNam, InDam = true, CapDo = 2 },
            new() { MaSo = "221", ChiTieu = "- Nguyên giá TSCĐ", SoDauNam = ma221_DauNam, SoCuoiNam = ma221_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "222", ChiTieu = "- Giá trị hao mòn lũy kế (*)", SoDauNam = -ma222_DauNam, SoCuoiNam = -ma222_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "260", ChiTieu = "II. Chi phí trả trước dài hạn (TK 242)", SoDauNam = ma260_DauNam, SoCuoiNam = ma260_CuoiNam, InDam = false, CapDo = 2 },

            new() { MaSo = "270", ChiTieu = "TỔNG CỘNG TÀI SẢN (270 = 100 + 200)", SoDauNam = ma270_DauNam, SoCuoiNam = ma270_CuoiNam, InDam = true, CapDo = 1 },

            new() { MaSo = "300", ChiTieu = "C. NỢ PHẢI TRẢ", SoDauNam = ma300_DauNam, SoCuoiNam = ma300_CuoiNam, InDam = true, CapDo = 1 },
            new() { MaSo = "310", ChiTieu = "I. Nợ ngắn hạn", SoDauNam = ma300_DauNam, SoCuoiNam = ma300_CuoiNam, InDam = true, CapDo = 2 },
            new() { MaSo = "311", ChiTieu = "1. Phải trả người bán ngắn hạn", SoDauNam = ma311_DauNam, SoCuoiNam = ma311_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "312", ChiTieu = "2. Người mua trả tiền trước ngắn hạn", SoDauNam = ma312_DauNam, SoCuoiNam = ma312_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "313", ChiTieu = "3. Thuế và các khoản phải nộp Nhà nước", SoDauNam = ma313_DauNam, SoCuoiNam = ma313_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "314", ChiTieu = "4. Phải trả người lao động", SoDauNam = ma314_DauNam, SoCuoiNam = ma314_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "319", ChiTieu = "5. Phải trả khác (Bảo hiểm, KPCĐ)", SoDauNam = ma319_DauNam, SoCuoiNam = ma319_CuoiNam, InDam = false, CapDo = 3 },

            new() { MaSo = "400", ChiTieu = "D. VỐN CHỦ SỞ HỮU", SoDauNam = ma400_DauNam, SoCuoiNam = ma400_CuoiNam, InDam = true, CapDo = 1 },
            new() { MaSo = "410", ChiTieu = "I. Vốn chủ sở hữu", SoDauNam = ma400_DauNam, SoCuoiNam = ma400_CuoiNam, InDam = true, CapDo = 2 },
            new() { MaSo = "411", ChiTieu = "1. Vốn góp của chủ sở hữu", SoDauNam = ma411_DauNam, SoCuoiNam = ma411_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "421", ChiTieu = "2. Lợi nhuận sau thuế chưa phân phối", SoDauNam = ma421_DauNam, SoCuoiNam = ma421_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "4211", ChiTieu = "  - LNST chưa phân phối lũy kế đến cuối năm trước", SoDauNam = ma4211_DauNam, SoCuoiNam = ma4211_CuoiNam, InDam = false, CapDo = 3 },
            new() { MaSo = "4212", ChiTieu = "  - LNST chưa phân phối năm nay", SoDauNam = ma4212_DauNam, SoCuoiNam = ma4212_CuoiNam, InDam = false, CapDo = 3 },

            new() { MaSo = "440", ChiTieu = "TỔNG CỘNG NGUỒN VỐN (440 = 300 + 400)", SoDauNam = ma440_DauNam, SoCuoiNam = ma440_CuoiNam, InDam = true, CapDo = 1 }
        };

        return new BaoCaoTinhHinhTaiChinhViewModel
        {
            NamTaiChinh = namTaiChinh,
            NgayLap = DateTime.Today,
            SoChungTu = $"B01-DN-{namTaiChinh}",
            TongTaiSanDauNam = ma270_DauNam,
            TongTaiSanCuoiNam = ma270_CuoiNam,
            TongNguonVonDauNam = ma440_DauNam,
            TongNguonVonCuoiNam = ma440_CuoiNam,
            ChiTiets = chiTiets
        };
    }

    public async Task<BaoCaoKetQuaKinhDoanhViewModel> LapBaoCaoB02Async(int namTaiChinh)
    {
        var tuNgay = new DateTime(namTaiChinh, 1, 1);
        var denNgay = new DateTime(namTaiChinh, 12, 31, 23, 59, 59);

        var tuNgayTruoc = new DateTime(namTaiChinh - 1, 1, 1);
        var denNgayTruoc = new DateTime(namTaiChinh - 1, 12, 31, 23, 59, 59);

        // Doanh thu bán hàng & CCDV (Mã 01: Phát sinh Có TK 511)
        var ma01_NamNay = await TinhPhatSinhCoAsync("511", tuNgay, denNgay);
        var ma01_NamTruoc = await TinhPhatSinhCoAsync("511", tuNgayTruoc, denNgayTruoc);

        // Giảm trừ doanh thu (Mã 02)
        var ma02_NamNay = await TinhPhatSinhNoAsync("521", tuNgay, denNgay);
        var ma02_NamTruoc = await TinhPhatSinhNoAsync("521", tuNgayTruoc, denNgayTruoc);

        // Doanh thu thuần (Mã 10 = Mã 01 - Mã 02)
        var ma10_NamNay = ma01_NamNay - ma02_NamNay;
        var ma10_NamTruoc = ma01_NamTruoc - ma02_NamTruoc;

        // Giá vốn hàng bán (Mã 11: Phát sinh Nợ TK 632)
        var ma11_NamNay = await TinhPhatSinhNoAsync("632", tuNgay, denNgay);
        var ma11_NamTruoc = await TinhPhatSinhNoAsync("632", tuNgayTruoc, denNgayTruoc);

        // Lợi nhuận gộp (Mã 20 = Mã 10 - Mã 11)
        var ma20_NamNay = ma10_NamNay - ma11_NamNay;
        var ma20_NamTruoc = ma10_NamTruoc - ma11_NamTruoc;

        // Doanh thu hoạt động tài chính (Mã 21: Phát sinh Có TK 515)
        var ma21_NamNay = await TinhPhatSinhCoAsync("515", tuNgay, denNgay);
        var ma21_NamTruoc = await TinhPhatSinhCoAsync("515", tuNgayTruoc, denNgayTruoc);

        // Chi phí tài chính (Mã 22: Phát sinh Nợ TK 635)
        var ma22_NamNay = await TinhPhatSinhNoAsync("635", tuNgay, denNgay);
        var ma22_NamTruoc = await TinhPhatSinhNoAsync("635", tuNgayTruoc, denNgayTruoc);

        // Chi phí bán hàng và quản lý DN (Mã 25: Phát sinh Nợ TK 641 + 642)
        var ma25_NamNay = await TinhPhatSinhNoAsync("641", tuNgay, denNgay) + await TinhPhatSinhNoAsync("642", tuNgay, denNgay);
        var ma25_NamTruoc = await TinhPhatSinhNoAsync("641", tuNgayTruoc, denNgayTruoc) + await TinhPhatSinhNoAsync("642", tuNgayTruoc, denNgayTruoc);

        // Lợi nhuận thuần từ HĐKD (Mã 30 = Mã 20 + Mã 21 - Mã 22 - Mã 25)
        var ma30_NamNay = ma20_NamNay + ma21_NamNay - ma22_NamNay - ma25_NamNay;
        var ma30_NamTruoc = ma20_NamTruoc + ma21_NamTruoc - ma22_NamTruoc - ma25_NamTruoc;

        // Thu nhập khác (Mã 31: Có 711) và Chi phí khác (Mã 32: Nợ 811)
        var ma31_NamNay = await TinhPhatSinhCoAsync("711", tuNgay, denNgay);
        var ma31_NamTruoc = await TinhPhatSinhCoAsync("711", tuNgayTruoc, denNgayTruoc);

        var ma32_NamNay = await TinhPhatSinhNoAsync("811", tuNgay, denNgay);
        var ma32_NamTruoc = await TinhPhatSinhNoAsync("811", tuNgayTruoc, denNgayTruoc);

        var ma40_NamNay = ma31_NamNay - ma32_NamNay;
        var ma40_NamTruoc = ma31_NamTruoc - ma32_NamTruoc;

        // Tổng lợi nhuận kế toán trước thuế (Mã 50 = Mã 30 + Mã 40)
        var ma50_NamNay = ma30_NamNay + ma40_NamNay;
        var ma50_NamTruoc = ma30_NamTruoc + ma40_NamTruoc;

        // Chi phí thuế TNDN hiện hành (Mã 51: 20% nếu > 0)
        var ma51_NamNay = ma50_NamNay > 0 ? Math.Round(ma50_NamNay * 0.20m, 0) : 0m;
        var ma51_NamTruoc = ma50_NamTruoc > 0 ? Math.Round(ma50_NamTruoc * 0.20m, 0) : 0m;

        // Lợi nhuận sau thuế TNDN (Mã 60 = Mã 50 - Mã 51)
        var ma60_NamNay = ma50_NamNay - ma51_NamNay;
        var ma60_NamTruoc = ma50_NamTruoc - ma51_NamTruoc;

        var chiTiets = new List<DongChiTieuB02ViewModel>
        {
            new() { MaSo = "01", ChiTieu = "1. Doanh thu bán hàng và cung cấp dịch vụ", NamTruoc = ma01_NamTruoc, NamNay = ma01_NamNay, InDam = false },
            new() { MaSo = "02", ChiTieu = "2. Các khoản giảm trừ doanh thu", NamTruoc = ma02_NamTruoc, NamNay = ma02_NamNay, InDam = false },
            new() { MaSo = "10", ChiTieu = "3. Doanh thu thuần về bán hàng và CCDV (10 = 01 - 02)", NamTruoc = ma10_NamTruoc, NamNay = ma10_NamNay, InDam = true },
            new() { MaSo = "11", ChiTieu = "4. Giá vốn hàng bán", NamTruoc = ma11_NamTruoc, NamNay = ma11_NamNay, InDam = false },
            new() { MaSo = "20", ChiTieu = "5. Lợi nhuận gộp về bán hàng và CCDV (20 = 10 - 11)", NamTruoc = ma20_NamTruoc, NamNay = ma20_NamNay, InDam = true },
            new() { MaSo = "21", ChiTieu = "6. Doanh thu hoạt động tài chính", NamTruoc = ma21_NamTruoc, NamNay = ma21_NamNay, InDam = false },
            new() { MaSo = "22", ChiTieu = "7. Chi phí tài chính", NamTruoc = ma22_NamTruoc, NamNay = ma22_NamNay, InDam = false },
            new() { MaSo = "25", ChiTieu = "8. Chi phí bán hàng và chi phí QLDN", NamTruoc = ma25_NamTruoc, NamNay = ma25_NamNay, InDam = false },
            new() { MaSo = "30", ChiTieu = "9. Lợi nhuận thuần từ hoạt động kinh doanh (30 = 20 + 21 - 22 - 25)", NamTruoc = ma30_NamTruoc, NamNay = ma30_NamNay, InDam = true },
            new() { MaSo = "31", ChiTieu = "10. Thu nhập khác", NamTruoc = ma31_NamTruoc, NamNay = ma31_NamNay, InDam = false },
            new() { MaSo = "32", ChiTieu = "11. Chi phí khác", NamTruoc = ma32_NamTruoc, NamNay = ma32_NamNay, InDam = false },
            new() { MaSo = "40", ChiTieu = "12. Lợi nhuận khác (40 = 31 - 32)", NamTruoc = ma40_NamTruoc, NamNay = ma40_NamNay, InDam = false },
            new() { MaSo = "50", ChiTieu = "13. Tổng lợi nhuận kế toán trước thuế (50 = 30 + 40)", NamTruoc = ma50_NamTruoc, NamNay = ma50_NamNay, InDam = true },
            new() { MaSo = "51", ChiTieu = "14. Chi phí thuế TNDN hiện hành", NamTruoc = ma51_NamTruoc, NamNay = ma51_NamNay, InDam = false },
            new() { MaSo = "60", ChiTieu = "15. Lợi nhuận sau thuế TNDN (60 = 50 - 51)", NamTruoc = ma60_NamTruoc, NamNay = ma60_NamNay, InDam = true }
        };

        return new BaoCaoKetQuaKinhDoanhViewModel
        {
            NamTaiChinh = namTaiChinh,
            NgayLap = DateTime.Today,
            SoChungTu = $"B02-DN-{namTaiChinh}",
            DoanhThuThuan = ma10_NamNay,
            LoiNhuanGop = ma20_NamNay,
            LoiNhuanTruocThue = ma50_NamNay,
            ThueTndnHienHanh = ma51_NamNay,
            LoiNhuanSauThue = ma60_NamNay,
            ChenhLechTk4212 = ma60_NamNay,
            ChiTiets = chiTiets
        };
    }

    public async Task<BaoCaoLuuChuyenTienTeViewModel> LapBaoCaoB03Async(int namTaiChinh)
    {
        var tuNgay = new DateTime(namTaiChinh, 1, 1);
        var denNgay = new DateTime(namTaiChinh, 12, 31, 23, 59, 59);

        // Phân tích dòng tiền trực tiếp qua đối ứng với TK 1111 và 1121
        // Mã 01: Tiền thu bán hàng (Nợ 1111, 1121 / Có 511, 131, 3331)
        var ma01 = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanNo != null && (c.TaiKhoanNo.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("1121")) &&
                        c.TaiKhoanCo != null && (c.TaiKhoanCo.MaTaiKhoan.StartsWith("511") || c.TaiKhoanCo.MaTaiKhoan.StartsWith("131")))
            .SumAsync(c => c.SoTien);

        // Mã 02: Tiền chi trả người cung cấp HH, DV (Nợ 331, 152, 156, 641, 642 / Có 1111, 1121)
        var ma02 = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && (c.TaiKhoanCo.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanCo.MaTaiKhoan.StartsWith("1121")) &&
                        c.TaiKhoanNo != null && (c.TaiKhoanNo.MaTaiKhoan.StartsWith("331") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("15") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("641") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("642")))
            .SumAsync(c => c.SoTien);

        // Mã 03: Tiền chi trả cho người lao động (Nợ 334 / Có 1111, 1121)
        var ma03 = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && (c.TaiKhoanCo.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanCo.MaTaiKhoan.StartsWith("1121")) &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("334"))
            .SumAsync(c => c.SoTien);

        // Mã 05: Tiền chi nộp thuế TNDN (Nợ 3334 / Có 1111, 1121)
        var ma05 = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && (c.TaiKhoanCo.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanCo.MaTaiKhoan.StartsWith("1121")) &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("3334"))
            .SumAsync(c => c.SoTien);

        // Mã 20: Lưu chuyển thuần HĐKD
        var ma20 = ma01 - ma02 - ma03 - ma05;

        // Mã 21: Tiền chi mua sắm TSCĐ (Nợ 211 / Có 1111, 1121)
        var ma21 = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && (c.TaiKhoanCo.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanCo.MaTaiKhoan.StartsWith("1121")) &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("211"))
            .SumAsync(c => c.SoTien);

        var ma30 = -ma21; // Lưu chuyển thuần HĐĐT

        // Mã 31: Tiền thu nhận vốn góp chủ sở hữu (Nợ 1111, 1121 / Có 411)
        var ma31 = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanNo != null && (c.TaiKhoanNo.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanNo.MaTaiKhoan.StartsWith("1121")) &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("411"))
            .SumAsync(c => c.SoTien);

        // Mã 34: Tiền chi trả nợ gốc vay (Nợ 341 / Có 1111, 1121)
        var ma34 = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && (c.TaiKhoanCo.MaTaiKhoan.StartsWith("1111") || c.TaiKhoanCo.MaTaiKhoan.StartsWith("1121")) &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("341"))
            .SumAsync(c => c.SoTien);

        var ma40 = ma31 - ma34; // Lưu chuyển thuần HĐTC

        // Mã 50 = 20 + 30 + 40
        var ma50 = ma20 + ma30 + ma40;

        // Tiền đầu kỳ (Mã 60: Số dư Nợ 1111 + 1121 đầu kỳ)
        var tienDauKy = await TinhSoDuNoTaiKhoanAsync("1111", tuNgay.AddDays(-1)) + await TinhSoDuNoTaiKhoanAsync("1121", tuNgay.AddDays(-1));

        // Tiền cuối kỳ (Mã 70 = 50 + 60)
        var tienCuoiKy = tienDauKy + ma50;

        // Kiểm tra khớp với số dư sổ cái thực tế
        var duNoSoCaiCuoiKy = await TinhSoDuNoTaiKhoanAsync("1111", denNgay) + await TinhSoDuNoTaiKhoanAsync("1121", denNgay);

        var chiTiets = new List<DongChiTieuB03ViewModel>
        {
            new() { MaSo = "01", ChiTieu = "1. Tiền thu từ bán hàng, CCDV", NamNay = ma01, InDam = false },
            new() { MaSo = "02", ChiTieu = "2. Tiền chi trả cho người cung cấp HH, DV", NamNay = -ma02, InDam = false },
            new() { MaSo = "03", ChiTieu = "3. Tiền chi trả cho người lao động", NamNay = -ma03, InDam = false },
            new() { MaSo = "05", ChiTieu = "4. Tiền chi nộp thuế thu nhập doanh nghiệp", NamNay = -ma05, InDam = false },
            new() { MaSo = "20", ChiTieu = "Lưu chuyển tiền thuần từ hoạt động kinh doanh", NamNay = ma20, InDam = true },

            new() { MaSo = "21", ChiTieu = "1. Tiền chi mua sắm TSCĐ và các tài sản dài hạn", NamNay = -ma21, InDam = false },
            new() { MaSo = "30", ChiTieu = "Lưu chuyển tiền thuần từ hoạt động đầu tư", NamNay = ma30, InDam = true },

            new() { MaSo = "31", ChiTieu = "1. Tiền thu từ phát hành cổ phiếu, nhận góp vốn", NamNay = ma31, InDam = false },
            new() { MaSo = "34", ChiTieu = "2. Tiền chi trả nợ gốc vay", NamNay = -ma34, InDam = false },
            new() { MaSo = "40", ChiTieu = "Lưu chuyển tiền thuần từ hoạt động tài chính", NamNay = ma40, InDam = true },

            new() { MaSo = "50", ChiTieu = "LƯU CHUYỂN TIỀN THUẦN TRONG KỲ (50 = 20 + 30 + 40)", NamNay = ma50, InDam = true },
            new() { MaSo = "60", ChiTieu = "Tiền và tương đương tiền đầu kỳ", NamNay = tienDauKy, InDam = true },
            new() { MaSo = "70", ChiTieu = "TIỀN VÀ TƯƠNG ĐƯƠNG TIỀN CUỐI KỲ (70 = 50 + 60)", NamNay = tienCuoiKy, InDam = true }
        };

        return new BaoCaoLuuChuyenTienTeViewModel
        {
            NamTaiChinh = namTaiChinh,
            NgayLap = DateTime.Today,
            SoChungTu = $"B03-DN-{namTaiChinh}",
            LuuChuyenHdkd = ma20,
            LuuChuyenHddt = ma30,
            LuuChuyenHdtc = ma40,
            LuuChuyenThuanTrongKy = ma50,
            TienDauKy = tienDauKy,
            TienCuoiKy = tienCuoiKy,
            DuNoTk111Va112 = duNoSoCaiCuoiKy,
            ChiTiets = chiTiets
        };
    }

    public async Task<ThuyetMinhBctcViewModel> LapThuyetMinhB09Async(int namTaiChinh)
    {
        var denNgay = new DateTime(namTaiChinh, 12, 31, 23, 59, 59);

        var nguyenGia = await TinhSoDuNoTaiKhoanAsync("211", denNgay);
        var haoMon = await TinhSoDuCoTaiKhoanAsync("2141", denNgay);
        var ccdc = await TinhSoDuNoTaiKhoanAsync("242", denNgay);
        var phaiThu = await TinhSoDuNoTaiKhoanLuongTinhAsync("131", denNgay);
        var phaiTra = await TinhSoDuCoTaiKhoanLuongTinhAsync("331", denNgay);
        var quyLuong = await TinhPhatSinhCoAsync("334", new DateTime(namTaiChinh, 1, 1), denNgay);
        var baoHiem = await TinhPhatSinhCoAsync("3383", new DateTime(namTaiChinh, 1, 1), denNgay) +
                      await TinhPhatSinhCoAsync("3384", new DateTime(namTaiChinh, 1, 1), denNgay) +
                      await TinhPhatSinhCoAsync("3386", new DateTime(namTaiChinh, 1, 1), denNgay);
        var thue = await TinhPhatSinhNoAsync("333", new DateTime(namTaiChinh, 1, 1), denNgay);

        var company = await _context.ThongTinDoanhNghieps.FirstOrDefaultAsync();

        return new ThuyetMinhBctcViewModel
        {
            NamTaiChinh = namTaiChinh,
            TenDoanhNghiep = company?.TenDoanhNghiep ?? "CÔNG TY CỔ PHẦN CÔNG NGHỆ NINJATAX VIỆT NAM",
            MaSoThue = company?.MaSoThue ?? "0109998883",
            DiaChi = company?.DiaChiTruSo ?? "Hà Nội, Việt Nam",
            CheDoKeToan = "Thông tư 99/2025/TT-BTC",
            DonViTienTe = "VND",
            NguyenGiaTscd = nguyenGia,
            HaoMonLuyKeTscd = haoMon,
            GiaTriConLaiTscd = nguyenGia - haoMon,
            TongChiPhiCcDcPhanBo = ccdc,
            TongPhaiThuKhachHang = phaiThu,
            TongPhaiTraNguoiBan = phaiTra,
            TongQuyLuongTrongNam = quyLuong,
            TongBaoHiemDaTrichNop = baoHiem,
            TongThueDaNopTrongNam = thue
        };
    }

    public async Task KhoaSoBctcNamAsync(int namTaiChinh)
    {
        var existing = await _context.BaoCaoTaiChinhNams
            .FirstOrDefaultAsync(b => b.NamTaiChinh == namTaiChinh);

        if (existing == null)
        {
            var b01 = await LapBaoCaoB01Async(namTaiChinh);
            var b02 = await LapBaoCaoB02Async(namTaiChinh);
            var b03 = await LapBaoCaoB03Async(namTaiChinh);

            existing = new BaoCaoTaiChinhNam
            {
                NamTaiChinh = namTaiChinh,
                NgayLap = DateTime.Today,
                SoChungTu = $"BCTC-{namTaiChinh}",
                TrangThai = TrangThaiBaoCaoTaiChinh.DaKhoaSo,
                TongTaiSan = b01.TongTaiSanCuoiNam,
                TongNguonVon = b01.TongNguonVonCuoiNam,
                DoanhThuThuan = b02.DoanhThuThuan,
                LoiNhuanSauThue = b02.LoiNhuanSauThue,
                LuuChuyenTienThuan = b03.LuuChuyenThuanTrongKy,
                TienCuoiKy = b03.TienCuoiKy
            };
            _context.BaoCaoTaiChinhNams.Add(existing);
        }
        else
        {
            existing.TrangThai = TrangThaiBaoCaoTaiChinh.DaKhoaSo;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<decimal> TinhSoDuNoTaiKhoanAsync(string maTkPrefix, DateTime mocThoiGian)
    {
        var tongPhatSinhNo = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= mocThoiGian &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTkPrefix))
            .SumAsync(c => c.SoTien);

        var tongPhatSinhCo = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= mocThoiGian &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTkPrefix))
            .SumAsync(c => c.SoTien);

        return Math.Max(0m, tongPhatSinhNo - tongPhatSinhCo);
    }

    private async Task<decimal> TinhSoDuCoTaiKhoanAsync(string maTkPrefix, DateTime mocThoiGian)
    {
        var tongPhatSinhCo = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= mocThoiGian &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTkPrefix))
            .SumAsync(c => c.SoTien);

        var tongPhatSinhNo = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= mocThoiGian &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTkPrefix))
            .SumAsync(c => c.SoTien);

        return Math.Max(0m, tongPhatSinhCo - tongPhatSinhNo);
    }

    private async Task<decimal> TinhPhatSinhNoAsync(string maTkPrefix, DateTime tuNgay, DateTime denNgay)
    {
        return await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTkPrefix))
            .SumAsync(c => c.SoTien);
    }

    private async Task<decimal> TinhPhatSinhCoAsync(string maTkPrefix, DateTime tuNgay, DateTime denNgay)
    {
        return await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan >= tuNgay && c.ButToan.NgayHachToan <= denNgay &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTkPrefix))
            .SumAsync(c => c.SoTien);
    }

    private async Task<decimal> TinhSoDuNoTaiKhoanLuongTinhAsync(string maTkPrefix, DateTime mocThoiGian)
    {
        var records = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= mocThoiGian &&
                        ((c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTkPrefix)) ||
                         (c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTkPrefix))))
            .Select(c => new
            {
                c.DoiTuongId,
                No = (c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTkPrefix)) ? c.SoTien : 0m,
                Co = (c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTkPrefix)) ? c.SoTien : 0m
            })
            .ToListAsync();

        return records
            .GroupBy(r => r.DoiTuongId)
            .Select(g => g.Sum(x => x.No) - g.Sum(x => x.Co))
            .Where(bal => bal > 0)
            .Sum();
    }

    private async Task<decimal> TinhSoDuCoTaiKhoanLuongTinhAsync(string maTkPrefix, DateTime mocThoiGian)
    {
        var records = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= mocThoiGian &&
                        ((c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTkPrefix)) ||
                         (c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTkPrefix))))
            .Select(c => new
            {
                c.DoiTuongId,
                No = (c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith(maTkPrefix)) ? c.SoTien : 0m,
                Co = (c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith(maTkPrefix)) ? c.SoTien : 0m
            })
            .ToListAsync();

        return records
            .GroupBy(r => r.DoiTuongId)
            .Select(g => g.Sum(x => x.Co) - g.Sum(x => x.No))
            .Where(bal => bal > 0)
            .Sum();
    }
}
