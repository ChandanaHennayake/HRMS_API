using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("Designation")]
[Index("CompanyId", "Name", Name = "UQ_Designation_Company_Name", IsUnique = true)]
public partial class Designation
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Code { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    [InverseProperty("Designation")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
