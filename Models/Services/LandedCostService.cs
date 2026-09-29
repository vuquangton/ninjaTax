using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public class LandedCostService : ILandedCostService
{
    private readonly AppDbContext _context;
    private readonly IButToanService _butToanService;
    private readonly ILogger<LandedCostService> _logger;

    public LandedCostService(
        AppDbContext context, 
        IButToanService butToanService, 
        ILogger<LandedCostService> logger)
    {
        _context = context;
        _butToanService = butToanService;
        _logger = logger;
    }

    public async Task<ChungTuChiPhiMuaHang?> GetByIdAsync(long id)
    {
        return await _context.ChungTuChiPhiMuaHangs
            .Include(c => c.NhaCungCapDichVu)
            .Include(c => c.ChiNhanh)
            .Include(c => c.ButToan)
            .Include(c => c.ChiTietPhanBos)
                .ThenInclude(p => p.ChiTietNhapKho)
                    .ThenInclude(n => n!.VatTuHangHoa)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<ChungTuChiPhiMuaHang>> GetAllAsync(long? chiNhanhId = null)
    {
        var query = _context.ChungTuChiPhiMuaHangs
            .Include(c => c.NhaCungCapDichVu)
            .Include(c => c.ChiTietPhanBos)
            .AsQueryable();

        if (chiNhanhId.HasValue)
        {
            query = query.Where(c => c.ChiNhanhId == chiNhanhId.Value);
        }

        return await query.OrderByDescending(c => c.NgayChungTu).ToListAsync();
    }

    public async Task<ChungTuChiPhiMuaHang> CreateAsync(ChungTuChiPhiMuaHang chungTu)
    {
        if (chungTu.TongChiPhi <= 0)
        {
            throw new ArgumentException("Tổng chi phí mua hàng phải lớn hơn 0.");
        }

        chungTu.NgayTao = DateTime.UtcNow;
        chungTu.DaPhanBo = false;
        
        await _context.ChungTuChiPhiMuaHangs.AddAsync(chungTu);
        await _context.SaveChangesAsync();
        return chungTu;
    }

    public async Task<(bool Success, string? ErrorMessage)> AllocateCostAsync(
        long chungTuChiPhiId, 
        List<long> chiTietNhapKhoIds, 
        PhuongThucPhanBoChiPhi phuongThuc,
        long taiKhoanChiPhiId,
        long taiKhoanDoiUngId)
    {
        var chungTu = await _context.ChungTuChiPhiMuaHangs
            .Include(c => c.ChiTietPhanBos)
            .FirstOrDefaultAsync(c => c.Id == chungTuChiPhiId);

        if (chungTu == null)
        {
            return (false, "Không tìm thấy chứng từ chi phí mua hàng.");
        }

        if (chungTu.DaPhanBo)
        {
            return (false, "Chứng từ chi phí này đã được phân bổ.");
        }

        // Kiểm tra khóa sổ
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh != null && !cauHinh.ChoPhepGhiSo(chungTu.NgayHachToan))
        {
            return (false, $"Ngày hạch toán {chungTu.NgayHachToan:dd/MM/yyyy} nằm trong kỳ kế toán đã khóa sổ.");
        }

        if (chiTietNhapKhoIds == null || chiTietNhapKhoIds.Count == 0)
        {
            return (false, "Vui lòng chọn ít nhất một dòng nhập kho để phân bổ.");
        }

        var lines = await _context.ChiTietNhapKhos
            .Where(c => chiTietNhapKhoIds.Contains(c.Id))
            .ToListAsync();

        if (lines.Count != chiTietNhapKhoIds.Count)
        {
            return (false, "Một số dòng chi tiết nhập kho không hợp lệ.");
        }

        decimal tongTieuChi = phuongThuc == PhuongThucPhanBoChiPhi.TheoGiaTri
            ? lines.Sum(l => l.ThanhTien)
            : lines.Sum(l => l.SoLuong);

        if (tongTieuChi <= 0)
        {
            return (false, "Tổng tiêu chí phân bổ (thành tiền hoặc số lượng) phải lớn hơn 0.");
        }

        decimal tongChiPhi = chungTu.TongChiPhi;
        decimal tongDaPhanBo = 0m;
        var phanBoList = new List<ChiPhiMuaHangPhanBo>();

        // Thuật toán phân bổ kèm bù trừ chênh lệch làm tròn xu (Penny-rounding absorption)
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            decimal tienPhanBo;

            if (i == lines.Count - 1)
            {
                // Dòng cuối cùng hấp thụ toàn bộ phần dư còn lại để tổng phân bổ bằng đúng tổng chi phí
                tienPhanBo = tongChiPhi - tongDaPhanBo;
            }
            else
            {
                decimal tieuChiLine = phuongThuc == PhuongThucPhanBoChiPhi.TheoGiaTri ? line.ThanhTien : line.SoLuong;
                tienPhanBo = Math.Round((tieuChiLine / tongTieuChi) * tongChiPhi, 4);
                tongDaPhanBo += tienPhanBo;
            }

            line.ChiPhiMuaHangPhanBo += tienPhanBo;

            phanBoList.Add(new ChiPhiMuaHangPhanBo
            {
                ChungTuChiPhiMuaHangId = chungTu.Id,
                ChiTietNhapKhoId = line.Id,
                SoTienPhanBo = tienPhanBo
            });
        }

        // Tạo bút toán hạch toán Nợ 1561 / Có 331 (hoặc 111/112)
        var butToan = new ButToan
        {
            SoChungTu = $"PKT-CPMH-{chungTu.SoChungTu}",
            NgayChungTu = chungTu.NgayChungTu,
            NgayHachToan = chungTu.NgayHachToan,
            SoChungTuGoc = chungTu.SoChungTu,
            NgayChungTuGoc = chungTu.NgayChungTu,
            DienGiai = $"Phân bổ chi phí mua hàng theo {chungTu.SoChungTu}: {chungTu.DienGiai}",
            TrangThai = TrangThaiButToan.DaGhiSo,
            TongTien = tongChiPhi,
            TongNo = tongChiPhi,
            TongCo = tongChiPhi,
            ChiTietButToans = new List<ChiTietButToan>
            {
                new ChiTietButToan
                {
                    TaiKhoanNoId = taiKhoanChiPhiId,
                    TaiKhoanCoId = taiKhoanDoiUngId,
                    SoTien = tongChiPhi,
                    DienGiai = $"Chi phí mua hàng phân bổ vào giá trị nhập kho theo {chungTu.SoChungTu}"
                }
            }
        };

        if (chungTu.TienThueVat > 0)
        {
            var tk1331 = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1331");
            if (tk1331 != null)
            {
                butToan.ChiTietButToans.Add(new ChiTietButToan
                {
                    TaiKhoanNoId = tk1331.Id,
                    TaiKhoanCoId = taiKhoanDoiUngId,
                    SoTien = chungTu.TienThueVat,
                    DienGiai = $"Thuế GTGT chi phí mua hàng theo {chungTu.SoChungTu}"
                });
                butToan.TongTien += chungTu.TienThueVat;
                butToan.TongNo += chungTu.TienThueVat;
                butToan.TongCo += chungTu.TienThueVat;
            }
        }

        await _context.ButToans.AddAsync(butToan);
        await _context.SaveChangesAsync();

        chungTu.ButToanId = butToan.Id;
        chungTu.DaPhanBo = true;
        chungTu.PhuongThucPhanBo = phuongThuc;
        await _context.ChiPhiMuaHangPhanBos.AddRangeAsync(phanBoList);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã phân bổ chi phí mua hàng {SoChungTu} thành công: {TongChiPhi:N0} VNĐ vào {Count} dòng nhập kho.", chungTu.SoChungTu, tongChiPhi, lines.Count);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> CancelAllocationAsync(long chungTuChiPhiId)
    {
        var chungTu = await _context.ChungTuChiPhiMuaHangs
            .Include(c => c.ChiTietPhanBos)
                .ThenInclude(p => p.ChiTietNhapKho)
            .FirstOrDefaultAsync(c => c.Id == chungTuChiPhiId);

        if (chungTu == null)
        {
            return (false, "Không tìm thấy chứng từ chi phí mua hàng.");
        }

        if (!chungTu.DaPhanBo)
        {
            return (false, "Chứng từ này chưa được phân bổ, không thể hủy.");
        }

        // Kiểm tra khóa sổ
        var cauHinh = await _context.CauHinhKeToans.FirstOrDefaultAsync();
        if (cauHinh != null && !cauHinh.ChoPhepGhiSo(chungTu.NgayHachToan))
        {
            return (false, "Kỳ kế toán đã bị khóa sổ. Không thể hủy phân bổ.");
        }

        // Hoàn tác chi phí phân bổ trên các dòng nhập kho
        foreach (var phanBo in chungTu.ChiTietPhanBos)
        {
            if (phanBo.ChiTietNhapKho != null)
            {
                phanBo.ChiTietNhapKho.ChiPhiMuaHangPhanBo = Math.Max(0, phanBo.ChiTietNhapKho.ChiPhiMuaHangPhanBo - phanBo.SoTienPhanBo);
            }
        }

        _context.ChiPhiMuaHangPhanBos.RemoveRange(chungTu.ChiTietPhanBos);

        // Xóa bút toán kế toán
        if (chungTu.ButToanId.HasValue)
        {
            var butToan = await _context.ButToans
                .Include(b => b.ChiTietButToans)
                .FirstOrDefaultAsync(b => b.Id == chungTu.ButToanId.Value);

            if (butToan != null)
            {
                _context.ChiTietButToans.RemoveRange(butToan.ChiTietButToans);
                _context.ButToans.Remove(butToan);
            }
            chungTu.ButToanId = null;
        }

        chungTu.DaPhanBo = false;
        await _context.SaveChangesAsync();

        return (true, null);
    }
}
