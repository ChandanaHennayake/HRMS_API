namespace HRMS.API.DTOs.User
{
    public class CreateUserRequest
    {
        public int CompanyId { get; set; }

        public int? BranchId { get; set; }

        public int RoleId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public int? EmployeeId { get; set; } 


    }
}
