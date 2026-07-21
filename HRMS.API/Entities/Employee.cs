using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Entities;

[Table("Employee")]
[Index("BranchId", Name = "IX_Employee_BranchId")]
[Index("CompanyId", Name = "IX_Employee_CompanyId")]
[Index("DepartmentId", Name = "IX_Employee_DepartmentId")]
[Index("DesignationId", Name = "IX_Employee_DesignationId")]
[Index("Email", Name = "IX_Employee_Email")]
[Index("EmploymentTypeId", Name = "IX_Employee_EmploymentTypeId")]
[Index("CompanyId", "EmployeeCode", Name = "UQ_Employee_Company_EmployeeCode", IsUnique = true)]
public partial class Employee
{
    [Key]
    public long Id { get; set; }

    public long CompanyId { get; set; }

    public long BranchId { get; set; }

    [StringLength(50)]
    public string EmployeeCode { get; set; } = null!;

    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public long? DepartmentId { get; set; }

    public long? DesignationId { get; set; }

    public long? EmploymentTypeId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly JoinedDate { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [ForeignKey("DepartmentId")]
    [InverseProperty("Employees")]
    public virtual Department? Department { get; set; }

    [ForeignKey("DesignationId")]
    [InverseProperty("Employees")]
    public virtual Designation? Designation { get; set; }

    [ForeignKey("EmploymentTypeId")]
    [InverseProperty("Employees")]
    public virtual EmploymentType? EmploymentType { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
