using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(AppDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PhongBan>> GetDepartmentsAsync(long? branchId = null)
    {
        var query = _context.PhongBans
            .Include(p => p.ChiNhanh)
            .Include(p => p.PhongBanCha)
            .Include(p => p.TruongPhong)
            .Include(p => p.NhanViens)
            .AsQueryable();

        if (branchId.HasValue && branchId.Value > 0)
        {
            query = query.Where(p => p.ChiNhanhId == branchId.Value);
        }

        return await query
            .OrderBy(p => p.ChiNhanhId)
            .ThenBy(p => p.MaPhongBan)
            .ToListAsync();
    }

    public async Task<PhongBan?> GetDepartmentByIdAsync(long id)
    {
        return await _context.PhongBans
            .Include(p => p.ChiNhanh)
            .Include(p => p.PhongBanCha)
            .Include(p => p.TruongPhong)
            .Include(p => p.NhanViens)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PhongBan> SaveDepartmentAsync(PhongBan department)
    {
        department.MaPhongBan = department.MaPhongBan.Trim();
        department.TenPhongBan = department.TenPhongBan.Trim();

        if (string.IsNullOrWhiteSpace(department.MaPhongBan))
        {
            throw new ArgumentException("Mã phòng ban không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(department.TenPhongBan))
        {
            throw new ArgumentException("Tên phòng ban không được để trống.");
        }

        // Kiểm tra trùng mã trong cùng chi nhánh
        var isDuplicate = await _context.PhongBans.AnyAsync(p =>
            p.ChiNhanhId == department.ChiNhanhId &&
            p.MaPhongBan == department.MaPhongBan &&
            p.Id != department.Id);

        if (isDuplicate)
        {
            throw new ArgumentException($"Mã phòng ban '{department.MaPhongBan}' đã tồn tại trong chi nhánh này.");
        }

        // Tự động gán mã tài khoản chi phí mặc định nếu chưa chỉ định
        if (string.IsNullOrWhiteSpace(department.MaTaiKhoanChiPhi))
        {
            department.MaTaiKhoanChiPhi = department.LoaiPhongBan.LayMaTaiKhoanChiPhiMacDinh();
        }

        // Kiểm tra tính chu trình trên cây phân cấp
        if (department.PhongBanChaId.HasValue && department.PhongBanChaId.Value > 0)
        {
            var allDepartments = await _context.PhongBans
                .AsNoTracking()
                .Select(p => new { p.Id, p.PhongBanChaId })
                .ToListAsync();

            var parentLookup = allDepartments.ToDictionary(p => p.Id, p => p.PhongBanChaId);

            PhongBan.XacThucPhongBanCha(department.Id, department.PhongBanChaId, id =>
                parentLookup.TryGetValue(id, out var pId) ? pId : null);
        }

        if (department.Id == 0)
        {
            department.NgayTao = DateTime.UtcNow;
            _context.PhongBans.Add(department);
        }
        else
        {
            var existing = await _context.PhongBans.FindAsync(department.Id);
            if (existing == null) throw new KeyNotFoundException("Không tìm thấy phòng ban.");

            existing.ChiNhanhId = department.ChiNhanhId;
            existing.PhongBanChaId = department.PhongBanChaId;
            existing.MaPhongBan = department.MaPhongBan;
            existing.TenPhongBan = department.TenPhongBan;
            existing.TenTiengAnh = department.TenTiengAnh;
            existing.LoaiPhongBan = department.LoaiPhongBan;
            existing.MaTaiKhoanChiPhi = department.MaTaiKhoanChiPhi;
            existing.TruongPhongId = department.TruongPhongId;
            existing.LaTrungTamLoiNhuan = department.LaTrungTamLoiNhuan;
            existing.DangHoatDong = department.DangHoatDong;
            existing.GhiChu = department.GhiChu;
            existing.NgayCapNhat = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã lưu phòng ban {Ma} - {Ten}", department.MaPhongBan, department.TenPhongBan);
        return department;
    }

    public async Task<bool> DeleteDepartmentAsync(long id)
    {
        var dept = await _context.PhongBans.FindAsync(id);
        if (dept == null) return false;

        // Bất biến: Chặn xóa nếu có phòng ban con
        var hasChildren = await _context.PhongBans.AnyAsync(p => p.PhongBanChaId == id);
        if (hasChildren)
        {
            throw new InvalidOperationException("Không thể xóa phòng ban đang có các phòng ban/bộ phận con trực thuộc.");
        }

        // Bất biến: Chặn xóa nếu đang có nhân viên
        var hasEmployees = await _context.NhanViens.AnyAsync(n => n.PhongBanId == id);
        if (hasEmployees)
        {
            throw new InvalidOperationException("Không thể xóa phòng ban đang có nhân viên trực thuộc.");
        }

        // Bất biến: Chặn xóa nếu đã phát sinh chứng từ
        var hasVouchers = await _context.ChiTietButToans.AnyAsync(c => c.PhongBanId == id);
        if (hasVouchers)
        {
            throw new InvalidOperationException("Không thể xóa phòng ban đã phát sinh dữ liệu hạch toán trên Sổ Cái (Vui lòng chuyển trạng thái sang Ngừng hoạt động).");
        }

        _context.PhongBans.Remove(dept);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã xóa phòng ban ID {Id}", id);
        return true;
    }

    public async Task<List<PhongBan>> GetDepartmentTreeAsync(long? branchId = null)
    {
        var all = await GetDepartmentsAsync(branchId);
        var lookup = all.ToLookup(p => p.PhongBanChaId);

        foreach (var item in all)
        {
            item.PhongBanCons = lookup[item.Id].ToList();
        }

        // Trả về danh sách các node gốc (Root nodes)
        return all.Where(p => !p.PhongBanChaId.HasValue || p.PhongBanChaId.Value == 0).ToList();
    }

    public async Task AssignEmployeeDepartmentAsync(long employeeId, long departmentId)
    {
        var employee = await _context.NhanViens.FindAsync(employeeId);
        if (employee == null) throw new KeyNotFoundException("Không tìm thấy nhân viên.");

        var dept = await _context.PhongBans.FindAsync(departmentId);
        if (dept == null) throw new KeyNotFoundException("Không tìm thấy phòng ban.");

        employee.PhongBanId = dept.Id;
        employee.PhongBan = dept.TenPhongBan;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã gán nhân viên {Nv} vào phòng ban {Dept}", employee.HoTen, dept.TenPhongBan);
    }
}

