namespace HRMS.API.DTOs.User
{
    public class UserProfileResponse
    {
        public int UserId { get; set; }
        public int? EmployeeId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public int RoleId { get; set; }

        // Employee Details
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? EmploymentTypeName { get; set; }
        public DateOnly? JoinedDate { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? Qualification { get; set; }

        // Bank Account Details
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankBranch { get; set; }

        // Change Request Status
        public bool HasPendingChangeRequest { get; set; }

        // Branch & Geofence
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal? BranchLatitude { get; set; }
        public decimal? BranchLongitude { get; set; }
        public int? GeofenceRadiusMeters { get; set; }
    }
}
