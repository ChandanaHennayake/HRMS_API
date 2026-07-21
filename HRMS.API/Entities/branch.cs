using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class Branch
{
    [Key]
    public int branchid { get; set; }

    public int companyid { get; set; }

    [StringLength(20)]
    public string? branchcode { get; set; }

    [StringLength(150)]
    public string branchname { get; set; } = null!;

    public string? address { get; set; }

    [StringLength(30)]
    public string? phone { get; set; }

    public bool? isactive { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? createddate { get; set; }

    [Precision(10, 7)]
    public decimal? Latitude { get; set; }

    [Precision(10, 7)]
    public decimal? Longitude { get; set; }

    public int GeofenceRadiusMeters { get; set; }

    [InverseProperty("Branch")]
    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

    [InverseProperty("branch")]
    public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();

    [ForeignKey("companyid")]
    [InverseProperty("Branches")]
    public virtual Company company { get; set; } = null!;
}
