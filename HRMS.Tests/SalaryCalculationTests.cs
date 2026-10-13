using System.Collections.Generic;
using HRMS.API.DTOs.Salary;
using HRMS.API.Service.Salary;
using Xunit;

namespace HRMS.Tests
{
    public class SalaryCalculationTests
    {
        private readonly SalaryService _service;

        public SalaryCalculationTests()
        {
            // Null repositories since CalculateSalary is a pure deterministic engine
            _service = new SalaryService(null!, null!);
        }

        [Fact]
        public void Test_Deterministic_Salary_Fixture_Matches_Reference()
        {
            // Reference Fixture:
            // Basic 80,000 + Allowances 12,000 - No-Pay 3,000 - Deductions 2,000 = Net 87,000
            var request = new SalaryCalculationRequestDto
            {
                BaseSalary = 80000m,
                Allowances = new List<SalaryItemDto>
                {
                    new() { Name = "Fixed Allowance", Amount = 12000m, IsEpfApplicable = false, IsTaxable = true }
                },
                CustomNoPayDeduction = 3000m,
                Deductions = new List<SalaryItemDto>
                {
                    new() { Name = "Other Deductions", Amount = 2000m }
                },
                IncludeStatutoryDeductions = false // Component level calculation
            };

            var result = _service.CalculateSalary(request);

            Assert.Equal(80000m, result.BaseSalary);
            Assert.Equal(12000m, result.TotalAllowances);
            Assert.Equal(92000m, result.GrossEarnings);
            Assert.Equal(3000m, result.NoPayDeduction);
            Assert.Equal(2000m, result.OtherDeductions);
            Assert.Equal(5000m, result.TotalDeductions);
            Assert.Equal(87000m, result.NetSalary);
        }

        [Fact]
        public void Test_Sri_Lankan_Statutory_Deductions_Calculation()
        {
            // Gross EPF = Base 80,000 - No-Pay 3,000 = 77,000
            // Employee EPF 8% = 6,160
            // Employer EPF 12% = 9,240
            // Employer ETF 3% = 2,310
            var request = new SalaryCalculationRequestDto
            {
                BaseSalary = 80000m,
                Allowances = new List<SalaryItemDto>
                {
                    new() { Name = "Transport Allowance", Amount = 12000m, IsEpfApplicable = false, IsTaxable = true }
                },
                CustomNoPayDeduction = 3000m,
                Deductions = new List<SalaryItemDto>
                {
                    new() { Name = "Loan Deduction", Amount = 2000m }
                },
                IncludeStatutoryDeductions = true
            };

            var result = _service.CalculateSalary(request);

            Assert.Equal(77000m, result.GrossEpfSalary);
            Assert.Equal(6160m, result.EmployeeEpf); // 8% of 77,000
            Assert.Equal(9240m, result.EmployerEpf); // 12% of 77,000
            Assert.Equal(2310m, result.EmployerEtf); // 3% of 77,000

            // Total Deductions = No-Pay (3000) + Other Deductions (2000) + Employee EPF (6160) = 11,160
            Assert.Equal(11160m, result.TotalDeductions);

            // Net Salary = Gross Earnings (92,000) - Total Deductions (11,160) = 80,840
            Assert.Equal(80840m, result.NetSalary);
        }
    }
}
