using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("userbranch")]
public partial class userbranch
{
    [Key]
    public int userbranchid { get; set; }

    public int userid { get; set; }

    public int branchid { get; set; }

    [ForeignKey("branchid")]
    [InverseProperty("userbranches")]
    public virtual branch branch { get; set; } = null!;

    [ForeignKey("userid")]
    [InverseProperty("userbranches")]
    public virtual User user { get; set; } = null!;
}
