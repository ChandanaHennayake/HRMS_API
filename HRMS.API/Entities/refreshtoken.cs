using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

public partial class RefreshToken
{
    [Key]
    public int refreshtokenid { get; set; }

    public int userid { get; set; }

    public string token { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime expirydate { get; set; }

    public bool? isrevoked { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? createddate { get; set; }

    [ForeignKey("userid")]
    [InverseProperty("RefreshTokens")]
    public virtual AppUser user { get; set; } = null!;
}
