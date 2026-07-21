using HRMS.API.DTOs.Leave;

namespace HRMS.API.Service.Leave
{
    public interface ILeaveService
    {
        Task<LeaveResponseDto> ApplyLeaveAsync(
            long employeeId,
            ApplyLeaveDto request);

        Task<IEnumerable<LeaveResponseDto>>
            GetEmployeeLeavesAsync(long employeeId);

        Task<IEnumerable<LeaveResponseDto>>
            GetPendingLeavesAsync();

        Task<LeaveResponseDto> ApproveLeaveAsync(
            long leaveRequestId,
            long approvedBy,
            LeaveActionDto request);

        Task<LeaveResponseDto> RejectLeaveAsync(
            long leaveRequestId,
            long rejectedBy,
            LeaveActionDto request);

        Task<LeaveResponseDto> CancelLeaveAsync(
            long leaveRequestId,
            long employeeId);
    }
}