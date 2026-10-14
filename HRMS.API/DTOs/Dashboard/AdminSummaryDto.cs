namespace HRMS.API.DTOs.Dashboard
{
    public class AdminSummaryDto
    {
        public int TotalEmployees { get; set; }
        public int PresentToday { get; set; }
        public int OnLeaveToday { get; set; }
        public int AbsentToday { get; set; }
        public int PendingLeaves { get; set; }
        public int LateEmployees { get; set; }
        public int MissingCheckOut { get; set; }
        public double PresentPercentage { get; set; }
        public double LatePercentage { get; set; }
        public double LeavePercentage { get; set; }
        public double AbsentPercentage { get; set; }
    }
}
