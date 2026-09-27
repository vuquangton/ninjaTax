using ninjaTax.Models.Entities;

namespace ninjaTax.Models.Services;

public interface IDepartmentService
{
    Task<List<PhongBan>> GetDepartmentsAsync(long? branchId = null);
    Task<PhongBan?> GetDepartmentByIdAsync(long id);
    Task<PhongBan> SaveDepartmentAsync(PhongBan department);
    Task<bool> DeleteDepartmentAsync(long id);
    Task<List<PhongBan>> GetDepartmentTreeAsync(long? branchId = null);
    Task AssignEmployeeDepartmentAsync(long employeeId, long departmentId);
}
