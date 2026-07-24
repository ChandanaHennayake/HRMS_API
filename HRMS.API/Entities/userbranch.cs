using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class UserBranch
{
    [Key]
    public int userbranchid { get; set; }

    public int userid { get; set; }

    public int branchid { get; set; }

    [ForeignKey("branchid")]
    [InverseProperty("UserBranches")]
    public virtual Branch branch { get; set; } = null!;

    [ForeignKey("userid")]
    [InverseProperty("UserBranches")]
    public virtual AppUser user { get; set; } = null!;
}
