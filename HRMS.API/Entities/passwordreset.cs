using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class PasswordReset
{
    [Key]
    public int passwordresetid { get; set; }

    public int userid { get; set; }

    [StringLength(250)]
    public string resettoken { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime expirydate { get; set; }

    public bool? used { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? createddate { get; set; }

    [ForeignKey("userid")]
    [InverseProperty("PasswordResets")]
    public virtual AppUser user { get; set; } = null!;
}
