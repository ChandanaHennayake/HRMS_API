using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("Attendance")]
[Index("EmployeeId", "AttendanceDate", Name = "UQ_Attendance_Employee_Date", IsUnique = true)]
public partial class Attendance
{
    [Key]
    public int Id { get; set; }

    public long EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CheckInTime { get; set; }

    [Precision(10, 7)]
    public decimal? CheckInLatitude { get; set; }

    [Precision(10, 7)]
    public decimal? CheckInLongitude { get; set; }

    [Precision(10, 2)]
    public decimal? CheckInDistanceMeters { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CheckOutTime { get; set; }

    [Precision(10, 7)]
    public decimal? CheckOutLatitude { get; set; }

    [Precision(10, 7)]
    public decimal? CheckOutLongitude { get; set; }

    [Precision(10, 2)]
    public decimal? CheckOutDistanceMeters { get; set; }

    public int WorkedMinutes { get; set; }

    public bool IsLate { get; set; }

    public int LateMinutes { get; set; }

    public bool IsEarlyDeparture { get; set; }

    public int EarlyDepartureMinutes { get; set; }

    public int OTMinutes { get; set; }

    public int Status { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Attendances")]
    public virtual Employee Employee { get; set; } = null!;
}
