namespace HRMS.API.DTOs.Dashboard
{
    public class EmployeeOnLeaveDto
    {
        public long EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
    }

    public class WeeklyAttendanceDayDto
    {
        public string DayName { get; set; } = string.Empty; // "Mon", "Tue", etc.
        public string Date { get; set; } = string.Empty; // "2026-10-12"
        public double Percentage { get; set; } // e.g. 92.0
        public int PresentCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class ManagerSummaryDto
    {
        public long BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TeamTotal { get; set; }
        public int PresentToday { get; set; }
        public int OnLeaveToday { get; set; }
        public int AbsentToday { get; set; }
        public int PendingLeaves { get; set; }
        public int PendingCorrections { get; set; }
        public List<EmployeeOnLeaveDto> TodayLeave { get; set; } = new();
        public List<WeeklyAttendanceDayDto> WeeklyAttendance { get; set; } = new();
    }
}
