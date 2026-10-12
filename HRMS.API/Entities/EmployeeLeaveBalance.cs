using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("EmployeeLeaveBalance")]
[Index("EmployeeId", "LeaveTypeId", "Year", Name = "UQ_Employee_LeaveType_Year", IsUnique = true)]
public partial class EmployeeLeaveBalance
{
    [Key]
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public long LeaveTypeId { get; set; }

    public int Year { get; set; }

    [Precision(5, 2)]
    public decimal AllocatedDays { get; set; }

    [Precision(5, 2)]
    public decimal UsedDays { get; set; }

    [Precision(5, 2)]
    public decimal PendingDays { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("LeaveTypeId")]
    public virtual LeaveType LeaveType { get; set; } = null!;
}
