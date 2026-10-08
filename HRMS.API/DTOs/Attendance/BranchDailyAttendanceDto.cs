namespace HRMS.API.DTOs.Attendance
{
    public class BranchDailyAttendanceDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public long BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int WorkedMinutes { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsLate { get; set; }
        public int LateMinutes { get; set; }
        public bool IsEarlyDeparture { get; set; }
        public int EarlyDepartureMinutes { get; set; }
    }
}
