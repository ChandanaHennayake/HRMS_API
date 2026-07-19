using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("AppUser")]
[Index("Email", Name = "User_email_key", IsUnique = true)]
[Index("Username", Name = "User_username_key", IsUnique = true)]
public partial class AppUser
{
    [Key]
    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public int? BranchId { get; set; }

    public int RoleId { get; set; }

    public int? EmployeeId { get; set; }

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(150)]
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PasswordSalt { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsLocked { get; set; }

    public int? FailedLoginAttempts { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? LockoutEnd { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? LastLogin { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? PasswordChangedDate { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    [ForeignKey("BranchId")]
    [InverseProperty("AppUsers")]
    public virtual Branch? Branch { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("AppUsers")]
    public virtual Company Company { get; set; } = null!;

    [InverseProperty("user")]
    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    [InverseProperty("user")]
    public virtual ICollection<PasswordReset> PasswordResets { get; set; } = new List<PasswordReset>();

    [InverseProperty("user")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [ForeignKey("RoleId")]
    [InverseProperty("AppUsers")]
    public virtual Role Role { get; set; } = null!;

    [InverseProperty("user")]
    public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
