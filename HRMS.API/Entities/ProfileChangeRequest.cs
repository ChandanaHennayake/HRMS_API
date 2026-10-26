using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Entities;

[Table("ProfileChangeRequest")]
public partial class ProfileChangeRequest
{
    [Key]
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public int? UserId { get; set; }

    public long CompanyId { get; set; }

    public long? BranchId { get; set; }

    // Proposed Personal & Contact Details
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(255)]
    public string? Qualification { get; set; }

    // Proposed Bank Details
    [StringLength(100)]
    public string? BankName { get; set; }

    [StringLength(50)]
    public string? BankAccountNo { get; set; }

    [StringLength(100)]
    public string? BankBranch { get; set; }

    // Snapshot of Current Values Before Change
    [StringLength(100)]
    public string? CurrentFirstName { get; set; }

    [StringLength(100)]
    public string? CurrentLastName { get; set; }

    [StringLength(30)]
    public string? CurrentPhone { get; set; }

    public DateOnly? CurrentDateOfBirth { get; set; }

    [StringLength(20)]
    public string? CurrentGender { get; set; }

    [StringLength(200)]
    public string? CurrentAddressLine1 { get; set; }

    [StringLength(200)]
    public string? CurrentAddressLine2 { get; set; }

    [StringLength(100)]
    public string? CurrentCity { get; set; }

    [StringLength(255)]
    public string? CurrentQualification { get; set; }

    [StringLength(100)]
    public string? CurrentBankName { get; set; }

    [StringLength(50)]
    public string? CurrentBankAccountNo { get; set; }

    [StringLength(100)]
    public string? CurrentBankBranch { get; set; }

    /// <summary>
    /// 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Cancelled
    /// </summary>
    public short Status { get; set; } = 0;

    [StringLength(500)]
    public string? Reason { get; set; }

    public long? ReviewedBy { get; set; }

    [StringLength(150)]
    public string? ReviewerName { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ReviewedAt { get; set; }

    [StringLength(500)]
    public string? ReviewerComment { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual AppUser? AppUser { get; set; }
}
