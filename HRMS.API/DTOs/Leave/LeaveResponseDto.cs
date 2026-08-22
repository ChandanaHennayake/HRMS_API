namespace HRMS.API.DTOs.Leave
{
    public class LeaveResponseDto
    {
        public long Id { get; set; }

        public long EmployeeId { get; set; }

        public long LeaveTypeId { get; set; }

        public string LeaveType { get; set; } = string.Empty;

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public decimal TotalDays { get; set; }

        public string? Reason { get; set; }

        public short Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public DateTime AppliedAt { get; set; }

        public long? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public long? RejectedBy { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? ManagerComment { get; set; }

        public DateTime? CancelledAt { get; set; }
    }
}