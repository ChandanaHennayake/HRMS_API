namespace HRMS.API.DTOs.Leave
{
    public class LeaveActionDto
    {
        public string? Comment { get; set; }
        public string? Remarks { get; set; }

        public string EffectiveComment => (!string.IsNullOrWhiteSpace(Comment) ? Comment : Remarks)?.Trim() ?? string.Empty;
    }
}