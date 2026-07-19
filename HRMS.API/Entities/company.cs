using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Index("companycode", Name = "company_companycode_key", IsUnique = true)]
public partial class Company
{
    [Key]
    public int companyid { get; set; }

    [StringLength(20)]
    public string companycode { get; set; } = null!;

    [StringLength(150)]
    public string companyname { get; set; } = null!;

    [StringLength(150)]
    public string? email { get; set; }

    [StringLength(30)]
    public string? phone { get; set; }

    public string? address { get; set; }

    public bool isactive { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime createddate { get; set; }

    public int? createdby { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? modifieddate { get; set; }

    public int? modifiedby { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

    [InverseProperty("company")]
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    [InverseProperty("company")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
