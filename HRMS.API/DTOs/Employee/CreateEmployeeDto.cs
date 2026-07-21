namespace HRMS.API.DTOs.Employee
{
    public class CreateEmployeeDto
    {
        // Employee Details
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

        public DateOnly? DateOfBirth { get; set; }
        public DateOnly JoinedDate { get; set; }

        public string? Gender { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }

        public long? CreatedBy { get; set; }

        // Login Details
        public int RoleId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}