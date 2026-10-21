namespace HRMS.API.DTOs.Employee
{
    public class UpdateEmployeeDto
    {
        public long CompanyId { get; set; }
        public long BranchId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }

        public long? DepartmentId { get; set; }
        public long? DesignationId { get; set; }
        public long? EmploymentTypeId { get; set; }

        // PostgreSQL DATE
        public DateOnly? DateOfBirth { get; set; }

        // PostgreSQL DATE
        public DateOnly JoinedDate { get; set; }

        public string? Gender { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }

        // Bank Details
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankBranch { get; set; }

        // Qualification Details
        public string? Qualification { get; set; }

        public bool IsActive { get; set; }

        // Optional User Account updates
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }

        public long? UpdatedBy { get; set; }
    }
}