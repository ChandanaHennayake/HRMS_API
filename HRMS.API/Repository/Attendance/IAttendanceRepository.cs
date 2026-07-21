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
    }
}