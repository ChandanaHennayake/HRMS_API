using ABANS_BAN.DTOs.Attendance;
using HRMS.API.Enums;
using HRMS.API.Helpers;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repository.Attendance;
using HRMS.API.Repository.Leave;
using HRMS.API.Service.Notification;
using AttendanceEntity = HRMS.API.Entities.Attendance;

namespace HRMS.API.Service.Attendance
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly HRMS.API.Repository.Holiday.IHolidayRepository _holidayRepository;
        private readonly INotificationService _notificationService;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IEmployeeRepository employeeRepository,
            IBranchRepository branchRepository,
            ILeaveRepository leaveRepository,
            HRMS.API.Repository.Holiday.IHolidayRepository holidayRepository,
            INotificationService notificationService)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _leaveRepository = leaveRepository;
            _holidayRepository = holidayRepository;
            _notificationService = notificationService;
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
                    if (attendance.CheckOutTime == null && attendance.CheckInTime != null)
                    {
                        await _notificationService.SendNotificationAsync(new HRMS.API.DTOs.Notification.CreateNotificationDto
                        {
                            EmployeeId = employee.Id,
                            Title = "Missing Check-Out Alert",
                            Message = $"You checked in on {date:yyyy-MM-dd} at {attendance.CheckInTime:HH:mm} without checking out. Please submit an attendance correction request.",
                            Type = "Warning",
                            ReferenceType = "Attendance",
                            ReferenceId = attendance.Id
                        });
                    }
                    continue;
                }

                var isHoliday =
                    await _holidayRepository
                        .IsHolidayAsync(
                            employee.CompanyId,
                            date);

                var hasApprovedLeave =
                    await _leaveRepository
                        .HasApprovedLeaveAsync(
                            employee.Id,
                            date);

                short status;
                if (isHoliday)
                {
                    status = (short)AttendanceStatus.Holiday;
                }
                else if (hasApprovedLeave)
                {
                    status = (short)AttendanceStatus.OnLeave;
                }
                else
                {
                    status = (short)AttendanceStatus.Absent;
                }

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

                    Status = status,

                    CreatedAt = DateTime.Now
                };

                await _attendanceRepository
                    .AddAsync(newAttendance);
            }

            await _attendanceRepository
                .SaveChangesAsync();
        }

        // =========================================================
        // GET ATTENDANCE HISTORY
        // =========================================================

        public async Task<List<AttendanceHistoryResponseDto>>
            GetAttendanceHistoryAsync(
                long loggedInEmployeeId,
                bool isAdmin,
                long? employeeId,
                DateOnly? fromDate,
                DateOnly? toDate)
        {
            // -----------------------------------------------------
            // VALIDATE DATE RANGE
            // -----------------------------------------------------

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value > toDate.Value)
            {
                throw new Exception(
                    "From date cannot be greater than To date.");
            }


            // -----------------------------------------------------
            // EMPLOYEE
            //
            // Employee is ONLY allowed to see own attendance.
            // Ignore employeeId supplied by client.
            // -----------------------------------------------------

            if (!isAdmin)
            {
                employeeId = loggedInEmployeeId;
            }


            // -----------------------------------------------------
            // DEFAULT DATE
            //
            // If no dates are supplied, repository/SP will use
            // CURRENT_DATE.
            // -----------------------------------------------------

            var history =
                await _attendanceRepository
                    .GetAttendanceHistoryAsync(
                        employeeId,
                        fromDate,
                        toDate);


            return history;
        }

        // =========================================================
        // GET BRANCH / COMPANY DAILY ATTENDANCE ROSTER
        // =========================================================

        public async Task<List<HRMS.API.DTOs.Attendance.BranchDailyAttendanceDto>>
            GetBranchDailyAttendanceAsync(
                int? branchId,
                DateOnly? date)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            // Fetch active employees
            var allEmployees = await _employeeRepository.GetAllAsync();
            var filteredEmployees = allEmployees
                .Where(e => e.IsActive && (!branchId.HasValue || branchId.Value <= 0 || e.BranchId == branchId.Value))
                .ToList();

            var branches = (await _branchRepository.GetAllAsync())
                .ToDictionary(b => b.branchid, b => b.branchname);

            var result = new List<HRMS.API.DTOs.Attendance.BranchDailyAttendanceDto>();

            foreach (var emp in filteredEmployees)
            {
                var att = await _attendanceRepository.GetTodayAttendanceAsync(emp.Id, targetDate);

                int status;
                string statusName;

                if (att != null)
                {
                    status = att.Status;
                    statusName = status switch
                    {
                        1 => "Present",
                        2 => "On Leave",
                        3 => "Weekly Off",
                        4 => "Holiday",
                        _ => "Absent"
                    };

                    result.Add(new HRMS.API.DTOs.Attendance.BranchDailyAttendanceDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        FullName = emp.FullName,
                        DepartmentName = emp.DepartmentName,
                        DesignationName = emp.DesignationName,
                        BranchId = emp.BranchId,
                        BranchName = branches.TryGetValue((int)emp.BranchId, out var bName) ? bName : "N/A",
                        AttendanceDate = targetDate,
                        CheckInTime = att.CheckInTime,
                        CheckOutTime = att.CheckOutTime,
                        WorkedMinutes = att.WorkedMinutes,
                        Status = status,
                        StatusName = statusName,
                        IsLate = att.IsLate,
                        LateMinutes = att.LateMinutes,
                        IsEarlyDeparture = att.IsEarlyDeparture,
                        EarlyDepartureMinutes = att.EarlyDepartureMinutes
                    });
                }
                else
                {
                    var isHoliday = await _holidayRepository.IsHolidayAsync(emp.CompanyId, targetDate);
                    var hasApprovedLeave = await _leaveRepository.HasApprovedLeaveAsync(emp.Id, targetDate);

                    if (isHoliday)
                    {
                        status = 4;
                        statusName = "Holiday";
                    }
                    else if (hasApprovedLeave)
                    {
                        status = 2;
                        statusName = "On Leave";
                    }
                    else if (targetDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        status = 3;
                        statusName = "Weekly Off";
                    }
                    else if (targetDate < DateOnly.FromDateTime(DateTime.Today))
                    {
                        status = 0;
                        statusName = "Absent";
                    }
                    else
                    {
                        status = 0;
                        statusName = "Not Reported";
                    }

                    result.Add(new HRMS.API.DTOs.Attendance.BranchDailyAttendanceDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        FullName = emp.FullName,
                        DepartmentName = emp.DepartmentName,
                        DesignationName = emp.DesignationName,
                        BranchId = emp.BranchId,
                        BranchName = branches.TryGetValue((int)emp.BranchId, out var bName) ? bName : "N/A",
                        AttendanceDate = targetDate,
                        CheckInTime = null,
                        CheckOutTime = null,
                        WorkedMinutes = 0,
                        Status = status,
                        StatusName = statusName,
                        IsLate = false,
                        LateMinutes = 0,
                        IsEarlyDeparture = false,
                        EarlyDepartureMinutes = 0
                    });
                }
            }

            return result.OrderBy(r => r.FullName).ToList();
        }
    }
}