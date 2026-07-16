using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("role")]
public partial class role
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

    [InverseProperty("role")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    [ForeignKey("companyid")]
    [InverseProperty("roles")]
    public virtual company? company { get; set; }

    [InverseProperty("role")]
    public virtual ICollection<rolepermission> rolepermissions { get; set; } = new List<rolepermission>();
}
