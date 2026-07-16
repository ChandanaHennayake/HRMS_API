using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("User")]
[Index("email", Name = "User_email_key", IsUnique = true)]
[Index("username", Name = "User_username_key", IsUnique = true)]
public partial class User
{
    [Key]
    public int userid { get; set; }

    public int companyid { get; set; }

    public int? branchid { get; set; }

    public int roleid { get; set; }

    public int? employeeid { get; set; }

    [StringLength(100)]
    public string username { get; set; } = null!;

    [StringLength(150)]
    public string email { get; set; } = null!;

    public string passwordhash { get; set; } = null!;

    public string? passwordsalt { get; set; }

    [StringLength(100)]
    public string? firstname { get; set; }

    [StringLength(100)]
    public string? lastname { get; set; }

    [StringLength(30)]
    public string? phone { get; set; }

    public bool? isactive { get; set; }

    public bool? islocked { get; set; }

    public int? failedloginattempts { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? lockoutend { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? lastlogin { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? passwordchangeddate { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? createddate { get; set; }

    public int? createdby { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? modifieddate { get; set; }

    public int? modifiedby { get; set; }

    [ForeignKey("branchid")]
    [InverseProperty("Users")]
    public virtual branch? branch { get; set; }

    [ForeignKey("companyid")]
    [InverseProperty("Users")]
    public virtual company company { get; set; } = null!;

    [InverseProperty("user")]
    public virtual ICollection<loginhistory> loginhistories { get; set; } = new List<loginhistory>();

    [InverseProperty("user")]
    public virtual ICollection<passwordreset> passwordresets { get; set; } = new List<passwordreset>();

    [InverseProperty("user")]
    public virtual ICollection<refreshtoken> refreshtokens { get; set; } = new List<refreshtoken>();

    [ForeignKey("roleid")]
    [InverseProperty("Users")]
    public virtual role role { get; set; } = null!;

    [InverseProperty("user")]
    public virtual ICollection<userbranch> userbranches { get; set; } = new List<userbranch>();
}
