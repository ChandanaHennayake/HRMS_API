using System;

namespace HRMS.API.DTOs.Attendance
{
    public class CreateAttendanceCorrectionDto
    {
        public DateOnly AttendanceDate { get; set; }
        public DateTime? RequestedCheckInTime { get; set; }
        public DateTime? RequestedCheckOutTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewAttendanceCorrectionDto
    {
        /// <summary>
        /// 1 = Approved, 2 = Rejected
        /// </summary>
        public short Status { get; set; }
        public string? ReviewerComment { get; set; }
    }

    public class AttendanceCorrectionDto
    {
        public long Id { get; set; }
        public int? AttendanceId { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateOnly AttendanceDate { get; set; }
        public DateTime? OriginalCheckInTime { get; set; }
        public DateTime? OriginalCheckOutTime { get; set; }
        public DateTime? RequestedCheckInTime { get; set; }
        public DateTime? RequestedCheckOutTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public short Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public long? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewerComment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
