using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("branch")]
public partial class branch
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

    [InverseProperty("branch")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    [ForeignKey("companyid")]
    [InverseProperty("branches")]
    public virtual company company { get; set; } = null!;

    [InverseProperty("branch")]
    public virtual ICollection<userbranch> userbranches { get; set; } = new List<userbranch>();
}
