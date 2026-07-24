using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("Holiday")]
[Index("CompanyId", "HolidayDate", Name = "UQ_Holiday_Company_Date", IsUnique = true)]
public partial class Holiday
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    public DateOnly HolidayDate { get; set; }

    [StringLength(150)]
    public string HolidayName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    public long? CreatedBy { get; set; }
}
