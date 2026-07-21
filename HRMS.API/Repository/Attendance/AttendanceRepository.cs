using HRMS.API.Data;
using Microsoft.EntityFrameworkCore;
using AttendanceEntity = HRMS.API.Entities.Attendance;

namespace HRMS.API.Repository.Attendance
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly DefaultContext _context;

        public AttendanceRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<AttendanceEntity?> GetTodayAttendanceAsync(
            long employeeId,
            DateOnly attendanceDate)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.AttendanceDate == attendanceDate);
        }

        public async Task<AttendanceEntity?> GetByIdAsync(long id)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(AttendanceEntity attendance)
        {
            await _context.Attendances.AddAsync(attendance);
        }

        public void Update(AttendanceEntity attendance)
        {
            _context.Attendances.Update(attendance);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}