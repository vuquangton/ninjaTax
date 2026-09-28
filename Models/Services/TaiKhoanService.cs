using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

/// <summary>
/// Dịch vụ quản trị Hệ thống Tài khoản kế toán (COA Management Service).
/// Tuân thủ quy tắc nghiệp vụ Thông tư 99/2025/TT-BTC:
/// - Quản lý cây tài khoản đa cấp cha-con (Hierarchical tree).
/// - Tự động cập nhật LaTaiKhoanSoCai = true cho tài khoản mẹ khi sinh tài khoản con.
/// - Ngăn chặn hạch toán vào tài khoản tổng hợp.
/// - Bảo vệ tính toàn vẹn dữ liệu: Cấm xóa hoặc đổi mã tài khoản khi đã phát sinh bút toán.
/// </summary>
public class TaiKhoanService : ITaiKhoanService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaiKhoanService> _logger;

    public TaiKhoanService(AppDbContext context, ILogger<TaiKhoanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TaiKhoanViewModel>> LayDanhSachAsync(string? timKiem = null, LoaiTaiKhoan? loaiTaiKhoan = null)
    {
        var query = _context.TaiKhoans
            .Include(t => t.TaiKhoanMe)
            .Include(t => t.TaiKhoanCons)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(timKiem))
        {
            var k = timKiem.Trim().ToLower();
            query = query.Where(t => t.MaTaiKhoan.ToLower().Contains(k) || t.TenTaiKhoan.ToLower().Contains(k));
        }

        if (loaiTaiKhoan.HasValue)
        {
            query = query.Where(t => t.LoaiTaiKhoan == loaiTaiKhoan.Value);
        }

        var accounts = await query.OrderBy(t => t.MaTaiKhoan).ToListAsync();

        // Lấy danh sách ID các tài khoản đã từng phát sinh giao dịch trong ChiTietButToans
        var inUseIds = await _context.ChiTietButToans
            .Select(c => c.TaiKhoanNoId)
            .Concat(_context.ChiTietButToans.Select(c => c.TaiKhoanCoId))
            .Distinct()
            .ToListAsync();
        var inUseSet = new HashSet<long>(inUseIds);

        return accounts.Select(t => new TaiKhoanViewModel
        {
            Id = t.Id,
            MaTaiKhoan = t.MaTaiKhoan,
            TenTaiKhoan = t.TenTaiKhoan,
            BacTaiKhoan = t.BacTaiKhoan,
            TaiKhoanMeId = t.TaiKhoanMeId,
            MaTaiKhoanMe = t.TaiKhoanMe?.MaTaiKhoan,
            TenTaiKhoanMe = t.TaiKhoanMe?.TenTaiKhoan,
            LoaiTaiKhoan = t.LoaiTaiKhoan,
            TinhChat = t.TinhChat,
            LaTaiKhoanSoCai = t.LaTaiKhoanSoCai,
            DangHoatDong = t.DangHoatDong,
            SoLuongCon = t.TaiKhoanCons?.Count ?? 0,
            DaPhatSinhGiaoDich = inUseSet.Contains(t.Id)
        }).ToList();
    }

    public async Task<TaiKhoan?> LayTheoIdAsync(long id)
    {
        return await _context.TaiKhoans
            .Include(t => t.TaiKhoanMe)
            .Include(t => t.TaiKhoanCons)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaiKhoan?> LayTheoMaAsync(string maTaiKhoan)
    {
        return await _context.TaiKhoans
            .Include(t => t.TaiKhoanMe)
            .Include(t => t.TaiKhoanCons)
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Trim());
    }

    public async Task<(bool ThanhCong, string? ThongBao, long? TaiKhoanId)> TaoMoiAsync(TaiKhoanCreateEditViewModel model)
    {
        var maTk = model.MaTaiKhoan.Trim();

        // 1. Kiểm tra trùng mã
        if (await _context.TaiKhoans.AnyAsync(t => t.MaTaiKhoan == maTk))
        {
            return (false, $"Mã tài khoản [{maTk}] đã tồn tại trong hệ thống.", null);
        }

        TaiKhoan? tkMe = null;
        if (model.TaiKhoanMeId.HasValue)
        {
            tkMe = await _context.TaiKhoans.FindAsync(model.TaiKhoanMeId.Value);
            if (tkMe == null)
            {
                return (false, "Tài khoản mẹ không tồn tại.", null);
            }

            // Bất biến: Mã con phải bắt đầu bằng mã mẹ
            if (!maTk.StartsWith(tkMe.MaTaiKhoan))
            {
                return (false, $"Mã tài khoản con [{maTk}] phải bắt đầu bằng mã tài khoản mẹ [{tkMe.MaTaiKhoan}].", null);
            }

            if (!tkMe.DangHoatDong)
            {
                return (false, $"Không thể tạo tài khoản con cho tài khoản mẹ [{tkMe.MaTaiKhoan}] đang ngừng hoạt động.", null);
            }
        }

        var newAccount = new TaiKhoan
        {
            MaTaiKhoan = maTk,
            TenTaiKhoan = model.TenTaiKhoan.Trim(),
            TaiKhoanMeId = model.TaiKhoanMeId,
            BacTaiKhoan = tkMe != null ? tkMe.BacTaiKhoan + 1 : model.BacTaiKhoan,
            LoaiTaiKhoan = tkMe != null ? tkMe.LoaiTaiKhoan : model.LoaiTaiKhoan,
            TinhChat = model.TinhChat,
            LaTaiKhoanSoCai = false, // Mặc định tài khoản mới tạo là tài khoản lá
            DangHoatDong = model.DangHoatDong
        };

        // Khi tài khoản mẹ có thêm con, tự động chuyển tài khoản mẹ thành tài khoản tổng hợp (LaTaiKhoanSoCai = true)
        if (tkMe != null && !tkMe.LaTaiKhoanSoCai)
        {
            tkMe.LaTaiKhoanSoCai = true;
            _logger.LogInformation("Tài khoản mẹ [{MaTkMe}] được tự động chuyển thành tài khoản tổng hợp (LaTaiKhoanSoCai = true)", tkMe.MaTaiKhoan);
        }

        await _context.TaiKhoans.AddAsync(newAccount);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tạo mới tài khoản thành công: [{MaTk}] - {TenTk}", newAccount.MaTaiKhoan, newAccount.TenTaiKhoan);
        return (true, null, newAccount.Id);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, TaiKhoanCreateEditViewModel model)
    {
        var account = await _context.TaiKhoans.FindAsync(id);
        if (account == null)
        {
            return (false, "Tài khoản không tồn tại.");
        }

        var newCode = model.MaTaiKhoan.Trim();

        // Kiểm tra xem tài khoản đã phát sinh giao dịch chưa
        var hasTransactions = await _context.ChiTietButToans
            .AnyAsync(c => c.TaiKhoanNoId == id || c.TaiKhoanCoId == id);

        // Bất biến: Nếu đã có phát sinh giao dịch, cấm tuyệt đối thay đổi số hiệu tài khoản
        if (hasTransactions && account.MaTaiKhoan != newCode)
        {
            return (false, $"Tài khoản [{account.MaTaiKhoan}] đã phát sinh giao dịch ghi sổ. Nghiêm cấm thay đổi số hiệu tài khoản.");
        }

        // Kiểm tra trùng mã nếu đổi sang mã khác
        if (account.MaTaiKhoan != newCode && await _context.TaiKhoans.AnyAsync(t => t.MaTaiKhoan == newCode && t.Id != id))
        {
            return (false, $"Mã tài khoản [{newCode}] đã được sử dụng bởi một tài khoản khác.");
        }

        // Kiểm tra tiền tố mã tài khoản mẹ nếu có
        if (account.TaiKhoanMeId.HasValue)
        {
            var tkMe = await _context.TaiKhoans.FindAsync(account.TaiKhoanMeId.Value);
            if (tkMe != null && !newCode.StartsWith(tkMe.MaTaiKhoan))
            {
                return (false, $"Mã tài khoản [{newCode}] phải bắt đầu bằng mã tài khoản mẹ [{tkMe.MaTaiKhoan}].");
            }
        }

        account.MaTaiKhoan = newCode;
        account.TenTaiKhoan = model.TenTaiKhoan.Trim();
        account.TinhChat = model.TinhChat;
        account.DangHoatDong = model.DangHoatDong;

        // Nếu người dùng chủ động đánh dấu là TK Sổ cái
        if (model.LaTaiKhoanSoCai && !account.LaTaiKhoanSoCai)
        {
            account.LaTaiKhoanSoCai = true;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id)
    {
        var account = await _context.TaiKhoans
            .Include(t => t.TaiKhoanCons)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (account == null)
        {
            return (false, "Tài khoản không tồn tại.");
        }

        // 1. Kiểm tra tài khoản con
        if (account.TaiKhoanCons != null && account.TaiKhoanCons.Count > 0)
        {
            return (false, $"Tài khoản [{account.MaTaiKhoan}] đang có {account.TaiKhoanCons.Count} tài khoản con. Vui lòng xóa các tài khoản con trước.");
        }

        // 2. Kiểm tra phát sinh giao dịch
        var transactionCount = await _context.ChiTietButToans
            .CountAsync(c => c.TaiKhoanNoId == id || c.TaiKhoanCoId == id);

        if (transactionCount > 0)
        {
            return (false, $"Tài khoản [{account.MaTaiKhoan}] đã phát sinh {transactionCount} dòng bút toán trong sổ sách. Nghiêm cấm xóa!");
        }

        var parentId = account.TaiKhoanMeId;

        _context.TaiKhoans.Remove(account);
        await _context.SaveChangesAsync();

        // 3. Nếu tài khoản mẹ sau khi xóa không còn con nào khác, cập nhật lại LaTaiKhoanSoCai = false nếu phù hợp
        if (parentId.HasValue)
        {
            var remainingChildren = await _context.TaiKhoans.CountAsync(t => t.TaiKhoanMeId == parentId.Value);
            if (remainingChildren == 0)
            {
                var parent = await _context.TaiKhoans.FindAsync(parentId.Value);
                if (parent != null)
                {
                    parent.LaTaiKhoanSoCai = false;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Tài khoản mẹ [{MaTkMe}] không còn tài khoản con nào, cập nhật LaTaiKhoanSoCai = false", parent.MaTaiKhoan);
                }
            }
        }

        _logger.LogInformation("Xóa thành công tài khoản: [{MaTk}]", account.MaTaiKhoan);
        return (true, null);
    }

    public async Task<List<TaiKhoan>> LayDanhSachTaiKhoanMeAsync()
    {
        return await _context.TaiKhoans
            .Where(t => t.DangHoatDong)
            .OrderBy(t => t.MaTaiKhoan)
            .AsNoTracking()
            .ToListAsync();
    }
}
