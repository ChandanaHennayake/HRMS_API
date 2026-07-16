using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("rolepermission")]
public partial class rolepermission
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
    [InverseProperty("rolepermissions")]
    public virtual permission permission { get; set; } = null!;

    [ForeignKey("roleid")]
    [InverseProperty("rolepermissions")]
    public virtual role role { get; set; } = null!;
}
