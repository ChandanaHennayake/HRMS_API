using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("company")]
[Index("companycode", Name = "company_companycode_key", IsUnique = true)]
public partial class company
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

    [InverseProperty("company")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    [InverseProperty("company")]
    public virtual ICollection<branch> branches { get; set; } = new List<branch>();

    [InverseProperty("company")]
    public virtual ICollection<role> roles { get; set; } = new List<role>();
}
