using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("refreshtoken")]
public partial class refreshtoken
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
    [InverseProperty("refreshtokens")]
    public virtual User user { get; set; } = null!;
}
