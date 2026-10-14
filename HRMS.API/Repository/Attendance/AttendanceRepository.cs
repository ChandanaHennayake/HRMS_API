using ABANS_BAN.DTOs.Attendance;
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

        public async Task<List<AttendanceHistoryResponseDto>>
         GetAttendanceHistoryAsync(
             long? employeeId,
             DateOnly? fromDate,
             DateOnly? toDate)
        {
            var employeeIdParameter =
                employeeId.HasValue
                    ? employeeId.Value
                    : (object)DBNull.Value;

            var fromDateParameter =
                fromDate.HasValue
                    ? fromDate.Value
                    : (object)DBNull.Value;

            var toDateParameter =
                toDate.HasValue
                    ? toDate.Value
                    : (object)DBNull.Value;


            return await _context.Database
                .SqlQueryRaw<AttendanceHistoryResponseDto>(
                    """
                    SELECT
                        employee_id AS "EmployeeId",
                        attendance_date AS "AttendanceDate",

                        attendance_status AS "AttendanceStatus",
                        status_name AS "StatusName",

                        check_in_time AS "CheckInTime",
                        check_out_time AS "CheckOutTime",

                        check_in_latitude AS "CheckInLatitude",
                        check_in_longitude AS "CheckInLongitude",
                        check_in_distance_meters AS "CheckInDistanceMeters",

                        check_out_latitude AS "CheckOutLatitude",
                        check_out_longitude AS "CheckOutLongitude",
                        check_out_distance_meters AS "CheckOutDistanceMeters",

                        worked_minutes AS "WorkedMinutes",

                        is_late AS "IsLate",
                        late_minutes AS "LateMinutes",

                        is_early_departure AS "IsEarlyDeparture",
                        early_departure_minutes AS "EarlyDepartureMinutes",

                        ot_minutes AS "OTMinutes",

                        leave_type_id AS "LeaveTypeId",
                        leave_type_name AS "LeaveTypeName",

                        holiday_id AS "HolidayId",
                        holiday_name AS "HolidayName"

                    FROM public.get_attendance_history(
                        {0},
                        {1},
                        {2}
                    )
                    """,
                    employeeIdParameter,
                    fromDateParameter,
                    toDateParameter)
                .ToListAsync();
        }

       
    }
}