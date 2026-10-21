namespace HRMS.API.DTOs.User
{
    public class UserResponse
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public int? EmployeeId { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public string? ClearPassword { get; set; }
    }
}
