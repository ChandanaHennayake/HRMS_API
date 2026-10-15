using System;
using System.Collections.Generic;

namespace HRMS.API.DTOs.Payroll
{
    public class PayrollPeriodDto
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsClosed { get; set; }
    }

    public class ProcessPayrollRequestDto
    {
        public long CompanyId { get; set; } = 1;
        public int? BranchId { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
        public int Month { get; set; } = DateTime.Now.Month;
        public decimal WorkingDaysDivisor { get; set; } = 26;
        public string? Comments { get; set; }
    }

    public class UpdatePayrollStatusDto
    {
        /// <summary>
        /// 0 = Draft, 1 = UnderReview, 2 = Approved, 3 = Published
        /// </summary>
        public short Status { get; set; }
        public string? Comments { get; set; }
    }

    public class PayrollRunDto
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public long PayrollPeriodId { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public short Status { get; set; }
        public string StatusName => Status switch
        {
            0 => "Draft",
            1 => "Under Review",
            2 => "Approved",
            3 => "Published",
            _ => "Unknown"
        };
        public decimal TotalGross { get; set; }
        public decimal TotalNet { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public int ProcessedCount { get; set; }
        public long ProcessedBy { get; set; }
        public DateTime ProcessedAt { get; set; }
        public long? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Comments { get; set; }
        public List<PayslipSummaryDto> Payslips { get; set; } = new();
    }

    public class PayslipSummaryDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal GrossEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid";
    }

    public class PayslipDto
    {
        public long Id { get; set; }
        public long PayrollRunId { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? BranchName { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public string PaymentMode { get; set; } = "Bank Transfer";

        public long PeriodId { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }

        public decimal BaseSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal GrossEarnings { get; set; }

        public decimal NoPayDays { get; set; }
        public decimal NoPayDeduction { get; set; }
        public decimal GrossEpfSalary { get; set; }
        public decimal EmployeeEpf { get; set; }
        public decimal EmployerEpf { get; set; }
        public decimal EmployerEtf { get; set; }
        public decimal ApitTax { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }

        public decimal NetSalary { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid";
        public DateTime? PaymentDate { get; set; }

        public List<PayslipItemDto> Earnings { get; set; } = new();
        public List<PayslipItemDto> Deductions { get; set; } = new();
    }

    public class PayslipItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public short ItemType { get; set; } // 1 = Earning, 2 = Deduction
        public decimal Amount { get; set; }
        public bool IsEpfApplicable { get; set; }
        public bool IsTaxable { get; set; }
    }

    public class IndividualPayrollRequestDto
    {
        public long EmployeeId { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
        public int Month { get; set; } = DateTime.Now.Month;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? WorkedDays { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal WorkingDaysDivisor { get; set; } = 26;
        public decimal PerDayRate { get; set; }
        public decimal NoPayDays { get; set; }
        public decimal NoPayDeduction { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeRate { get; set; }
        public decimal OvertimePay { get; set; }
        public bool DeductEpf { get; set; } = true;
        public decimal EmployeeEpfRate { get; set; } = 8;
        public decimal EmployerEpfRate { get; set; } = 12;
        public decimal EmployerEtfRate { get; set; } = 3;
        public string PaymentStatus { get; set; } = "Unpaid";
        public string PaymentMode { get; set; } = "Bank Transfer";
        public string? Comments { get; set; }
        public List<PayrollManualItemDto> Allowances { get; set; } = new();
        public List<PayrollManualItemDto> Deductions { get; set; } = new();
    }

    public class PayrollManualItemDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsEpfApplicable { get; set; } = false;
    }

    public class EmployeePayrollPreviewDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? BranchName { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public decimal BaseSalary { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalCalendarDays { get; set; }
        public int WorkedDays { get; set; }
        public int HolidaysCount { get; set; }
        public int SundaysCount { get; set; }
        public decimal WorkingDaysDivisor { get; set; } = 26;
        public decimal SuggestedDailyRate { get; set; }
        public decimal DetectedAbsentDays { get; set; }
        public decimal SuggestedNoPayDeduction { get; set; }
        public decimal OtHours { get; set; }
        public decimal HourlyOtRate { get; set; }
        public decimal SuggestedOtPay { get; set; }
        public List<PayrollManualItemDto> StandardAllowances { get; set; } = new();
        public List<PayrollManualItemDto> StandardDeductions { get; set; } = new();
    }
}

