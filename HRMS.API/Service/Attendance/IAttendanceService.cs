using ABANS_BAN.DTOs.Attendance;

namespace HRMS.API.Service.Attendance
{
    public interface IAttendanceService
    {
        Task<AttendanceResponseDto> CheckInAsync(
            long employeeId,
            CheckInRequestDto request);

        Task<AttendanceResponseDto> CheckOutAsync(
            long employeeId,
            CheckOutRequestDto request);

        Task<AttendanceResponseDto?> GetTodayAttendanceAsync(
            long employeeId);

        Task FinalizeDailyAttendanceAsync(DateOnly date);
    }
}