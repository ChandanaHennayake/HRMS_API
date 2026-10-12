using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Entities;

[Table("AttendanceCorrection")]
public partial class AttendanceCorrection
{
    [Key]
    public long Id { get; set; }

    public int? AttendanceId { get; set; }

    public long EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? RequestedCheckInTime { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? RequestedCheckOutTime { get; set; }

    [StringLength(500)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Cancelled
    /// </summary>
    public short Status { get; set; }

    public long? ReviewedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ReviewedAt { get; set; }

    [StringLength(500)]
    public string? ReviewerComment { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("AttendanceId")]
    public virtual Attendance? Attendance { get; set; }

    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;
}
