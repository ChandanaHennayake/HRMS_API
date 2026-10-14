using System.Collections.Generic;

namespace HRMS.API.DTOs.Salary
{
    public class SalaryComponentDto
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public short ComponentType { get; set; } // 1 = Earning, 2 = Deduction
        public string TypeName => ComponentType == 1 ? "Earning" : "Deduction";
        public bool IsTaxable { get; set; }
        public bool IsEpfApplicable { get; set; }
        public bool IsFixed { get; set; }
        public bool IsActive { get; set; }
    }

    public class SalaryItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsEpfApplicable { get; set; }
        public bool IsTaxable { get; set; }
    }

    public class SalaryCalculationRequestDto
    {
        public decimal BaseSalary { get; set; }
        public List<SalaryItemDto> Allowances { get; set; } = new();
        public List<SalaryItemDto> Deductions { get; set; } = new();
        public decimal NoPayDays { get; set; } = 0;
        public decimal WorkingDaysDivisor { get; set; } = 26; // Standard 26 or 30 days divisor
        public decimal OtHours { get; set; } = 0;
        public decimal? CustomNoPayDeduction { get; set; } // If provided, overrides formula
        public bool IncludeStatutoryDeductions { get; set; } = true;
    }

    public class SalaryCalculationResultDto
    {
        public decimal BaseSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal GrossEarnings { get; set; }
        public List<SalaryItemDto> EarningsBreakdown { get; set; } = new();

        public decimal NoPayDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public List<SalaryItemDto> DeductionsBreakdown { get; set; } = new();

        // Statutory
        public decimal GrossEpfSalary { get; set; }
        public decimal EmployeeEpf { get; set; } // 8%
        public decimal EmployerEpf { get; set; } // 12%
        public decimal EmployerEtf { get; set; } // 3%
        public decimal ApitTax { get; set; } // Advance Personal Income Tax

        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalEmployerCost { get; set; }
    }

    public class EmployeeSalaryDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public long? SalaryStructureId { get; set; }
        public string? StructureName { get; set; }
        public decimal BaseSalary { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public List<SalaryItemDto> Earnings { get; set; } = new();
        public List<SalaryItemDto> Deductions { get; set; } = new();
        public SalaryCalculationResultDto Calculation { get; set; } = new();
    }

    public class SaveEmployeeSalaryDto
    {
        public long EmployeeId { get; set; }
        public long? SalaryStructureId { get; set; }
        public decimal BaseSalary { get; set; }
        public DateOnly EffectiveFrom { get; set; } = DateOnly.FromDateTime(System.DateTime.Now);
        public string PaymentMode { get; set; } = "Bank Transfer";
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public List<SaveSalaryComponentItemDto> Components { get; set; } = new();
    }

    public class SaveSalaryComponentItemDto
    {
        public long SalaryComponentId { get; set; }
        public decimal Amount { get; set; }
    }
}
