using HRMS.API.Data;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Attendance
{
    public class AttendanceCorrectionRepository : IAttendanceCorrectionRepository
    {
        private readonly DefaultContext _context;

        public AttendanceCorrectionRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<AttendanceCorrection> AddAsync(AttendanceCorrection correction)
        {
            await _context.AttendanceCorrections.AddAsync(correction);
            await _context.SaveChangesAsync();
            return correction;
        }

        public async Task<AttendanceCorrection?> GetByIdAsync(long id)
        {
            return await _context.AttendanceCorrections
                .Include(c => c.Employee)
                .Include(c => c.Attendance)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<AttendanceCorrection>> GetByEmployeeIdAsync(long employeeId)
        {
            return await _context.AttendanceCorrections
                .Include(c => c.Employee)
                .Include(c => c.Attendance)
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.AttendanceDate)
                .ThenByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceCorrection>> GetPendingAsync(int? branchId)
        {
            var query = _context.AttendanceCorrections
                .Include(c => c.Employee)
                .Include(c => c.Attendance)
                .Where(c => c.Status == 0);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(c => c.Employee.BranchId == branchId.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasPendingRequestAsync(long employeeId, DateOnly attendanceDate)
        {
            return await _context.AttendanceCorrections
                .AnyAsync(c => c.EmployeeId == employeeId && c.AttendanceDate == attendanceDate && c.Status == 0);
        }

        public void Update(AttendanceCorrection correction)
        {
            _context.AttendanceCorrections.Update(correction);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
