using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Entities;

[Table("SalaryStructure")]
public partial class SalaryStructure
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(250)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SalaryStructureComponent> StructureComponents { get; set; } = new List<SalaryStructureComponent>();
}
