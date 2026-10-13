using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("SalaryStructureComponent")]
[Index("SalaryStructureId", "SalaryComponentId", Name = "UQ_Structure_Component", IsUnique = true)]
public partial class SalaryStructureComponent
{
    [Key]
    public long Id { get; set; }

    public long SalaryStructureId { get; set; }

    public long SalaryComponentId { get; set; }

    [Precision(18, 2)]
    public decimal DefaultAmount { get; set; }

    [Precision(5, 2)]
    public decimal? PercentageOfBasic { get; set; }

    [ForeignKey("SalaryStructureId")]
    public virtual SalaryStructure SalaryStructure { get; set; } = null!;

    [ForeignKey("SalaryComponentId")]
    public virtual SalaryComponent SalaryComponent { get; set; } = null!;
}
