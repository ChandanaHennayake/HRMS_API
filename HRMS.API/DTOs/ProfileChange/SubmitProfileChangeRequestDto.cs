using System;

namespace HRMS.API.DTOs.ProfileChange;

public class SubmitProfileChangeRequestDto
{
    public long? EmployeeId { get; set; }

    // Personal & Contact Details
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Qualification { get; set; }

    // Bank Details
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankBranch { get; set; }

    public string? Reason { get; set; }
}
