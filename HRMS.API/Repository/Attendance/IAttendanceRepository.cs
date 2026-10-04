using ABANS_BAN.DTOs.Attendance;
using AttendanceEntity = HRMS.API.Entities.Attendance;

namespace HRMS.API.Repository.Attendance
{
    public interface IAttendanceRepository
    {
        Task<AttendanceEntity?> GetTodayAttendanceAsync(
            long employeeId,
            DateOnly attendanceDate);

        Task<AttendanceEntity?> GetByIdAsync(long id);

        Task AddAsync(AttendanceEntity attendance);

        void Update(AttendanceEntity attendance);

        Task<int> SaveChangesAsync();

        Task<List<AttendanceHistoryResponseDto>>
            GetAttendanceHistoryAsync(
                long? employeeId,
                DateOnly? fromDate,
                DateOnly? toDate);
    }
}