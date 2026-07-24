using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class LoginHistory
{
    [Key]
    public int loginhistoryid { get; set; }

    public int? userid { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? logintime { get; set; }

    [StringLength(100)]
    public string? ipaddress { get; set; }

    [StringLength(300)]
    public string? browser { get; set; }

    [StringLength(300)]
    public string? device { get; set; }

    public bool? issuccess { get; set; }

    [StringLength(300)]
    public string? failurereason { get; set; }

    [ForeignKey("userid")]
    [InverseProperty("LoginHistories")]
    public virtual AppUser? user { get; set; }
}
