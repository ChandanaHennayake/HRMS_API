using System;
using System.Collections.Generic;

namespace HRMS.API.DTOs.Report
{
    // ==========================================
    // ATTENDANCE REPORT DTOs
    // ==========================================

    public class AttendanceReportFilterDto
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
    }

    public class AttendanceReportSummaryDto
    {
        public int TotalRecords { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int EarlyDepartureCount { get; set; }
        public double TotalWorkedHours { get; set; }
        public double TotalOvertimeHours { get; set; }
        public double PunctualityRate { get; set; }
    }

    public class AttendanceReportItemDto
    {
        public DateOnly Date { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double WorkedHours { get; set; }
        public bool IsLate { get; set; }
        public int LateMinutes { get; set; }
        public int OTMinutes { get; set; }
        public string Status { get; set; } = "Present";
    }

    public class AttendanceReportResponseDto
    {
        public AttendanceReportSummaryDto Summary { get; set; } = new();
        public List<AttendanceReportItemDto> Items { get; set; } = new();
    }

    // ==========================================
    // LEAVE REPORT DTOs
    // ==========================================

    public class LeaveReportFilterDto
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
        public long? LeaveTypeId { get; set; }
        public short? Status { get; set; }
    }

    public class LeaveReportSummaryDto
    {
        public int TotalRequests { get; set; }
        public decimal ApprovedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal RejectedDays { get; set; }
        public decimal PaidDays { get; set; }
        public decimal UnpaidDays { get; set; }
        public Dictionary<string, decimal> ByTypeBreakdown { get; set; } = new();
    }

    public class LeaveReportItemDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public decimal TotalDays { get; set; }
        public bool IsPaid { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
    }

    public class LeaveReportResponseDto
    {
        public LeaveReportSummaryDto Summary { get; set; } = new();
        public List<LeaveReportItemDto> Items { get; set; } = new();
    }

    // ==========================================
    // PAYROLL REPORT DTOs
    // ==========================================

    public class PayrollReportFilterDto
    {
        public long? PeriodId { get; set; }
        public long CompanyId { get; set; } = 1;
        public int? BranchId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class PayrollReportSummaryDto
    {
        public string PeriodName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal TotalBasicSalary { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalEPFEmployer { get; set; }
        public decimal TotalETFEmployer { get; set; }
        public decimal TotalCompanyCost { get; set; }
    }

    public class PayrollReportItemDto
    {
        public long PayslipId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public decimal GrossEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string PaymentStatus { get; set; } = "Paid";
    }

    public class PayrollReportResponseDto
    {
        public PayrollReportSummaryDto Summary { get; set; } = new();
        public List<PayrollReportItemDto> Items { get; set; } = new();
    }

    // ==========================================
    // HEADCOUNT / WORKFORCE DISTRIBUTION DTOs
    // ==========================================

    public class DistributionItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class HeadcountReportDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public List<DistributionItemDto> BranchDistribution { get; set; } = new();
        public List<DistributionItemDto> DepartmentDistribution { get; set; } = new();
        public List<DistributionItemDto> DesignationDistribution { get; set; } = new();
        public List<DistributionItemDto> EmploymentTypeDistribution { get; set; } = new();
    }
}
