using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class TaiKhoanNganHangService : ITaiKhoanNganHangService
{
    private readonly AppDbContext _context;

    public TaiKhoanNganHangService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaiKhoanNganHangItemViewModel>> LayDanhSachAsync(string? timKiem = null)
    {
        var query = _context.TaiKhoanNganHangs
            .Include(t => t.TaiKhoanKeToan)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(timKiem))
        {
            var kw = timKiem.Trim().ToLower();
            query = query.Where(t => t.SoTaiKhoan.ToLower().Contains(kw) ||
                                     t.TenNganHang.ToLower().Contains(kw) ||
                                     (t.ChiNhanh != null && t.ChiNhanh.ToLower().Contains(kw)) ||
                                     (t.ChuTaiKhoan != null && t.ChuTaiKhoan.ToLower().Contains(kw)));
        }

        var entities = await query.OrderBy(t => t.TenNganHang).ThenBy(t => t.SoTaiKhoan).ToListAsync();

        return entities.Select(t => new TaiKhoanNganHangItemViewModel
        {
            Id = t.Id,
            SoTaiKhoan = t.SoTaiKhoan,
            TenNganHang = t.TenNganHang,
            ChiNhanh = t.ChiNhanh,
            ChuTaiKhoan = t.ChuTaiKhoan,
            SoDuBanDau = t.SoDuBanDau,
            TaiKhoanKeToanMa = t.TaiKhoanKeToan?.MaTaiKhoan,
            DangHoatDong = t.DangHoatDong,
            DaPhatSinhGiaoDich = false
        }).ToList();
    }

    public async Task<TaiKhoanNganHang?> LayTheoIdAsync(long id)
    {
        return await _context.TaiKhoanNganHangs
            .Include(t => t.TaiKhoanKeToan)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(bool ThanhCong, string? ThongBao, long? BankAccountId)> TaoMoiAsync(TaiKhoanNganHangCreateEditViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SoTaiKhoan))
        {
            return (false, "Số tài khoản ngân hàng không được để trống.", null);
        }

        if (string.IsNullOrWhiteSpace(model.TenNganHang))
        {
            return (false, "Tên ngân hàng không được để trống.", null);
        }

        var cleanStk = model.SoTaiKhoan.Trim();
        var cleanBank = model.TenNganHang.Trim();

        var tonTai = await _context.TaiKhoanNganHangs.AnyAsync(t => t.SoTaiKhoan == cleanStk && t.TenNganHang == cleanBank);
        if (tonTai)
        {
            return (false, $"Số tài khoản '{cleanStk}' tại ngân hàng '{cleanBank}' đã tồn tại trong hệ thống.", null);
        }

        var entity = new TaiKhoanNganHang
        {
            SoTaiKhoan = cleanStk,
            TenNganHang = cleanBank,
            ChiNhanh = string.IsNullOrWhiteSpace(model.ChiNhanh) ? null : model.ChiNhanh.Trim(),
            ChuTaiKhoan = string.IsNullOrWhiteSpace(model.ChuTaiKhoan) ? null : model.ChuTaiKhoan.Trim(),
            SoDuBanDau = model.SoDuBanDau,
            TaiKhoanKeToanId = model.TaiKhoanKeToanId,
            DangHoatDong = model.DangHoatDong,
            GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu.Trim(),
            NgayTao = DateTime.UtcNow
        };

        _context.TaiKhoanNganHangs.Add(entity);
        await _context.SaveChangesAsync();

        return (true, null, entity.Id);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, TaiKhoanNganHangCreateEditViewModel model)
    {
        var entity = await _context.TaiKhoanNganHangs.FindAsync(id);
        if (entity == null)
        {
            return (false, "Không tìm thấy tài khoản ngân hàng cần cập nhật.");
        }

        var cleanStk = model.SoTaiKhoan.Trim();
        var cleanBank = model.TenNganHang.Trim();

        var tonTai = await _context.TaiKhoanNganHangs.AnyAsync(t => t.SoTaiKhoan == cleanStk && t.TenNganHang == cleanBank && t.Id != id);
        if (tonTai)
        {
            return (false, $"Số tài khoản '{cleanStk}' tại ngân hàng '{cleanBank}' đã được sử dụng bởi tài khoản khác.");
        }

        entity.SoTaiKhoan = cleanStk;
        entity.TenNganHang = cleanBank;
        entity.ChiNhanh = string.IsNullOrWhiteSpace(model.ChiNhanh) ? null : model.ChiNhanh.Trim();
        entity.ChuTaiKhoan = string.IsNullOrWhiteSpace(model.ChuTaiKhoan) ? null : model.ChuTaiKhoan.Trim();
        entity.SoDuBanDau = model.SoDuBanDau;
        entity.TaiKhoanKeToanId = model.TaiKhoanKeToanId;
        entity.DangHoatDong = model.DangHoatDong;
        entity.GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu.Trim();

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id)
    {
        var entity = await _context.TaiKhoanNganHangs.FindAsync(id);
        if (entity == null)
        {
            return (false, "Không tìm thấy tài khoản ngân hàng cần xóa.");
        }

        var daPhatSinh = await KiemTraDaPhatSinhGiaoDichAsync(id);
        if (daPhatSinh)
        {
            return (false, $"Tài khoản ngân hàng '{entity.SoTaiKhoan}' đã phát sinh giao dịch. Không thể xóa! Vui lòng chuyển trạng thái sang 'Ngừng hoạt động'.");
        }

        _context.TaiKhoanNganHangs.Remove(entity);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> KiemTraDaPhatSinhGiaoDichAsync(long id)
    {
        var coTrongThuChi = await _context.ChungTuThuChis.AnyAsync(c => c.TaiKhoanNganHangId == id);
        return coTrongThuChi;
    }
}
