using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("PayslipItem")]
[Index("PayslipId", Name = "IX_PayslipItem_PayslipId")]
public partial class PayslipItem
{
    [Key]
    public long Id { get; set; }

    public long PayslipId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 1 = Earning, 2 = Deduction
    /// </summary>
    public short ItemType { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public bool IsEpfApplicable { get; set; }

    public bool IsTaxable { get; set; }

    [ForeignKey("PayslipId")]
    public virtual Payslip Payslip { get; set; } = null!;
}
