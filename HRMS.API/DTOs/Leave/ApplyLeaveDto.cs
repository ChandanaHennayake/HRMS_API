namespace HRMS.API.DTOs.Leave
{
    public class ApplyLeaveDto
    {
        public long LeaveTypeId { get; set; }

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public string? Reason { get; set; }
    }
}