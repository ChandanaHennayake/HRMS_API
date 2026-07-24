using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("LeaveType")]
[Index("CompanyId", "Code", Name = "UQ_LeaveType_Company_Code", IsUnique = true)]
public partial class LeaveType
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    [StringLength(20)]
    public string Code { get; set; } = null!;

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(300)]
    public string? Description { get; set; }

    public bool IsPaid { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    [InverseProperty("LeaveType")]
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
