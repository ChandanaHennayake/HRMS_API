using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("SalaryComponent")]
[Index("CompanyId", "Code", Name = "UQ_SalaryComponent_Company_Code", IsUnique = true)]
public partial class SalaryComponent
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(30)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 1 = Earning, 2 = Deduction
    /// </summary>
    public short ComponentType { get; set; }

    public bool IsTaxable { get; set; } = true;

    public bool IsEpfApplicable { get; set; } = false;

    public bool IsFixed { get; set; } = true;

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
