namespace HRMS.API.DTOs.Employee
{
    public class EmployeeDto
    {
        public long Id { get; set; }

        public long CompanyId { get; set; }
        public long BranchId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Phone { get; set; }

        public long? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public long? DesignationId { get; set; }
        public string? DesignationName { get; set; }

        public long? EmploymentTypeId { get; set; }
        public string? EmploymentTypeName { get; set; }

        // PostgreSQL DATE
        public DateOnly? DateOfBirth { get; set; }

        // PostgreSQL DATE
        public DateOnly JoinedDate { get; set; }

        public string? Gender { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }

        public bool IsActive { get; set; }

        // PostgreSQL TIMESTAMPTZ
        public DateTime CreatedAt { get; set; }
    }
}