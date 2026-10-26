using System;

namespace HRMS.API.DTOs.ProfileChange;

public class ProfileChangeRequestDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public int? UserId { get; set; }
    public long CompanyId { get; set; }
    public long? BranchId { get; set; }
    public string? BranchName { get; set; }

    // Proposed values
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Qualification { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankBranch { get; set; }

    // Current values snapshot
    public string? CurrentFirstName { get; set; }
    public string? CurrentLastName { get; set; }
    public string? CurrentPhone { get; set; }
    public DateOnly? CurrentDateOfBirth { get; set; }
    public string? CurrentGender { get; set; }
    public string? CurrentAddressLine1 { get; set; }
    public string? CurrentAddressLine2 { get; set; }
    public string? CurrentCity { get; set; }
    public string? CurrentQualification { get; set; }
    public string? CurrentBankName { get; set; }
    public string? CurrentBankAccountNo { get; set; }
    public string? CurrentBankBranch { get; set; }

    public short Status { get; set; }
    public string StatusText => Status switch
    {
        0 => "Pending",
        1 => "Approved",
        2 => "Rejected",
        3 => "Cancelled",
        _ => "Unknown"
    };

    public string? Reason { get; set; }
    public long? ReviewedBy { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewerComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
