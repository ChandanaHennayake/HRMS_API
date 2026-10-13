using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.DTOs.Salary;
using HRMS.API.Entities;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repository.Salary;

namespace HRMS.API.Service.Salary
{
    public class SalaryService : ISalaryService
    {
        private readonly ISalaryRepository _salaryRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public SalaryService(
            ISalaryRepository salaryRepository,
            IEmployeeRepository employeeRepository)
        {
            _salaryRepository = salaryRepository;
            _employeeRepository = employeeRepository;
        }

        public SalaryCalculationResultDto CalculateSalary(SalaryCalculationRequestDto req)
        {
            var result = new SalaryCalculationResultDto
            {
                BaseSalary = req.BaseSalary
            };

            // 1. Allowances & Earnings
            decimal totalAllowances = 0;
            decimal epfAllowances = 0;

            if (req.Allowances != null)
            {
                foreach (var a in req.Allowances)
                {
                    totalAllowances += a.Amount;
                    if (a.IsEpfApplicable) epfAllowances += a.Amount;
                    result.EarningsBreakdown.Add(a);
                }
            }
            result.TotalAllowances = totalAllowances;

            // Overtime Pay
            decimal otPay = 0;
            if (req.OtHours > 0)
            {
                decimal hourlyRate = req.BaseSalary > 0 ? req.BaseSalary / 200m : 0m;
                otPay = Math.Round(hourlyRate * 1.5m * req.OtHours, 2);
            }
            result.OvertimePay = otPay;
            result.GrossEarnings = req.BaseSalary + totalAllowances + otPay;

            // 2. No-Pay Deduction
            decimal noPayDeduction = 0;
            if (req.CustomNoPayDeduction.HasValue)
            {
                noPayDeduction = req.CustomNoPayDeduction.Value;
            }
            else if (req.NoPayDays > 0)
            {
                decimal divisor = req.WorkingDaysDivisor > 0 ? req.WorkingDaysDivisor : 26m;
                noPayDeduction = Math.Round((req.BaseSalary / divisor) * req.NoPayDays, 2);
            }
            result.NoPayDeduction = noPayDeduction;

            // 3. Other Deductions
            decimal otherDeductions = 0;
            if (req.Deductions != null)
            {
                foreach (var d in req.Deductions)
                {
                    otherDeductions += d.Amount;
                    result.DeductionsBreakdown.Add(d);
                }
            }
            result.OtherDeductions = otherDeductions;

            // 4. Statutory Deductions (Sri Lanka)
            decimal grossEpfSalary = Math.Max(0, req.BaseSalary + epfAllowances - noPayDeduction);
            result.GrossEpfSalary = grossEpfSalary;

            decimal employeeEpf = 0;
            decimal employerEpf = 0;
            decimal employerEtf = 0;
            decimal apitTax = 0;

            if (req.IncludeStatutoryDeductions)
            {
                employeeEpf = Math.Round(grossEpfSalary * 0.08m, 2); // 8% Employee EPF
                employerEpf = Math.Round(grossEpfSalary * 0.12m, 2); // 12% Employer EPF
                employerEtf = Math.Round(grossEpfSalary * 0.03m, 2); // 3% Employer ETF

                // Sri Lanka APIT progressive monthly brackets (tax free up to 100,000 LKR)
                decimal taxableIncome = Math.Max(0, result.GrossEarnings - noPayDeduction - employeeEpf);
                if (taxableIncome > 100000m)
                {
                    decimal excess = taxableIncome - 100000m;
                    decimal bracketSize = 41666.67m;
                    decimal tax = 0;
                    decimal[] rates = [0.06m, 0.12m, 0.18m, 0.24m, 0.30m, 0.36m];
                    int i = 0;
                    while (excess > 0 && i < rates.Length)
                    {
                        decimal amountInBracket = Math.Min(excess, bracketSize);
                        tax += amountInBracket * rates[i];
                        excess -= amountInBracket;
                        i++;
                    }
                    apitTax = Math.Round(tax, 2);
                }
            }

            result.EmployeeEpf = employeeEpf;
            result.EmployerEpf = employerEpf;
            result.EmployerEtf = employerEtf;
            result.ApitTax = apitTax;

            // Total Deductions & Net Salary
            result.TotalDeductions = noPayDeduction + otherDeductions + employeeEpf + apitTax;
            result.NetSalary = result.GrossEarnings - result.TotalDeductions;
            result.TotalEmployerCost = (result.GrossEarnings - noPayDeduction) + employerEpf + employerEtf;

            return result;
        }

        public async Task<EmployeeSalaryDto?> GetEmployeeSalaryAsync(long employeeId)
        {
            var employee = await _employeeRepository.GetEntityByIdAsync(employeeId);
            if (employee == null) return null;

            await _salaryRepository.EnsureDefaultComponentsAsync(employee.CompanyId);

            var salary = await _salaryRepository.GetEmployeeSalaryAsync(employeeId);

            var dto = new EmployeeSalaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
                EmployeeCode = employee.EmployeeCode,
                SalaryStructureId = salary?.SalaryStructureId,
                StructureName = salary?.SalaryStructure?.Name ?? "Standard Monthly Package",
                BaseSalary = salary?.BaseSalary ?? 0m,
                EffectiveFrom = salary?.EffectiveFrom ?? DateOnly.FromDateTime(DateTime.Now),
                PaymentMode = salary?.PaymentMode ?? "Bank Transfer",
                BankName = salary?.BankName,
                BankAccountNo = salary?.BankAccountNo
            };

            if (salary?.Components != null)
            {
                foreach (var c in salary.Components)
                {
                    var item = new SalaryItemDto
                    {
                        Name = c.SalaryComponent?.Name ?? "Allowance",
                        Code = c.SalaryComponent?.Code ?? "",
                        Amount = c.Amount,
                        IsEpfApplicable = c.SalaryComponent?.IsEpfApplicable ?? false,
                        IsTaxable = c.SalaryComponent?.IsTaxable ?? true
                    };

                    if (c.SalaryComponent?.ComponentType == 2)
                    {
                        dto.Deductions.Add(item);
                    }
                    else
                    {
                        dto.Earnings.Add(item);
                    }
                }
            }

            // Run real-time calculation
            dto.Calculation = CalculateSalary(new SalaryCalculationRequestDto
            {
                BaseSalary = dto.BaseSalary,
                Allowances = dto.Earnings,
                Deductions = dto.Deductions,
                IncludeStatutoryDeductions = true
            });

            return dto;
        }

        public async Task<EmployeeSalaryDto> SaveEmployeeSalaryAsync(SaveEmployeeSalaryDto request)
        {
            var employee = await _employeeRepository.GetEntityByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                throw new Exception("Employee not found.");
            }

            var entity = new EmployeeSalary
            {
                EmployeeId = request.EmployeeId,
                SalaryStructureId = request.SalaryStructureId,
                BaseSalary = request.BaseSalary,
                EffectiveFrom = request.EffectiveFrom,
                PaymentMode = request.PaymentMode,
                BankName = request.BankName?.Trim(),
                BankAccountNo = request.BankAccountNo?.Trim()
            };

            if (request.Components != null)
            {
                foreach (var c in request.Components)
                {
                    entity.Components.Add(new EmployeeSalaryComponent
                    {
                        SalaryComponentId = c.SalaryComponentId,
                        Amount = c.Amount
                    });
                }
            }

            await _salaryRepository.SaveEmployeeSalaryAsync(entity);

            return (await GetEmployeeSalaryAsync(request.EmployeeId))!;
        }

        public async Task<IEnumerable<SalaryComponentDto>> GetComponentsAsync(long companyId)
        {
            await _salaryRepository.EnsureDefaultComponentsAsync(companyId);

            var list = await _salaryRepository.GetComponentsAsync(companyId);
            return list.Select(c => new SalaryComponentDto
            {
                Id = c.Id,
                CompanyId = c.CompanyId,
                Name = c.Name,
                Code = c.Code,
                ComponentType = c.ComponentType,
                IsTaxable = c.IsTaxable,
                IsEpfApplicable = c.IsEpfApplicable,
                IsFixed = c.IsFixed,
                IsActive = c.IsActive
            });
        }

        public async Task<SalaryComponentDto> CreateComponentAsync(SalaryComponentDto request)
        {
            var entity = new SalaryComponent
            {
                CompanyId = request.CompanyId,
                Name = request.Name.Trim(),
                Code = request.Code.Trim().ToUpperInvariant(),
                ComponentType = request.ComponentType,
                IsTaxable = request.IsTaxable,
                IsEpfApplicable = request.IsEpfApplicable,
                IsFixed = request.IsFixed,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var created = await _salaryRepository.CreateComponentAsync(entity);

            return new SalaryComponentDto
            {
                Id = created.Id,
                CompanyId = created.CompanyId,
                Name = created.Name,
                Code = created.Code,
                ComponentType = created.ComponentType,
                IsTaxable = created.IsTaxable,
                IsEpfApplicable = created.IsEpfApplicable,
                IsFixed = created.IsFixed,
                IsActive = created.IsActive
            };
        }

        public async Task<IEnumerable<SalaryStructure>> GetStructuresAsync(long companyId)
        {
            return await _salaryRepository.GetStructuresAsync(companyId);
        }
    }
}
