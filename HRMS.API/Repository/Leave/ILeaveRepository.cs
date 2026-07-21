using HRMS.API.Entities;

namespace HRMS.API.Repository.Leave
{
    public interface ILeaveRepository
    {
        // Create leave request
        Task<LeaveRequest> CreateAsync(
            LeaveRequest leaveRequest);

        // Get leave request by ID
        Task<LeaveRequest?> GetByIdAsync(
            long id);

        // Get employee leave history
        Task<IEnumerable<LeaveRequest>>
            GetEmployeeLeavesAsync(
                long employeeId);

        // Get all pending leave requests
        Task<IEnumerable<LeaveRequest>>
            GetPendingLeavesAsync();

        // Check whether employee already has
        // pending/approved leave in this period
        Task<bool> HasOverlappingLeaveAsync(
            long employeeId,
            DateOnly fromDate,
            DateOnly toDate);

        // Check approved leave for a particular date
        // Used by automatic attendance finalization
        Task<bool> HasApprovedLeaveAsync(
            long employeeId,
            DateOnly date);

        // Update leave request
        // Approve / Reject / Cancel
        Task UpdateAsync(
            LeaveRequest leaveRequest);
    }
}