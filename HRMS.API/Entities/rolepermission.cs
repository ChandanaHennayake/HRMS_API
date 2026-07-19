using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class RolePermission
{
    [Key]
    public int rolepermissionid { get; set; }

    public int roleid { get; set; }

    public int permissionid { get; set; }

    public bool? canview { get; set; }

    public bool? cancreate { get; set; }

    public bool? canedit { get; set; }

    public bool? candelete { get; set; }

    public bool? canapprove { get; set; }

    [ForeignKey("permissionid")]
    [InverseProperty("RolePermissions")]
    public virtual Permission permission { get; set; } = null!;

    [ForeignKey("roleid")]
    [InverseProperty("RolePermissions")]
    public virtual Role role { get; set; } = null!;
}
