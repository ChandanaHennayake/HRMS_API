using HRMS.API.Entities;

namespace HRMS.API.Repository.Leave
{
    public interface ILeaveTypeRepository
    {
        Task<IEnumerable<LeaveType>> GetAllAsync();

        Task<LeaveType?> GetByIdAsync(long id);

        Task<IEnumerable<LeaveType>> GetActiveAsync();

        Task<LeaveType> CreateAsync(LeaveType leaveType);

        Task UpdateAsync(LeaveType leaveType);
    }
}