namespace HRMS.API.DTOs.Authentication
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public int EmployeeId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string Role { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public int? BranchId { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime Expiration { get; set; }
    }
}