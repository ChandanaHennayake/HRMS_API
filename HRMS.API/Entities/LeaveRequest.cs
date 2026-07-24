using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("LeaveRequest")]
public partial class LeaveRequest
{
    [Key]
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public long LeaveTypeId { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    [Precision(5, 2)]
    public decimal TotalDays { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public short Status { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime AppliedAt { get; set; }

    public long? ApprovedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ApprovedAt { get; set; }

    public long? RejectedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? RejectedAt { get; set; }

    [StringLength(500)]
    public string? ManagerComment { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("LeaveRequests")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("LeaveTypeId")]
    [InverseProperty("LeaveRequests")]
    public virtual LeaveType LeaveType { get; set; } = null!;
}
