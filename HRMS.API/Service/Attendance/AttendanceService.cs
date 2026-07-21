using ABANS_BAN.DTOs.Attendance;
using HRMS.API.Helpers;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repository.Attendance;

using AttendanceEntity = HRMS.API.Entities.Attendance;

namespace HRMS.API.Service.Attendance
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IBranchRepository _branchRepository;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IEmployeeRepository employeeRepository,
            IBranchRepository branchRepository)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
        }


        // =========================================================
        // CHECK IN
        // =========================================================

        public async Task<AttendanceResponseDto> CheckInAsync(
            long employeeId,
            CheckInRequestDto request)
        {
            var employee = await _employeeRepository
                .GetEntityByIdAsync(employeeId);

            if (employee == null)
            {
                throw new Exception("Employee not found.");
            }

            if (!employee.IsActive)
            {
                throw new Exception("Employee is inactive.");
            }

            var branch = await _branchRepository
                .GetByIdAsync(Convert.ToInt32(employee.BranchId));

            if (branch == null)
            {
                throw new Exception("Employee branch not found.");
            }

            // -----------------------------------------------------
            // GEOFENCE
            // -----------------------------------------------------

            if (branch.Latitude == null ||
                branch.Longitude == null)
            {
                throw new Exception(
                    "Branch location is not configured.");
            }

            var distance =
                GeoLocationHelper.CalculateDistance(
                    (double)request.Latitude,
                    (double)request.Longitude,
                    (double)branch.Latitude.Value,
                    (double)branch.Longitude.Value);

            if (distance > branch.GeofenceRadiusMeters)
            {
                throw new Exception(
                    $"You are outside the allowed attendance area. " +
                    $"Distance: {distance:F0} meters.");
            }


            // -----------------------------------------------------
            // CURRENT DATE / TIME
            // -----------------------------------------------------

            var currentTime = DateTime.Now;

            var today =
                DateOnly.FromDateTime(currentTime);


            // -----------------------------------------------------
            // CHECK EXISTING ATTENDANCE
            // -----------------------------------------------------

            var existingAttendance =
                await _attendanceRepository
                    .GetTodayAttendanceAsync(
                        employeeId,
                        today);

            if (existingAttendance != null)
            {
                throw new Exception(
                    "You have already checked in today.");
            }


            // -----------------------------------------------------
            // CALCULATE LATE
            // Normal Start = 08:00 AM
            // -----------------------------------------------------

            var normalStartTime =
                new TimeOnly(8, 0);

            var actualCheckIn =
                TimeOnly.FromDateTime(currentTime);

            var isLate = false;

            var lateMinutes = 0;

            if (actualCheckIn > normalStartTime)
            {
                isLate = true;

                lateMinutes =
                    (int)(
                        actualCheckIn.ToTimeSpan() -
                        normalStartTime.ToTimeSpan()
                    ).TotalMinutes;
            }


            // -----------------------------------------------------
            // CREATE ATTENDANCE
            // -----------------------------------------------------

            var attendance = new AttendanceEntity
            {
                EmployeeId = employeeId,

                AttendanceDate = today,

                CheckInTime = currentTime,

                CheckInLatitude =
                    request.Latitude,

                CheckInLongitude =
                    request.Longitude,

                CheckInDistanceMeters =
                    (decimal)distance,

                WorkedMinutes = 0,

                IsLate = isLate,

                LateMinutes = lateMinutes,

                IsEarlyDeparture = false,

                EarlyDepartureMinutes = 0,

                OTMinutes = 0,

                Status = 1,

                CreatedAt = currentTime
            };


            await _attendanceRepository
                .AddAsync(attendance);

            await _attendanceRepository
                .SaveChangesAsync();


            return MapToResponse(attendance);
        }


        // =========================================================
        // CHECK OUT
        // =========================================================

        public async Task<AttendanceResponseDto> CheckOutAsync(
            long employeeId,
            CheckOutRequestDto request)
        {
            var employee = await _employeeRepository
                .GetEntityByIdAsync(employeeId);

            if (employee == null)
            {
                throw new Exception("Employee not found.");
            }

            if (!employee.IsActive)
            {
                throw new Exception("Employee is inactive.");
            }


            // -----------------------------------------------------
            // GET EMPLOYEE BRANCH
            // -----------------------------------------------------

            var branch = await _branchRepository
                .GetByIdAsync(Convert.ToInt32(employee.BranchId));

            if (branch == null)
            {
                throw new Exception(
                    "Employee branch not found.");
            }


            // -----------------------------------------------------
            // CHECK BRANCH LOCATION
            // -----------------------------------------------------

            if (branch.Latitude == null ||
                branch.Longitude == null)
            {
                throw new Exception(
                    "Branch location is not configured.");
            }


            // -----------------------------------------------------
            // CALCULATE GEOFENCE DISTANCE
            // -----------------------------------------------------

            var distance =
                GeoLocationHelper.CalculateDistance(
                    (double)request.Latitude,
                    (double)request.Longitude,
                    (double)branch.Latitude.Value,
                    (double)branch.Longitude.Value);


            if (distance > branch.GeofenceRadiusMeters)
            {
                throw new Exception(
                    $"You are outside the allowed attendance area. " +
                    $"Distance: {distance:F0} meters.");
            }


            var currentTime = DateTime.Now;

            var today =
                DateOnly.FromDateTime(currentTime);


            // -----------------------------------------------------
            // GET TODAY ATTENDANCE
            // -----------------------------------------------------

            var attendance =
                await _attendanceRepository
                    .GetTodayAttendanceAsync(
                        employeeId,
                        today);


            if (attendance == null)
            {
                throw new Exception(
                    "You have not checked in today.");
            }


            if (attendance.CheckInTime == null)
            {
                throw new Exception(
                    "Check-in time not found.");
            }


            if (attendance.CheckOutTime != null)
            {
                throw new Exception(
                    "You have already checked out today.");
            }


            // -----------------------------------------------------
            // CHECK OUT INFORMATION
            // -----------------------------------------------------

            attendance.CheckOutTime =
                currentTime;

            attendance.CheckOutLatitude =
                request.Latitude;

            attendance.CheckOutLongitude =
                request.Longitude;

            attendance.CheckOutDistanceMeters =
                (decimal)distance;


            // -----------------------------------------------------
            // WORKED MINUTES
            // -----------------------------------------------------

            var workedDuration =
                currentTime -
                attendance.CheckInTime.Value;

            attendance.WorkedMinutes =
                (int)workedDuration.TotalMinutes;


            // -----------------------------------------------------
            // EARLY DEPARTURE / OT
            // Normal End = 05:00 PM
            // -----------------------------------------------------

            var normalEndTime =
                new TimeOnly(17, 0);

            var actualCheckOut =
                TimeOnly.FromDateTime(currentTime);


            if (actualCheckOut < normalEndTime)
            {
                attendance.IsEarlyDeparture =
                    true;

                attendance.EarlyDepartureMinutes =
                    (int)(
                        normalEndTime.ToTimeSpan() -
                        actualCheckOut.ToTimeSpan()
                    ).TotalMinutes;

                attendance.OTMinutes = 0;
            }
            else
            {
                attendance.IsEarlyDeparture =
                    false;

                attendance.EarlyDepartureMinutes =
                    0;

                attendance.OTMinutes =
                    (int)(
                        actualCheckOut.ToTimeSpan() -
                        normalEndTime.ToTimeSpan()
                    ).TotalMinutes;
            }


            attendance.UpdatedAt =
                currentTime;


            _attendanceRepository
                .Update(attendance);

            await _attendanceRepository
                .SaveChangesAsync();


            return MapToResponse(attendance);
        }


        // =========================================================
        // GET TODAY ATTENDANCE
        // =========================================================

        public async Task<AttendanceResponseDto?>
            GetTodayAttendanceAsync(long employeeId)
        {
            var today =
                DateOnly.FromDateTime(
                    DateTime.Now);


            var attendance =
                await _attendanceRepository
                    .GetTodayAttendanceAsync(
                        employeeId,
                        today);


            if (attendance == null)
            {
                return null;
            }


            return MapToResponse(attendance);
        }


        // =========================================================
        // MAP ENTITY -> DTO
        // =========================================================

        private static AttendanceResponseDto MapToResponse(
            AttendanceEntity attendance)
        {
            return new AttendanceResponseDto
            {
                Id =
                    attendance.Id,

                EmployeeId =
                    attendance.EmployeeId,

                AttendanceDate =
                    attendance.AttendanceDate,

                CheckInTime =
                    attendance.CheckInTime,

                CheckOutTime =
                    attendance.CheckOutTime,

                CheckInDistanceMeters =
                    attendance.CheckInDistanceMeters,

                CheckOutDistanceMeters =
                    attendance.CheckOutDistanceMeters,

                WorkedMinutes =
                    attendance.WorkedMinutes,

                IsLate =
                    attendance.IsLate,

                LateMinutes =
                    attendance.LateMinutes,

                IsEarlyDeparture =
                    attendance.IsEarlyDeparture,

                EarlyDepartureMinutes =
                    attendance.EarlyDepartureMinutes,

                OTMinutes =
                    attendance.OTMinutes,

                Status =
                    attendance.Status
            };
        }

        public async Task FinalizeDailyAttendanceAsync(
    DateOnly date)
{
    // Sunday - no work
    if (date.DayOfWeek == DayOfWeek.Sunday)
    {
        return;
    }

    var employees =
        await _employeeRepository.GetAllActiveEntitiesAsync();

    foreach (var employee in employees)
    {
        var attendance =
            await _attendanceRepository
                .GetTodayAttendanceAsync(
                    employee.Id,
                    date);

        // Already checked in / attendance exists
        if (attendance != null)
        {
            continue;
        }

        var hasApprovedLeave =
            await _leaveRepository
                .HasApprovedLeaveAsync(
                    employee.Id,
                    date);

        var newAttendance = new AttendanceEntity
        {
            EmployeeId = employee.Id,

            AttendanceDate = date,

            CheckInTime = null,

            CheckOutTime = null,

            WorkedMinutes = 0,

            LateMinutes = 0,

            EarlyDepartureMinutes = 0,

            OTMinutes = 0,

            IsLate = false,

            IsEarlyDeparture = false,

            Status = hasApprovedLeave
                ? (short)AttendanceStatus.OnLeave
                : (short)AttendanceStatus.Absent,

            CreatedAt = DateTime.Now
        };

        await _attendanceRepository
            .AddAsync(newAttendance);
    }

    await _attendanceRepository
        .SaveChangesAsync();
}


    }
}