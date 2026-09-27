using Microsoft.EntityFrameworkCore;
using ninjaTax.Data;
using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

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
        department.MaPhongBan = department.MaPhongBan?.Trim() ?? string.Empty;
        department.TenPhongBan = department.TenPhongBan?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(department.MaPhongBan))
        {
            throw new ArgumentException("Mã phòng ban không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(department.TenPhongBan))
        {
            throw new ArgumentException("Tên phòng ban không được để trống.");
        }

        // Tự động gán mã tài khoản chi phí mặc định nếu chưa chỉ định
        if (string.IsNullOrWhiteSpace(department.MaTaiKhoanChiPhi))
        {
            department.MaTaiKhoanChiPhi = department.LoaiPhongBan.LayMaTaiKhoanChiPhiMacDinh();
        }
        else
        {
            department.MaTaiKhoanChiPhi = department.MaTaiKhoanChiPhi.Trim();
        }

        // Bất biến TT99: Tuyệt đối cấm sử dụng TK 911
        if (department.MaTaiKhoanChiPhi == "911" || department.MaTaiKhoanChiPhi.StartsWith("911"))
        {
            throw new ArgumentException("Nghiêm cấm tuyệt đối sử dụng Tài khoản 911 theo chuẩn TT 99/2025/TT-BTC.");
        }

        // Kiểm tra tài khoản chi phí có tồn tại trong hệ thống TT99
        var accountExists = await _context.TaiKhoans.AnyAsync(t => t.MaTaiKhoan == department.MaTaiKhoanChiPhi);
        if (!accountExists)
        {
            throw new ArgumentException($"Tài khoản chi phí '{department.MaTaiKhoanChiPhi}' không tồn tại trong Hệ thống Danh mục Tài khoản TT99.");
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

    public async Task<List<DepartmentListItemViewModel>> GetFlattenedHierarchyAsync(long? branchId = null)
    {
        var departments = await GetDepartmentsAsync(branchId);
        var result = new List<DepartmentListItemViewModel>();
        var lookup = departments.ToLookup(d => d.PhongBanChaId);
        var roots = departments.Where(d => !d.PhongBanChaId.HasValue || d.PhongBanChaId.Value == 0).ToList();

        void Traverse(PhongBan node, int level)
        {
            result.Add(new DepartmentListItemViewModel
            {
                Id = node.Id,
                ChiNhanhId = node.ChiNhanhId,
                TenChiNhanh = node.ChiNhanh?.TenChiNhanh ?? "Chi nhánh mặc định",
                PhongBanChaId = node.PhongBanChaId,
                TenPhongBanCha = node.PhongBanCha?.TenPhongBan,
                MaPhongBan = node.MaPhongBan,
                TenPhongBan = node.TenPhongBan,
                TenTiengAnh = node.TenTiengAnh,
                LoaiPhongBan = node.LoaiPhongBan,
                MaTaiKhoanChiPhi = node.MaTaiKhoanChiPhi,
                TenTruongPhong = node.TruongPhong?.HoTen,
                LaTrungTamLoiNhuan = node.LaTrungTamLoiNhuan,
                DangHoatDong = node.DangHoatDong,
                SoNhanVien = node.NhanViens?.Count ?? 0,
                Level = level
            });

            foreach (var child in lookup[node.Id])
            {
                Traverse(child, level + 1);
            }
        }

        foreach (var root in roots)
        {
            Traverse(root, 0);
        }

        // Bổ sung các node mồ côi nếu có (orphan nodes)
        var processedIds = result.Select(r => r.Id).ToHashSet();
        foreach (var orphan in departments.Where(d => !processedIds.Contains(d.Id)))
        {
            result.Add(new DepartmentListItemViewModel
            {
                Id = orphan.Id,
                ChiNhanhId = orphan.ChiNhanhId,
                TenChiNhanh = orphan.ChiNhanh?.TenChiNhanh ?? "Chi nhánh mặc định",
                PhongBanChaId = orphan.PhongBanChaId,
                TenPhongBanCha = orphan.PhongBanCha?.TenPhongBan,
                MaPhongBan = orphan.MaPhongBan,
                TenPhongBan = orphan.TenPhongBan,
                TenTiengAnh = orphan.TenTiengAnh,
                LoaiPhongBan = orphan.LoaiPhongBan,
                MaTaiKhoanChiPhi = orphan.MaTaiKhoanChiPhi,
                TenTruongPhong = orphan.TruongPhong?.HoTen,
                LaTrungTamLoiNhuan = orphan.LaTrungTamLoiNhuan,
                DangHoatDong = orphan.DangHoatDong,
                SoNhanVien = orphan.NhanViens?.Count ?? 0,
                Level = 0
            });
        }

        return result;
    }

    public async Task<List<PhongBan>> GetAvailableParentDepartmentsAsync(long branchId, long? excludeDeptId = null)
    {
        var allDepts = await _context.PhongBans
            .AsNoTracking()
            .Where(p => p.ChiNhanhId == branchId)
            .OrderBy(p => p.MaPhongBan)
            .ToListAsync();

        if (!excludeDeptId.HasValue || excludeDeptId.Value <= 0)
        {
            return allDepts;
        }

        var excludedIds = new HashSet<long> { excludeDeptId.Value };
        CollectDescendantIds(excludeDeptId.Value, allDepts, excludedIds);

        return allDepts.Where(p => !excludedIds.Contains(p.Id)).ToList();
    }

    private static void CollectDescendantIds(long parentId, List<PhongBan> allDepts, HashSet<long> result)
    {
        var directChildren = allDepts.Where(p => p.PhongBanChaId == parentId).Select(p => p.Id).ToList();
        foreach (var childId in directChildren)
        {
            if (result.Add(childId))
            {
                CollectDescendantIds(childId, allDepts, result);
            }
        }
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

