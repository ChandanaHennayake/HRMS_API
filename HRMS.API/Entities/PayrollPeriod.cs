using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("PayrollPeriod")]
[Index("CompanyId", "Year", "Month", Name = "UQ_PayrollPeriod_Company_Year_Month", IsUnique = true)]
public partial class PayrollPeriod
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string? PeriodName { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateOnly? CutoffDate { get; set; }

    public bool IsClosed { get; set; } = false;

    public short Status { get; set; } = 0;

    public long? CreatedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
}
