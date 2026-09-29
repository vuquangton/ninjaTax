using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public class UnitConversionService : IUnitConversionService
{
    private readonly AppDbContext _context;

    public UnitConversionService(AppDbContext context)
    {
        _context = context;
    }

    public decimal QuyDoiVeDonViCoBan(decimal soLuongGiaoDich, decimal tyLeQuyDoi, PhepTinhQuyDoi phepTinh = PhepTinhQuyDoi.Nhan)
    {
        if (tyLeQuyDoi <= 0)
        {
            throw new ArgumentException("Tỷ lệ quy đổi phải lớn hơn 0.", nameof(tyLeQuyDoi));
        }

        return phepTinh switch
        {
            PhepTinhQuyDoi.Nhan => decimal.Round(soLuongGiaoDich * tyLeQuyDoi, 4, MidpointRounding.AwayFromZero),
            PhepTinhQuyDoi.Chia => decimal.Round(soLuongGiaoDich / tyLeQuyDoi, 4, MidpointRounding.AwayFromZero),
            _ => throw new ArgumentOutOfRangeException(nameof(phepTinh), phepTinh, "Phép tính quy đổi không hợp lệ.")
        };
    }

    public decimal QuyDoiTuDonViCoBan(decimal soLuongCoBan, decimal tyLeQuyDoi, PhepTinhQuyDoi phepTinh = PhepTinhQuyDoi.Nhan)
    {
        if (tyLeQuyDoi <= 0)
        {
            throw new ArgumentException("Tỷ lệ quy đổi phải lớn hơn 0.", nameof(tyLeQuyDoi));
        }

        return phepTinh switch
        {
            PhepTinhQuyDoi.Nhan => decimal.Round(soLuongCoBan / tyLeQuyDoi, 4, MidpointRounding.AwayFromZero),
            PhepTinhQuyDoi.Chia => decimal.Round(soLuongCoBan * tyLeQuyDoi, 4, MidpointRounding.AwayFromZero),
            _ => throw new ArgumentOutOfRangeException(nameof(phepTinh), phepTinh, "Phép tính quy đổi không hợp lệ.")
        };
    }

    public async Task<List<DonViTinhQuyDoi>> GetConversionUnitsAsync(long vatTuId)
    {
        return await _context.DonViTinhQuyDois
            .Where(u => u.VatTuHangHoaId == vatTuId && u.DangHoatDong)
            .OrderBy(u => u.TenDonViTinh)
            .ToListAsync();
    }

    public async Task<DonViTinhQuyDoi> SaveConversionUnitAsync(DonViTinhQuyDoi unit)
    {
        if (unit.TyLeQuyDoi <= 0)
        {
            throw new ArgumentException("Tỷ lệ quy đổi phải lớn hơn 0.");
        }

        if (string.IsNullOrWhiteSpace(unit.TenDonViTinh))
        {
            throw new ArgumentException("Tên đơn vị tính không được để trống.");
        }

        var vatTu = await _context.VatTuHangHoas.FindAsync(unit.VatTuHangHoaId);
        if (vatTu == null)
        {
            throw new InvalidOperationException($"Không tìm thấy vật tư có Id={unit.VatTuHangHoaId}.");
        }

        if (string.Equals(unit.TenDonViTinh.Trim(), vatTu.DonViTinh.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Đơn vị tính phụ không được trùng với đơn vị tính cơ bản ({vatTu.DonViTinh}).");
        }

        var existing = await _context.DonViTinhQuyDois
            .FirstOrDefaultAsync(u => u.VatTuHangHoaId == unit.VatTuHangHoaId 
                                   && u.Id != unit.Id 
                                   && u.TenDonViTinh.ToLower() == unit.TenDonViTinh.Trim().ToLower());
        if (existing != null)
        {
            throw new InvalidOperationException($"Đơn vị tính '{unit.TenDonViTinh}' đã tồn tại cho mặt hàng này.");
        }

        if (unit.Id == 0)
        {
            _context.DonViTinhQuyDois.Add(unit);
        }
        else
        {
            _context.DonViTinhQuyDois.Update(unit);
        }

        await _context.SaveChangesAsync();
        return unit;
    }

    public async Task<bool> DeleteConversionUnitAsync(long id)
    {
        var unit = await _context.DonViTinhQuyDois.FindAsync(id);
        if (unit == null) return false;

        _context.DonViTinhQuyDois.Remove(unit);
        await _context.SaveChangesAsync();
        return true;
    }
}
