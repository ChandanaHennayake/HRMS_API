using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("EmployeeSalary")]
[Index("EmployeeId", Name = "IX_EmployeeSalary_EmployeeId")]
public partial class EmployeeSalary
{
    [Key]
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public long? SalaryStructureId { get; set; }

    [Precision(18, 2)]
    public decimal BaseSalary { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    [StringLength(50)]
    public string PaymentMode { get; set; } = "Bank Transfer";

    [StringLength(100)]
    public string? BankName { get; set; }

    [StringLength(50)]
    public string? BankAccountNo { get; set; }

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("SalaryStructureId")]
    public virtual SalaryStructure? SalaryStructure { get; set; }

    public virtual ICollection<EmployeeSalaryComponent> Components { get; set; } = new List<EmployeeSalaryComponent>();
}
