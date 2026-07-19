namespace HRMS.API.DTOs.User
{
    public class UpdateUserRequest
    {
        public int CompanyId { get; set; }

        public int? BranchId { get; set; }

        public int RoleId { get; set; }

        public int? EmployeeId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsLocked { get; set; }
    }
}