using HRMS.API.Entities;

namespace HRMS.API.Repository.Leave
{
    public interface ILeaveBalanceRepository
    {
        Task<IEnumerable<EmployeeLeaveBalance>> GetBalancesAsync(long employeeId, int year);
        Task<EmployeeLeaveBalance?> GetBalanceAsync(long employeeId, long leaveTypeId, int year);
        Task<EmployeeLeaveBalance> CreateAsync(EmployeeLeaveBalance balance);
        Task UpdateAsync(EmployeeLeaveBalance balance);
        Task EnsureBalancesAsync(long employeeId, int year, long companyId);
    }
}
