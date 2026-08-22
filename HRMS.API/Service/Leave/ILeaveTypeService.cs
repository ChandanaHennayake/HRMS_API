using HRMS.API.Entities;

namespace HRMS.API.Service.Leave
{
    public interface ILeaveTypeService
    {
        Task<IEnumerable<LeaveType>> GetAllAsync();

        Task<IEnumerable<LeaveType>> GetActiveAsync();

        Task<LeaveType?> GetByIdAsync(long id);

        Task<LeaveType> CreateAsync(LeaveType leaveType);

        Task UpdateAsync(LeaveType leaveType);
    }
}