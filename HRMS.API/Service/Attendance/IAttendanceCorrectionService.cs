using HRMS.API.DTOs.Attendance;

namespace HRMS.API.Service.Attendance
{
    public interface IAttendanceCorrectionService
    {
        Task<AttendanceCorrectionDto> SubmitCorrectionAsync(long employeeId, CreateAttendanceCorrectionDto request);
        Task<IEnumerable<AttendanceCorrectionDto>> GetEmployeeCorrectionsAsync(long employeeId);
        Task<IEnumerable<AttendanceCorrectionDto>> GetPendingCorrectionsAsync(int? branchId);
        Task<AttendanceCorrectionDto> ReviewCorrectionAsync(long id, long reviewerUserId, ReviewAttendanceCorrectionDto review);
        Task<bool> CancelCorrectionAsync(long id, long employeeId);
    }
}
