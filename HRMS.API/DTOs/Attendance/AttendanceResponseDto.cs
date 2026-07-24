namespace ABANS_BAN.DTOs.Attendance
{
    public class AttendanceResponseDto
    {
        public int Id { get; set; }

        public long EmployeeId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public decimal? CheckInDistanceMeters { get; set; }

        public decimal? CheckOutDistanceMeters { get; set; }

        public int WorkedMinutes { get; set; }

        public bool IsLate { get; set; }

        public int LateMinutes { get; set; }

        public bool IsEarlyDeparture { get; set; }

        public int EarlyDepartureMinutes { get; set; }

        public int OTMinutes { get; set; }

        public int Status { get; set; }
    }
}