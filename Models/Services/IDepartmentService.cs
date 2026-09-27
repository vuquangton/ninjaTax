using ninjaTax.Models.Entities;
using ninjaTax.Models.ViewModels;

namespace ninjaTax.Models.Services;

public interface IDepartmentService
{
    Task<List<PhongBan>> GetDepartmentsAsync(long? branchId = null);
    Task<PhongBan?> GetDepartmentByIdAsync(long id);
    Task<PhongBan> SaveDepartmentAsync(PhongBan department);
    Task<bool> DeleteDepartmentAsync(long id);
    Task<List<PhongBan>> GetDepartmentTreeAsync(long? branchId = null);
    Task<List<DepartmentListItemViewModel>> GetFlattenedHierarchyAsync(long? branchId = null);
    Task<List<PhongBan>> GetAvailableParentDepartmentsAsync(long branchId, long? excludeDeptId = null);
    Task AssignEmployeeDepartmentAsync(long employeeId, long departmentId);
}
