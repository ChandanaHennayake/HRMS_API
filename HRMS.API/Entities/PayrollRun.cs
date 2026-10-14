using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("PayrollRun")]
[Index("CompanyId", "PayrollPeriodId", Name = "IX_PayrollRun_Company_Period")]
public partial class PayrollRun
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    public int? BranchId { get; set; }

    public long PayrollPeriodId { get; set; }

    /// <summary>
    /// 0 = Draft, 1 = UnderReview, 2 = Approved, 3 = Published
    /// </summary>
    public short Status { get; set; } = 0;

    [Precision(18, 2)]
    public decimal TotalGross { get; set; }

    [Precision(18, 2)]
    public decimal TotalNet { get; set; }

    [Precision(18, 2)]
    public decimal TotalEmployerCost { get; set; }

    public int ProcessedCount { get; set; }

    public long ProcessedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime ProcessedAt { get; set; } = DateTime.Now;

    public long? ApprovedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ApprovedAt { get; set; }

    [StringLength(500)]
    public string? Comments { get; set; }

    [ForeignKey("PayrollPeriodId")]
    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;

    [ForeignKey("BranchId")]
    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
