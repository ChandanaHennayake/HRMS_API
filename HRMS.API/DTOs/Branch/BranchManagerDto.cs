namespace HRMS.API.DTOs.Branch
{
    public class BranchManagerDto
    {
        public int UserBranchId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
    }
}
