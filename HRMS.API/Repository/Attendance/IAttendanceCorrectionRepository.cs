using HRMS.API.Entities;

namespace HRMS.API.Repository.Attendance
{
    public interface IAttendanceCorrectionRepository
    {
        Task<AttendanceCorrection> AddAsync(AttendanceCorrection correction);
        Task<AttendanceCorrection?> GetByIdAsync(long id);
        Task<IEnumerable<AttendanceCorrection>> GetByEmployeeIdAsync(long employeeId);
        Task<IEnumerable<AttendanceCorrection>> GetPendingAsync(int? branchId);
        Task<bool> HasPendingRequestAsync(long employeeId, DateOnly attendanceDate);
        void Update(AttendanceCorrection correction);
        Task<int> SaveChangesAsync();
    }
}
