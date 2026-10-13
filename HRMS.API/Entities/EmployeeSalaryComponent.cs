using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("EmployeeSalaryComponent")]
[Index("EmployeeSalaryId", "SalaryComponentId", Name = "UQ_EmployeeSalary_Component", IsUnique = true)]
public partial class EmployeeSalaryComponent
{
    [Key]
    public long Id { get; set; }

    public long EmployeeSalaryId { get; set; }

    public long SalaryComponentId { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [ForeignKey("EmployeeSalaryId")]
    public virtual EmployeeSalary EmployeeSalary { get; set; } = null!;

    [ForeignKey("SalaryComponentId")]
    public virtual SalaryComponent SalaryComponent { get; set; } = null!;
}
