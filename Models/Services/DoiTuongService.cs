using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class DoiTuongService : IDoiTuongService
{
    private readonly AppDbContext _context;

    public DoiTuongService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoiTuongItemViewModel>> LayDanhSachAsync(string? timKiem = null, LoaiDoiTuong? loai = null)
    {
        var query = _context.DoiTuongs.AsNoTracking().AsQueryable();

        if (loai.HasValue)
        {
            query = query.Where(d => d.Loai == loai.Value);
        }

        if (!string.IsNullOrWhiteSpace(timKiem))
        {
            var kw = timKiem.Trim().ToLower();
            query = query.Where(d => d.MaDoiTuong.ToLower().Contains(kw) ||
                                     d.TenDoiTuong.ToLower().Contains(kw) ||
                                     (d.MaSoThue != null && d.MaSoThue.Contains(kw)) ||
                                     (d.SoDienThoai != null && d.SoDienThoai.Contains(kw)));
        }

        var entities = await query.OrderBy(d => d.MaDoiTuong).ToListAsync();

        var doiTuongIdsWithTransactions = await _context.ChiTietButToans
            .Where(c => c.DoiTuongId.HasValue)
            .Select(c => c.DoiTuongId!.Value)
            .Distinct()
            .ToHashSetAsync();

        var invoiceCustomers = await _context.HoaDonBanHangs
            .Select(h => h.KhachHangId)
            .Distinct()
            .ToHashSetAsync();

        var invoiceSuppliers = await _context.HoaDonMuaHangs
            .Select(h => h.NhaCungCapId)
            .Distinct()
            .ToHashSetAsync();

        return entities.Select(d => new DoiTuongItemViewModel
        {
            Id = d.Id,
            MaDoiTuong = d.MaDoiTuong,
            TenDoiTuong = d.TenDoiTuong,
            Loai = d.Loai,
            MaSoThue = d.MaSoThue,
            DiaChi = d.DiaChi,
            SoDienThoai = d.SoDienThoai,
            Email = d.Email,
            NguoiLienHe = d.NguoiLienHe,
            DangHoatDong = d.DangHoatDong,
            DaPhatSinhGiaoDich = doiTuongIdsWithTransactions.Contains(d.Id) ||
                                 invoiceCustomers.Contains(d.Id) ||
                                 invoiceSuppliers.Contains(d.Id)
        }).ToList();
    }

    public async Task<DoiTuong?> LayTheoIdAsync(long id)
    {
        return await _context.DoiTuongs.FindAsync(id);
    }

    public async Task<DoiTuong?> LayTheoMaAsync(string maDoiTuong)
    {
        var cleanMa = maDoiTuong.Trim().ToUpperInvariant();
        return await _context.DoiTuongs.FirstOrDefaultAsync(d => d.MaDoiTuong == cleanMa);
    }

    public async Task<(bool ThanhCong, string? ThongBao, long? DoiTuongId)> TaoMoiAsync(DoiTuongCreateEditViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MaDoiTuong))
        {
            return (false, "Mã đối tượng không được để trống.", null);
        }

        if (string.IsNullOrWhiteSpace(model.TenDoiTuong))
        {
            return (false, "Tên đối tượng không được để trống.", null);
        }

        var cleanMa = model.MaDoiTuong.Trim().ToUpperInvariant();

        var tonTai = await _context.DoiTuongs.AnyAsync(d => d.MaDoiTuong == cleanMa);
        if (tonTai)
        {
            return (false, $"Mã đối tượng '{cleanMa}' đã tồn tại trong hệ thống.", null);
        }

        var entity = new DoiTuong
        {
            MaDoiTuong = cleanMa,
            TenDoiTuong = model.TenDoiTuong.Trim(),
            Loai = model.Loai,
            MaSoThue = string.IsNullOrWhiteSpace(model.MaSoThue) ? null : model.MaSoThue.Trim(),
            DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim(),
            SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLowerInvariant(),
            NguoiLienHe = string.IsNullOrWhiteSpace(model.NguoiLienHe) ? null : model.NguoiLienHe.Trim(),
            SoTaiKhoanNganHang = string.IsNullOrWhiteSpace(model.SoTaiKhoanNganHang) ? null : model.SoTaiKhoanNganHang.Trim(),
            TenNganHang = string.IsNullOrWhiteSpace(model.TenNganHang) ? null : model.TenNganHang.Trim(),
            DangHoatDong = model.DangHoatDong
        };

        _context.DoiTuongs.Add(entity);
        await _context.SaveChangesAsync();

        return (true, null, entity.Id);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, DoiTuongCreateEditViewModel model)
    {
        var entity = await _context.DoiTuongs.FindAsync(id);
        if (entity == null)
        {
            return (false, "Không tìm thấy đối tượng cần cập nhật.");
        }

        var cleanMa = model.MaDoiTuong.Trim().ToUpperInvariant();
        if (cleanMa != entity.MaDoiTuong)
        {
            var daPhatSinh = await KiemTraDaPhatSinhGiaoDichAsync(id);
            if (daPhatSinh)
            {
                return (false, "Không thể thay đổi mã của đối tượng khi đã phát sinh chứng từ kế toán.");
            }

            var tonTai = await _context.DoiTuongs.AnyAsync(d => d.MaDoiTuong == cleanMa && d.Id != id);
            if (tonTai)
            {
                return (false, $"Mã đối tượng '{cleanMa}' đã được sử dụng bởi đối tượng khác.");
            }

            entity.MaDoiTuong = cleanMa;
        }

        entity.TenDoiTuong = model.TenDoiTuong.Trim();
        entity.Loai = model.Loai;
        entity.MaSoThue = string.IsNullOrWhiteSpace(model.MaSoThue) ? null : model.MaSoThue.Trim();
        entity.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();
        entity.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
        entity.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLowerInvariant();
        entity.NguoiLienHe = string.IsNullOrWhiteSpace(model.NguoiLienHe) ? null : model.NguoiLienHe.Trim();
        entity.SoTaiKhoanNganHang = string.IsNullOrWhiteSpace(model.SoTaiKhoanNganHang) ? null : model.SoTaiKhoanNganHang.Trim();
        entity.TenNganHang = string.IsNullOrWhiteSpace(model.TenNganHang) ? null : model.TenNganHang.Trim();
        entity.DangHoatDong = model.DangHoatDong;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id)
    {
        var entity = await _context.DoiTuongs.FindAsync(id);
        if (entity == null)
        {
            return (false, "Không tìm thấy đối tượng cần xóa.");
        }

        var daPhatSinh = await KiemTraDaPhatSinhGiaoDichAsync(id);
        if (daPhatSinh)
        {
            return (false, $"Đối tượng '{entity.MaDoiTuong}' đã phát sinh giao dịch trong sổ sách hoặc hóa đơn. Không được phép xóa! Vui lòng chuyển trạng thái sang 'Ngừng giao dịch'.");
        }

        _context.DoiTuongs.Remove(entity);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> KiemTraDaPhatSinhGiaoDichAsync(long id)
    {
        var coTrongButToan = await _context.ChiTietButToans.AnyAsync(c => c.DoiTuongId == id);
        if (coTrongButToan) return true;

        var coTrongHdBan = await _context.HoaDonBanHangs.AnyAsync(h => h.KhachHangId == id);
        if (coTrongHdBan) return true;

        var coTrongHdMua = await _context.HoaDonMuaHangs.AnyAsync(h => h.NhaCungCapId == id);
        if (coTrongHdMua) return true;

        var coTrongThuChi = await _context.ChungTuThuChis.AnyAsync(c => c.DoiTuongId == id);
        if (coTrongThuChi) return true;

        return false;
    }
}
