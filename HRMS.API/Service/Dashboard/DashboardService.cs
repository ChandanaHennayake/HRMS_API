using HRMS.API.Data;
using HRMS.API.DTOs.Dashboard;
using HRMS.API.Enums;
using HRMS.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Service.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly DefaultContext _context;

        public DashboardService(DefaultContext context)
        {
            _context = context;
        }

        public async Task<AdminSummaryDto> GetAdminSummaryAsync(long? companyId = null)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            // Active, non-deleted employees
            var empQuery = _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive);

            if (companyId.HasValue && companyId.Value > 0)
            {
                empQuery = empQuery.Where(e => e.CompanyId == companyId.Value);
            }

            var employeeIds = await empQuery.Select(e => e.Id).ToListAsync();
            var totalEmployees = employeeIds.Count;

            if (totalEmployees == 0)
            {
                return new AdminSummaryDto
                {
                    TotalEmployees = 0,
                    PresentToday = 0,
                    OnLeaveToday = 0,
                    AbsentToday = 0,
                    PendingLeaves = 0,
                    LateEmployees = 0,
                    MissingCheckOut = 0,
                    PresentPercentage = 0,
                    LatePercentage = 0,
                    LeavePercentage = 0,
                    AbsentPercentage = 0
                };
            }

            // Attendances recorded today
            var todayAttendances = await _context.Attendances
                .Where(a => a.AttendanceDate == today && employeeIds.Contains(a.EmployeeId))
                .ToListAsync();

            var presentEmployeeIds = todayAttendances
                .Where(a => a.CheckInTime != null)
                .Select(a => a.EmployeeId)
                .Distinct()
                .ToHashSet();

            var presentToday = presentEmployeeIds.Count;

            var lateEmployees = todayAttendances
                .Where(a => a.IsLate || a.LateMinutes > 0)
                .Select(a => a.EmployeeId)
                .Distinct()
                .Count();

            // Missing check-out: Checked in but check-out is null after 17:00 (5 PM)
            var isPastOfficeHours = now.TimeOfDay >= new TimeSpan(17, 0, 0);
            var missingCheckOut = isPastOfficeHours
                ? todayAttendances.Count(a => a.CheckInTime != null && a.CheckOutTime == null)
                : 0;

            // Leaves covering today
            var onLeaveCount = await _context.LeaveRequests
                .Where(l => l.Status == (short)LeaveRequestStatus.Approved &&
                            l.FromDate <= today && l.ToDate >= today &&
                            employeeIds.Contains(l.EmployeeId))
                .Select(l => l.EmployeeId)
                .Distinct()
                .CountAsync();

            // Absent today
            var absentToday = Math.Max(0, totalEmployees - (presentToday + onLeaveCount));

            // Total pending leaves awaiting review
            var pendingLeaves = await _context.LeaveRequests
                .Where(l => l.Status == (short)LeaveRequestStatus.Pending &&
                            employeeIds.Contains(l.EmployeeId))
                .CountAsync();

            // Total pending attendance corrections awaiting review
            var pendingCorrections = await _context.AttendanceCorrections
                .Where(c => c.Status == 0 &&
                            employeeIds.Contains(c.EmployeeId))
                .CountAsync();

            // Percentages
            var presentPct = Math.Round((double)presentToday / totalEmployees * 100, 1);
            var latePct = Math.Round((double)lateEmployees / totalEmployees * 100, 1);
            var leavePct = Math.Round((double)onLeaveCount / totalEmployees * 100, 1);
            var absentPct = Math.Round((double)absentToday / totalEmployees * 100, 1);

            return new AdminSummaryDto
            {
                TotalEmployees = totalEmployees,
                PresentToday = presentToday,
                OnLeaveToday = onLeaveCount,
                AbsentToday = absentToday,
                PendingLeaves = pendingLeaves,
                PendingCorrections = pendingCorrections,
                LateEmployees = lateEmployees,
                MissingCheckOut = missingCheckOut,
                PresentPercentage = presentPct,
                LatePercentage = latePct,
                LeavePercentage = leavePct,
                AbsentPercentage = absentPct
            };
        }

        public async Task<ManagerSummaryDto> GetManagerSummaryAsync(long branchId)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.branchid == (int)branchId);
            var branchName = branch?.branchname ?? "Branch";

            var branchEmployees = await _context.Employees
                .Where(e => !e.IsDeleted && e.IsActive && e.BranchId == branchId)
                .Select(e => e.Id)
                .ToListAsync();

            var teamTotal = branchEmployees.Count;

            if (teamTotal == 0)
            {
                return new ManagerSummaryDto
                {
                    BranchId = branchId,
                    BranchName = branchName,
                    TeamTotal = 0,
                    PresentToday = 0,
                    OnLeaveToday = 0,
                    AbsentToday = 0,
                    PendingLeaves = 0,
                    PendingCorrections = 0,
                    TodayLeave = new List<EmployeeOnLeaveDto>(),
                    WeeklyAttendance = GetEmptyWeeklyAttendance(today)
                };
            }

            // Today's attendances for branch
            var todayAttendances = await _context.Attendances
                .Where(a => a.AttendanceDate == today && branchEmployees.Contains(a.EmployeeId))
                .ToListAsync();

            var presentToday = todayAttendances
                .Where(a => a.CheckInTime != null)
                .Select(a => a.EmployeeId)
                .Distinct()
                .Count();

            // On leave today in branch
            var activeLeaveRequests = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Where(l => l.Status == (short)LeaveRequestStatus.Approved &&
                            l.FromDate <= today && l.ToDate >= today &&
                            branchEmployees.Contains(l.EmployeeId))
                .ToListAsync();

            var onLeaveToday = activeLeaveRequests.Select(l => l.EmployeeId).Distinct().Count();
            var absentToday = Math.Max(0, teamTotal - (presentToday + onLeaveToday));

            var pendingLeaves = await _context.LeaveRequests
                .Where(l => l.Status == (short)LeaveRequestStatus.Pending &&
                            branchEmployees.Contains(l.EmployeeId))
                .CountAsync();

            var pendingCorrections = await _context.AttendanceCorrections
                .Where(c => c.Status == 0 &&
                            branchEmployees.Contains(c.EmployeeId))
                .CountAsync();

            var todayLeaveList = activeLeaveRequests
                .Select(l => new EmployeeOnLeaveDto
                {
                    EmployeeId = l.EmployeeId,
                    Name = $"{l.Employee.FirstName} {l.Employee.LastName ?? string.Empty}".Trim(),
                    LeaveType = l.LeaveType?.Name ?? "Leave",
                    Duration = l.TotalDays <= 0.5m ? "Half Day" : $"{l.TotalDays:0.#} Day(s)",
                    FromDate = l.FromDate,
                    ToDate = l.ToDate
                })
                .ToList();

            // Weekly Attendance: Mon to Fri
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = today.AddDays(-diff);
            var weeklyList = new List<WeeklyAttendanceDayDto>();
            string[] dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" };

            // Query attendance counts for the week
            var weekAttendances = await _context.Attendances
                .Where(a => a.AttendanceDate >= monday &&
                            a.AttendanceDate <= monday.AddDays(4) &&
                            branchEmployees.Contains(a.EmployeeId) &&
                            a.CheckInTime != null)
                .ToListAsync();

            for (int i = 0; i < 5; i++)
            {
                var d = monday.AddDays(i);
                int dayPresentCount = 0;

                if (d <= today)
                {
                    dayPresentCount = weekAttendances
                        .Where(a => a.AttendanceDate == d)
                        .Select(a => a.EmployeeId)
                        .Distinct()
                        .Count();
                }

                double pct = teamTotal > 0
                    ? Math.Round((double)dayPresentCount / teamTotal * 100, 1)
                    : 0.0;

                weeklyList.Add(new WeeklyAttendanceDayDto
                {
                    DayName = dayNames[i],
                    Date = d.ToString("yyyy-MM-dd"),
                    Percentage = pct,
                    PresentCount = dayPresentCount,
                    TotalCount = teamTotal
                });
            }

            return new ManagerSummaryDto
            {
                BranchId = branchId,
                BranchName = branchName,
                TeamTotal = teamTotal,
                PresentToday = presentToday,
                OnLeaveToday = onLeaveToday,
                AbsentToday = absentToday,
                PendingLeaves = pendingLeaves,
                PendingCorrections = pendingCorrections,
                TodayLeave = todayLeaveList,
                WeeklyAttendance = weeklyList
            };
        }

        private static List<WeeklyAttendanceDayDto> GetEmptyWeeklyAttendance(DateOnly today)
        {
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = today.AddDays(-diff);
            var list = new List<WeeklyAttendanceDayDto>();
            string[] dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" };
            for (int i = 0; i < 5; i++)
            {
                list.Add(new WeeklyAttendanceDayDto
                {
                    DayName = dayNames[i],
                    Date = monday.AddDays(i).ToString("yyyy-MM-dd"),
                    Percentage = 0.0,
                    PresentCount = 0,
                    TotalCount = 0
                });
            }
            return list;
        }
    }
}
