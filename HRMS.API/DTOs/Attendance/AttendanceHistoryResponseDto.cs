namespace ABANS_BAN.DTOs.Attendance
{
    public class AttendanceHistoryResponseDto
    {
        public long EmployeeId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public int AttendanceStatus { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public decimal? CheckInLatitude { get; set; }

        public decimal? CheckInLongitude { get; set; }

        public decimal? CheckInDistanceMeters { get; set; }

        public decimal? CheckOutLatitude { get; set; }

        public decimal? CheckOutLongitude { get; set; }

        public decimal? CheckOutDistanceMeters { get; set; }

        public int WorkedMinutes { get; set; }

        public bool IsLate { get; set; }

        public int LateMinutes { get; set; }

        public bool IsEarlyDeparture { get; set; }

        public int EarlyDepartureMinutes { get; set; }

        public int OTMinutes { get; set; }

        public long? LeaveTypeId { get; set; }

        public string? LeaveTypeName { get; set; }

        public long? HolidayId { get; set; }

        public string? HolidayName { get; set; }
    }
}