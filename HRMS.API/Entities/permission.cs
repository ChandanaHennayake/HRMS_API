using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class Permission
{
    [Key]
    public int permissionid { get; set; }

    [StringLength(100)]
    public string? module { get; set; }

    [StringLength(150)]
    public string? permissionname { get; set; }

    [StringLength(300)]
    public string? description { get; set; }

    [InverseProperty("permission")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
