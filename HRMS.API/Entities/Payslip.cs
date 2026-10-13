using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("Payslip")]
[Index("PayrollRunId", Name = "IX_Payslip_PayrollRunId")]
[Index("EmployeeId", "PeriodId", Name = "IX_Payslip_Employee_Period")]
public partial class Payslip
{
    [Key]
    public long Id { get; set; }

    public long PayrollRunId { get; set; }

    public long EmployeeId { get; set; }

    public long PeriodId { get; set; }

    [Precision(18, 2)]
    public decimal BaseSalary { get; set; }

    [Precision(18, 2)]
    public decimal TotalAllowances { get; set; }

    [Precision(5, 2)]
    public decimal OvertimeHours { get; set; }

    [Precision(18, 2)]
    public decimal OvertimePay { get; set; }

    [Precision(18, 2)]
    public decimal GrossEarnings { get; set; }

    [Precision(5, 2)]
    public decimal NoPayDays { get; set; }

    [Precision(18, 2)]
    public decimal NoPayDeduction { get; set; }

    [Precision(18, 2)]
    public decimal GrossEpfSalary { get; set; }

    [Precision(18, 2)]
    public decimal EmployeeEpf { get; set; }

    [Precision(18, 2)]
    public decimal EmployerEpf { get; set; }

    [Precision(18, 2)]
    public decimal EmployerEtf { get; set; }

    [Precision(18, 2)]
    public decimal ApitTax { get; set; }

    [Precision(18, 2)]
    public decimal OtherDeductions { get; set; }

    [Precision(18, 2)]
    public decimal TotalDeductions { get; set; }

    [Precision(18, 2)]
    public decimal NetSalary { get; set; }

    [StringLength(30)]
    public string PaymentStatus { get; set; } = "Unpaid";

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? PaymentDate { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("PayrollRunId")]
    public virtual PayrollRun PayrollRun { get; set; } = null!;

    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("PeriodId")]
    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;

    public virtual ICollection<PayslipItem> Items { get; set; } = new List<PayslipItem>();
}
