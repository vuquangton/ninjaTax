using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ đối soát tính toàn vẹn số liệu giữa Sổ phụ (Subledger AR/AP/Inventory/FixedAssets) và Sổ Cái (General Ledger).
/// </summary>
public class SubledgerReconciliationService : ISubledgerReconciliationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubledgerReconciliationService> _logger;

    public SubledgerReconciliationService(
        AppDbContext context,
        ILogger<SubledgerReconciliationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SubledgerReconciliationReportViewModel> KiemTraDoiSoatToanHeThongAsync(DateTime mocThoiGian)
    {
        var cutoff = mocThoiGian.Date.AddDays(1).AddTicks(-1);
        var report = new SubledgerReconciliationReportViewModel
        {
            MocThoiGian = mocThoiGian
        };

        // 1. ĐỐI SOÁT CÔNG NỢ PHẢI THU KHÁCH HÀNG (AR Subledger vs TK 131)
        // Subledger AR: Tổng phải thu trên hóa đơn bán hàng - Đã thu tiền (chỉ tính HĐ hợp lệ)
        var arSubledger = await _context.HoaDonBanHangs
            .Where(h => h.NgayHachToan <= cutoff && h.TrangThai != TrangThaiHddt.DaHuy)
            .SumAsync(h => h.TongThanhToan - h.DaThuTien);

        // GL TK 131: Dư Nợ - Dư Có của TK 131
        var gl131No = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("131"))
            .SumAsync(c => c.SoTien);

        var gl131Co = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("131"))
            .SumAsync(c => c.SoTien);

        var gl131Balance = gl131No - gl131Co;

        report.DanhSachDoiSoat.Add(new DongDoiSoatSubledgerViewModel
        {
            PhanHe = "Công Nợ Phải Thu (AR)",
            MaTaiKhoan = "131",
            TenTaiKhoan = "Phải thu của khách hàng",
            SoDuSoPhuSubledger = arSubledger,
            SoDuSoCaiGl = gl131Balance,
            GhiChu = "So sánh tổng nợ chưa thanh toán trên hóa đơn bán hàng với số dư sổ cái TK 131."
        });

        // 2. ĐỐI SOÁT CÔNG NỢ PHẢI TRẢ NHÀ CUNG CẤP (AP Subledger vs TK 331)
        // Subledger AP: Tổng phải trả trên hóa đơn mua hàng - Đã thanh toán
        var apSubledger = await _context.HoaDonMuaHangs
            .Where(h => h.NgayHachToan <= cutoff && h.TrangThai == TrangThaiHoaDonMua.DaGhiSo)
            .SumAsync(h => h.TongThanhToan - h.DaThanhToan);

        // GL TK 331: Dư Có - Dư Nợ của TK 331
        var gl331Co = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("331"))
            .SumAsync(c => c.SoTien);

        var gl331No = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("331"))
            .SumAsync(c => c.SoTien);

        var gl331Balance = gl331Co - gl331No;

        report.DanhSachDoiSoat.Add(new DongDoiSoatSubledgerViewModel
        {
            PhanHe = "Công Nợ Phải Trả (AP)",
            MaTaiKhoan = "331",
            TenTaiKhoan = "Phải trả cho người bán",
            SoDuSoPhuSubledger = apSubledger,
            SoDuSoCaiGl = gl331Balance,
            GhiChu = "So sánh tổng nợ chưa thanh toán trên hóa đơn mua hàng với số dư sổ cái TK 331."
        });

        // 3. ĐỐI SOÁT KHO & TỒN KHO HÀNG HÓA (Inventory Subledger vs TK 1561)
        // Subledger Kho: Tổng tiền nhập - Tổng tiền xuất từ phiếu nhập kho / xuất kho đã ghi sổ
        var tongNhapKho = await _context.PhieuNhapKhos
            .Where(p => p.NgayHachToan <= cutoff && p.TrangThai == TrangThaiPhieuKho.DaGhiSo)
            .SumAsync(p => p.TongTienHang);

        var tongXuatKho = await _context.PhieuXuatKhos
            .Where(p => p.NgayHachToan <= cutoff && p.TrangThai == TrangThaiPhieuKho.DaGhiSo)
            .SumAsync(p => p.TongTienGiaVon);

        var khoSubledger = tongNhapKho - tongXuatKho;

        // GL TK 1561: Dư Nợ - Dư Có của TK 1561
        var gl1561No = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("1561"))
            .SumAsync(c => c.SoTien);

        var gl1561Co = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("1561"))
            .SumAsync(c => c.SoTien);

        var gl1561Balance = gl1561No - gl1561Co;

        report.DanhSachDoiSoat.Add(new DongDoiSoatSubledgerViewModel
        {
            PhanHe = "Kho Hàng Hóa (S10-DN)",
            MaTaiKhoan = "1561",
            TenTaiKhoan = "Hàng hóa",
            SoDuSoPhuSubledger = khoSubledger,
            SoDuSoCaiGl = gl1561Balance,
            GhiChu = "So sánh giá trị tồn kho tổng hợp (Nhập - Xuất) với số dư tài khoản 1561."
        });

        // 4. ĐỐI SOÁT NGUYÊN GIÁ TÀI SẢN CỐ ĐỊNH (Fixed Asset Subledger vs TK 211)
        var tscdSubledger = await _context.TaiSanCoDinhs
            .Where(t => t.NgayGhiTang <= cutoff && t.TrangThai == TrangThaiTaiSan.DangSuDung)
            .SumAsync(t => t.NguyenGia);

        var gl211No = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanNo != null && c.TaiKhoanNo.MaTaiKhoan.StartsWith("211"))
            .SumAsync(c => c.SoTien);

        var gl211Co = await _context.ChiTietButToans
            .Where(c => c.ButToan != null && c.ButToan.TrangThai == TrangThaiButToan.DaGhiSo &&
                        c.ButToan.NgayHachToan <= cutoff &&
                        c.TaiKhoanCo != null && c.TaiKhoanCo.MaTaiKhoan.StartsWith("211"))
            .SumAsync(c => c.SoTien);

        var gl211Balance = gl211No - gl211Co;

        report.DanhSachDoiSoat.Add(new DongDoiSoatSubledgerViewModel
        {
            PhanHe = "Tài Sản Cố Định (TSCĐ)",
            MaTaiKhoan = "211",
            TenTaiKhoan = "Tài sản cố định hữu hình",
            SoDuSoPhuSubledger = tscdSubledger,
            SoDuSoCaiGl = gl211Balance,
            GhiChu = "So sánh tổng nguyên giá danh mục TSCĐ đang quản lý với số dư sổ cái TK 211."
        });

        _logger.LogInformation("Hoàn tất đối soát sổ phụ vs Sổ cái tại {Moc}: Khớp hoàn toàn={Khop}, Số mục lệch={Lech}",
            mocThoiGian, report.ToanBoKhopSoLieu, report.SoMucLech);

        return report;
    }
}
