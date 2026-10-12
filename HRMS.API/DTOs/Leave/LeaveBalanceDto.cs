using System;

namespace HRMS.API.DTOs.Leave
{
    public class LeaveBalanceDto
    {
        public long LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveTypeCode { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal AvailableDays => Math.Max(0, AllocatedDays - (UsedDays + PendingDays));
    }
}
