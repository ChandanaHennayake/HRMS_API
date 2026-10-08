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
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? EmploymentTypeName { get; set; }
        public DateOnly? JoinedDate { get; set; }

        // Branch & Geofence
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal? BranchLatitude { get; set; }
        public decimal? BranchLongitude { get; set; }
        public int? GeofenceRadiusMeters { get; set; }
    }
}
