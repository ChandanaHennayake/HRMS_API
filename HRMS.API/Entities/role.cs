using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class Role
{
    [Key]
    public int roleid { get; set; }

    public int? companyid { get; set; }

    [StringLength(100)]
    public string rolename { get; set; } = null!;

    [StringLength(300)]
    public string? description { get; set; }

    public bool? issystemrole { get; set; }

    public bool? isactive { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? createddate { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

    [InverseProperty("role")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [ForeignKey("companyid")]
    [InverseProperty("Roles")]
    public virtual Company? company { get; set; }
}
