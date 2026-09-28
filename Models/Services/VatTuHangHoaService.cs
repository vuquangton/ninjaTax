using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public class VatTuHangHoaService : IVatTuHangHoaService
{
    private readonly AppDbContext _context;

    public VatTuHangHoaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<VatTuHangHoaItemViewModel>> LayDanhSachAsync(string? timKiem = null, LoaiVatTuHangHoa? loai = null)
    {
        var query = _context.VatTuHangHoas
            .Include(v => v.TaiKhoanKho)
            .Include(v => v.TaiKhoanDoanhThu)
            .Include(v => v.TaiKhoanGiaVon)
            .AsNoTracking()
            .AsQueryable();

        if (loai.HasValue)
        {
            query = query.Where(v => v.LoaiVatTu == loai.Value);
        }

        if (!string.IsNullOrWhiteSpace(timKiem))
        {
            var kw = timKiem.Trim().ToLower();
            query = query.Where(v => v.MaVatTu.ToLower().Contains(kw) || v.TenVatTu.ToLower().Contains(kw));
        }

        var entities = await query.OrderBy(v => v.MaVatTu).ToListAsync();

        var nhapIds = await _context.ChiTietNhapKhos.Select(c => c.VatTuHangHoaId).Distinct().ToHashSetAsync();
        var xuatIds = await _context.ChiTietXuatKhos.Select(c => c.VatTuHangHoaId).Distinct().ToHashSetAsync();
        var hdBanIds = await _context.ChiTietHoaDonBans.Select(c => c.VatTuHangHoaId).Distinct().ToHashSetAsync();
        var hdMuaIds = await _context.ChiTietHoaDonMuas.Select(c => c.VatTuHangHoaId).Distinct().ToHashSetAsync();

        return entities.Select(v => new VatTuHangHoaItemViewModel
        {
            Id = v.Id,
            MaVatTu = v.MaVatTu,
            TenVatTu = v.TenVatTu,
            DonViTinh = v.DonViTinh,
            LoaiVatTu = v.LoaiVatTu,
            ThueSuatVatMacDinh = v.ThueSuatVatMacDinh,
            DonGiaMuaGanNhat = v.DonGiaMuaGanNhat,
            DonGiaBanTieuChuan = v.DonGiaBanTieuChuan,
            DangHoatDong = v.DangHoatDong,
            DangTheoDoiTonKho = v.DangTheoDoiTonKho,
            DaPhatSinhGiaoDich = nhapIds.Contains(v.Id) || xuatIds.Contains(v.Id) || hdBanIds.Contains(v.Id) || hdMuaIds.Contains(v.Id),
            TaiKhoanKhoMa = v.TaiKhoanKho?.MaTaiKhoan,
            TaiKhoanDoanhThuMa = v.TaiKhoanDoanhThu?.MaTaiKhoan,
            TaiKhoanGiaVonMa = v.TaiKhoanGiaVon?.MaTaiKhoan
        }).ToList();
    }

    public async Task<VatTuHangHoa?> LayTheoIdAsync(long id)
    {
        return await _context.VatTuHangHoas
            .Include(v => v.TaiKhoanKho)
            .Include(v => v.TaiKhoanDoanhThu)
            .Include(v => v.TaiKhoanGiaVon)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<VatTuHangHoa?> LayTheoMaAsync(string maVatTu)
    {
        var cleanMa = maVatTu.Trim().ToUpperInvariant();
        return await _context.VatTuHangHoas
            .Include(v => v.TaiKhoanKho)
            .Include(v => v.TaiKhoanDoanhThu)
            .Include(v => v.TaiKhoanGiaVon)
            .FirstOrDefaultAsync(v => v.MaVatTu == cleanMa);
    }

    public async Task<(bool ThanhCong, string? ThongBao, long? VatTuId)> TaoMoiAsync(VatTuHangHoaCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MaVatTu))
        {
            return (false, "Mã vật tư không được để trống.", null);
        }

        if (string.IsNullOrWhiteSpace(model.TenVatTu))
        {
            return (false, "Tên vật tư hàng hóa không được để trống.", null);
        }

        var cleanMa = model.MaVatTu.Trim().ToUpperInvariant();

        var tonTai = await _context.VatTuHangHoas.AnyAsync(v => v.MaVatTu == cleanMa);
        if (tonTai)
        {
            return (false, $"Mã vật tư '{cleanMa}' đã tồn tại trong hệ thống.", null);
        }

        var entity = new VatTuHangHoa
        {
            MaVatTu = cleanMa,
            TenVatTu = model.TenVatTu.Trim(),
            DonViTinh = model.DonViTinh.Trim(),
            LoaiVatTu = model.LoaiVatTu,
            TaiKhoanKhoId = model.TaiKhoanKhoId,
            TaiKhoanDoanhThuId = model.TaiKhoanDoanhThuId,
            TaiKhoanGiaVonId = model.TaiKhoanGiaVonId,
            ThueSuatVatMacDinh = model.ThueSuatVatMacDinh,
            DonGiaMuaGanNhat = model.DonGiaMuaGanNhat,
            DonGiaBanTieuChuan = model.DonGiaBanTieuChuan,
            DangTheoDoiTonKho = model.DangTheoDoiTonKho,
            DangHoatDong = true
        };

        _context.VatTuHangHoas.Add(entity);
        await _context.SaveChangesAsync();

        return (true, null, entity.Id);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, VatTuHangHoaEditViewModel model)
    {
        var entity = await _context.VatTuHangHoas.FindAsync(id);
        if (entity == null)
        {
            return (false, "Không tìm thấy mặt hàng cần cập nhật.");
        }

        var cleanMa = model.MaVatTu.Trim().ToUpperInvariant();
        if (cleanMa != entity.MaVatTu)
        {
            var daPhatSinh = await KiemTraDaPhatSinhGiaoDichAsync(id);
            if (daPhatSinh)
            {
                return (false, "Không thể thay đổi mã của mặt hàng khi đã phát sinh chứng từ kho hoặc hóa đơn.");
            }

            var tonTai = await _context.VatTuHangHoas.AnyAsync(v => v.MaVatTu == cleanMa && v.Id != id);
            if (tonTai)
            {
                return (false, $"Mã vật tư '{cleanMa}' đã được sử dụng bởi mặt hàng khác.");
            }

            entity.MaVatTu = cleanMa;
        }

        entity.TenVatTu = model.TenVatTu.Trim();
        entity.DonViTinh = model.DonViTinh.Trim();
        entity.LoaiVatTu = model.LoaiVatTu;
        entity.TaiKhoanKhoId = model.TaiKhoanKhoId;
        entity.TaiKhoanDoanhThuId = model.TaiKhoanDoanhThuId;
        entity.TaiKhoanGiaVonId = model.TaiKhoanGiaVonId;
        entity.ThueSuatVatMacDinh = model.ThueSuatVatMacDinh;
        entity.DonGiaMuaGanNhat = model.DonGiaMuaGanNhat;
        entity.DonGiaBanTieuChuan = model.DonGiaBanTieuChuan;
        entity.DangTheoDoiTonKho = model.DangTheoDoiTonKho;
        entity.DangHoatDong = model.DangHoatDong;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id)
    {
        var entity = await _context.VatTuHangHoas.FindAsync(id);
        if (entity == null)
        {
            return (false, "Không tìm thấy mặt hàng cần xóa.");
        }

        var daPhatSinh = await KiemTraDaPhatSinhGiaoDichAsync(id);
        if (daPhatSinh)
        {
            return (false, $"Mặt hàng '{entity.MaVatTu}' đã phát sinh giao dịch nhập/xuất kho hoặc hóa đơn. Không được phép xóa! Vui lòng chuyển trạng thái sang 'Ngừng hoạt động'.");
        }

        _context.VatTuHangHoas.Remove(entity);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> KiemTraDaPhatSinhGiaoDichAsync(long id)
    {
        var coTrongNhap = await _context.ChiTietNhapKhos.AnyAsync(c => c.VatTuHangHoaId == id);
        if (coTrongNhap) return true;

        var coTrongXuat = await _context.ChiTietXuatKhos.AnyAsync(c => c.VatTuHangHoaId == id);
        if (coTrongXuat) return true;

        var coTrongHdBan = await _context.ChiTietHoaDonBans.AnyAsync(c => c.VatTuHangHoaId == id);
        if (coTrongHdBan) return true;

        var coTrongHdMua = await _context.ChiTietHoaDonMuas.AnyAsync(c => c.VatTuHangHoaId == id);
        if (coTrongHdMua) return true;

        return false;
    }
}
