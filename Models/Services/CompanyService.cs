using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ quản lý Hồ sơ Doanh nghiệp, Đa chi nhánh và Khóa sổ Kế toán (Phase 6).
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(AppDbContext context, ILogger<CompanyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ThongTinDoanhNghiep> GetCompanyProfileAsync()
    {
        var company = await _context.ThongTinDoanhNghieps
            .Include(c => c.ChiNhanhs)
            .Include(c => c.CauHinhKeToan)
            .FirstOrDefaultAsync();

        if (company == null)
        {
            company = new ThongTinDoanhNghiep
            {
                MaDoanhNghiep = "DN01",
                TenDoanhNghiep = "CÔNG TY CỔ PHẦN CÔNG NGHỆ NINJATAX VIỆT NAM",
                TenGiaoDich = "NINJATAX JSC",
                MaSoThue = "0109998883",
                DiaChiTruSo = "Tầng 10, Tòa nhà Keangnam Landmark 72, Đường Phạm Hùng, Q. Nam Từ Liêm, TP. Hà Nội",
                TinhThanhPho = "TP. Hà Nội",
                QuanHuyen = "Quận Nam Từ Liêm",
                MaCoQuanThueQuanLy = "101",
                TenCoQuanThueQuanLy = "Cục Thuế Thành phố Hà Nội",
                NguoiDaiDienPhapLuat = "Nguyễn Văn Doanh",
                ChucDanhNguoiDaiDien = "Tổng Giám Đốc",
                GiamDoc = "Nguyễn Văn Doanh",
                KeToanTruong = "Trần Thị Kế Toán",
                NguoiLapBieu = "Lê Văn Lập Biểu",
                ThuQuy = "Phạm Thị Thủ Quỹ",
                SoDienThoai = "024.3999.8888",
                Email = "ketoan@ninjatax.vn",
                VonDieuLe = 20_000_000_000m,
                NgayThanhLap = new DateTime(2020, 1, 1)
            };

            var hoBranch = new ChiNhanh
            {
                MaChiNhanh = "HO-01",
                TenChiNhanh = "Trụ sở chính Hà Nội",
                MaSoThueChiNhanh = company.MaSoThue,
                LoaiChiNhanh = LoaiChiNhanh.TruSoChinh,
                DiaChi = company.DiaChiTruSo,
                TinhThanhPho = company.TinhThanhPho,
                DangHoatDong = true
            };
            company.ChiNhanhs.Add(hoBranch);

            var accountingConfig = new CauHinhKeToan
            {
                CheDoKeToan = CheDoKeToanDoanhNghiep.TT99_2025,
                DonViTienTe = "VND",
                NgayBatDauNienDo = 1,
                ThangBatDauNienDo = 1,
                PhuongPhapThueGtgt = PhuongPhapTinhThueGtgt.KhauTru,
                PhuongPhapXuatKho = PhuongPhapGiaXuatKho.BinhQuanCuoiKy,
                PhuongPhapKhauHaoTscd = PhuongPhapKhauHao.DuongThang,
                CanhBaoChiVuotQuy = true,
                CanhBaoXuatAmKho = true,
                CanhBaoHoaDonTren20TrTienMat = true
            };
            company.CauHinhKeToan = accountingConfig;

            _context.ThongTinDoanhNghieps.Add(company);
            await _context.SaveChangesAsync();
        }

        return company;
    }

    public async Task UpdateCompanyProfileAsync(ThongTinDoanhNghiep profile)
    {
        if (!ThongTinDoanhNghiep.KiemTraMstHopLe(profile.MaSoThue))
        {
            throw new ArgumentException($"Mã số thuế '{profile.MaSoThue}' không hợp lệ theo thuật toán Modulo 11 của Tổng cục Thuế.");
        }

        var existing = await _context.ThongTinDoanhNghieps.FirstOrDefaultAsync(c => c.Id == profile.Id)
                       ?? await _context.ThongTinDoanhNghieps.FirstOrDefaultAsync();
        if (existing == null)
        {
            throw new KeyNotFoundException("Không tìm thấy hồ sơ doanh nghiệp.");
        }

        existing.TenDoanhNghiep = profile.TenDoanhNghiep;
        existing.TenGiaoDich = profile.TenGiaoDich;
        existing.TenTiengAnh = profile.TenTiengAnh;
        existing.MaSoThue = profile.MaSoThue;
        existing.DiaChiTruSo = profile.DiaChiTruSo;
        existing.TinhThanhPho = profile.TinhThanhPho;
        existing.QuanHuyen = profile.QuanHuyen;
        existing.MaCoQuanThueQuanLy = profile.MaCoQuanThueQuanLy;
        existing.TenCoQuanThueQuanLy = profile.TenCoQuanThueQuanLy;
        existing.NguoiDaiDienPhapLuat = profile.NguoiDaiDienPhapLuat;
        existing.ChucDanhNguoiDaiDien = profile.ChucDanhNguoiDaiDien;
        existing.GiamDoc = profile.GiamDoc;
        existing.KeToanTruong = profile.KeToanTruong;
        existing.NguoiLapBieu = profile.NguoiLapBieu;
        existing.ThuQuy = profile.ThuQuy;
        existing.SoDienThoai = profile.SoDienThoai;
        existing.Email = profile.Email;
        existing.Website = profile.Website;
        existing.VonDieuLe = profile.VonDieuLe;
        existing.LogoUrl = profile.LogoUrl;
        existing.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã cập nhật hồ sơ doanh nghiệp MST {Mst}", existing.MaSoThue);
    }

    public async Task<List<ChiNhanh>> GetBranchesAsync()
    {
        await GetCompanyProfileAsync(); // Đảm bảo đã khởi tạo Doanh nghiệp & Chi nhánh Trụ sở chính mặc định
        return await _context.ChiNhanhs
            .OrderBy(b => b.LoaiChiNhanh)
            .ThenBy(b => b.MaChiNhanh)
            .ToListAsync();
    }

    public async Task<ChiNhanh?> GetBranchByIdAsync(long id)
    {
        return await _context.ChiNhanhs
            .Include(b => b.CauHinhHddt)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<ChiNhanh> SaveBranchAsync(ChiNhanh branch)
    {
        var company = await GetCompanyProfileAsync();

        if (branch.LoaiChiNhanh != LoaiChiNhanh.TruSoChinh && !string.IsNullOrWhiteSpace(branch.MaSoThueChiNhanh))
        {
            if (!ChiNhanh.KiemTraMstChiNhanhHopLe(branch.MaSoThueChiNhanh, company.MaSoThue))
            {
                throw new ArgumentException($"Mã số thuế chi nhánh '{branch.MaSoThueChiNhanh}' không hợp lệ (Phải là 13 số XXXXXXXXXX-YYY khớp với MST công ty mẹ {company.MaSoThue}).");
            }
        }

        if (branch.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh)
        {
            var hasHo = await _context.ChiNhanhs.AnyAsync(b => b.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh && b.Id != branch.Id);
            if (hasHo)
            {
                throw new InvalidOperationException("Doanh nghiệp đã có Trụ sở chính. Không thể tạo thêm Trụ sở chính thứ hai.");
            }
        }

        if (branch.Id == 0)
        {
            branch.DoanhNghiepId = company.Id;
            branch.NgayTao = DateTime.UtcNow;
            _context.ChiNhanhs.Add(branch);
        }
        else
        {
            var existing = await _context.ChiNhanhs.FindAsync(branch.Id);
            if (existing == null) throw new KeyNotFoundException("Không tìm thấy chi nhánh.");

            // Không cho phép đổi loại của Trụ sở chính
            if (existing.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh && branch.LoaiChiNhanh != LoaiChiNhanh.TruSoChinh)
            {
                throw new InvalidOperationException("Không thể thay đổi loại đơn vị của Trụ sở chính.");
            }

            existing.MaChiNhanh = branch.MaChiNhanh;
            existing.TenChiNhanh = branch.TenChiNhanh;
            existing.MaSoThueChiNhanh = branch.MaSoThueChiNhanh;
            existing.LoaiChiNhanh = branch.LoaiChiNhanh;
            existing.KeKhaiThueGtgtRieng = branch.KeKhaiThueGtgtRieng;
            existing.KeKhaiThueTncnRieng = branch.KeKhaiThueTncnRieng;
            existing.DiaChi = branch.DiaChi;
            existing.TinhThanhPho = branch.TinhThanhPho;
            existing.MaCoQuanThueQuanLyRieng = branch.MaCoQuanThueQuanLyRieng;
            existing.TenCoQuanThueQuanLyRieng = branch.TenCoQuanThueQuanLyRieng;
            existing.NguoiDungDau = branch.NguoiDungDau;
            existing.SoDienThoai = branch.SoDienThoai;
            existing.DangHoatDong = branch.DangHoatDong;
            existing.NgayCapNhat = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return branch;
    }

    public async Task<bool> DeleteBranchAsync(long id)
    {
        var branch = await _context.ChiNhanhs.FindAsync(id);
        if (branch == null) return false;

        if (branch.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh)
        {
            throw new InvalidOperationException("Không được phép xóa đơn vị Trụ sở chính (Head Office).");
        }

        _context.ChiNhanhs.Remove(branch);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CauHinhKeToan> GetAccountingConfigAsync()
    {
        var company = await GetCompanyProfileAsync();
        var config = await _context.CauHinhKeToans.FirstOrDefaultAsync(c => c.DoanhNghiepId == company.Id);

        if (config == null)
        {
            config = new CauHinhKeToan
            {
                DoanhNghiepId = company.Id,
                CheDoKeToan = CheDoKeToanDoanhNghiep.TT99_2025,
                DonViTienTe = "VND",
                NgayBatDauNienDo = 1,
                ThangBatDauNienDo = 1,
                PhuongPhapThueGtgt = PhuongPhapTinhThueGtgt.KhauTru,
                PhuongPhapXuatKho = PhuongPhapGiaXuatKho.BinhQuanCuoiKy,
                PhuongPhapKhauHaoTscd = PhuongPhapKhauHao.DuongThang
            };
            _context.CauHinhKeToans.Add(config);
            await _context.SaveChangesAsync();
        }

        return config;
    }

    public async Task UpdateAccountingConfigAsync(CauHinhKeToan config)
    {
        var existing = await _context.CauHinhKeToans.FirstOrDefaultAsync(c => c.Id == config.Id || (config.DoanhNghiepId > 0 && c.DoanhNghiepId == config.DoanhNghiepId))
                       ?? await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (existing == null) throw new KeyNotFoundException("Không tìm thấy cấu hình kế toán.");

        existing.CheDoKeToan = config.CheDoKeToan;
        existing.DonViTienTe = config.DonViTienTe;
        existing.NgayBatDauNienDo = config.NgayBatDauNienDo;
        existing.ThangBatDauNienDo = config.ThangBatDauNienDo;
        existing.PhuongPhapThueGtgt = config.PhuongPhapThueGtgt;
        existing.PhuongPhapXuatKho = config.PhuongPhapXuatKho;
        existing.PhuongPhapKhauHaoTscd = config.PhuongPhapKhauHaoTscd;
        existing.CanhBaoChiVuotQuy = config.CanhBaoChiVuotQuy;
        existing.CanhBaoXuatAmKho = config.CanhBaoXuatAmKho;
        existing.CanhBaoHoaDonTren20TrTienMat = config.CanhBaoHoaDonTren20TrTienMat;
        existing.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã cập nhật cấu hình kế toán TT99.");
    }

    public async Task LockBookToDateAsync(DateTime lockDate)
    {
        var config = await GetAccountingConfigAsync();
        config.NgayKhoaSo = lockDate.Date;
        config.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogWarning("Đã khóa sổ kế toán toàn hệ thống đến hết ngày {NgayKhoaSo:dd/MM/yyyy}", lockDate);
    }

    public async Task UnlockBookAsync()
    {
        var config = await GetAccountingConfigAsync();
        config.NgayKhoaSo = null;
        config.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã mở khóa sổ kế toán toàn hệ thống.");
    }

    public async Task<bool> ValidateCanPostTransactionAsync(DateTime transactionDate)
    {
        var config = await GetAccountingConfigAsync();
        if (!config.ChoPhepGhiSo(transactionDate))
        {
            throw new InvalidOperationException($"Kỳ kế toán đã khóa sổ đến hết ngày {config.NgayKhoaSo:dd/MM/yyyy}. Không thể thêm, sửa hoặc xóa chứng từ hạch toán ngày {transactionDate:dd/MM/yyyy}.");
        }
        return true;
    }
}
