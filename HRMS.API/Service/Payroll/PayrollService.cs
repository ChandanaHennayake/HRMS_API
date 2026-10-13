using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.DTOs.Payroll;
using HRMS.API.DTOs.Salary;
using HRMS.API.Entities;
using HRMS.API.Repository.Payroll;
using HRMS.API.Repository.Salary;
using HRMS.API.Service.Audit;
using HRMS.API.Service.Email;
using HRMS.API.Service.Notification;
using HRMS.API.Service.Salary;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Service.Payroll
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ISalaryService _salaryService;
        private readonly ISalaryRepository _salaryRepository;
        private readonly DefaultContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;

        public PayrollService(
            IPayrollRepository payrollRepository,
            ISalaryService salaryService,
            ISalaryRepository salaryRepository,
            DefaultContext context,
            INotificationService notificationService,
            IAuditService auditService,
            IEmailService emailService)
        {
            _payrollRepository = payrollRepository;
            _salaryService = salaryService;
            _salaryRepository = salaryRepository;
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<PayrollPeriodDto>> GetPeriodsAsync(long companyId, int? year)
        {
            var periods = await _payrollRepository.GetPeriodsAsync(companyId, year);
            return periods.Select(p => new PayrollPeriodDto
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                Year = p.Year,
                Month = p.Month,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsClosed = p.IsClosed
            });
        }

        public async Task<PayrollRunDto> ProcessPayrollAsync(ProcessPayrollRequestDto request, long processedBy)
        {
            // 1. Ensure or retrieve PayrollPeriod
            var period = await _payrollRepository.GetPeriodAsync(request.CompanyId, request.Year, request.Month);
            if (period == null)
            {
                var startDate = new DateOnly(request.Year, request.Month, 1);
                var endDate = new DateOnly(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));
                var periodName = $"{new DateTime(request.Year, request.Month, 1):MMMM yyyy}";

                period = await _payrollRepository.CreatePeriodAsync(new PayrollPeriod
                {
                    CompanyId = request.CompanyId,
                    Year = request.Year,
                    Month = request.Month,
                    Name = periodName,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsClosed = false,
                    CreatedAt = DateTime.Now
                });
            }

            // 2. Fetch active employees
            var empQuery = _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => e.CompanyId == request.CompanyId && e.IsActive && !e.IsDeleted);

            if (request.BranchId.HasValue && request.BranchId.Value > 0)
            {
                empQuery = empQuery.Where(e => e.BranchId == request.BranchId.Value);
            }

            var employees = await empQuery.ToListAsync();

            if (!employees.Any())
            {
                throw new Exception("No active employees found matching the specified parameters.");
            }

            // 3. Create Draft PayrollRun entity
            var run = new PayrollRun
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                PayrollPeriodId = period.Id,
                Status = 0, // Draft
                ProcessedBy = processedBy,
                ProcessedAt = DateTime.Now,
                Comments = request.Comments?.Trim()
            };

            decimal runGross = 0;
            decimal runNet = 0;
            decimal runEmployerCost = 0;

            // 4. Calculate Payslip for each employee
            foreach (var emp in employees)
            {
                // Fetch employee salary config
                var empSalary = await _salaryRepository.GetEmployeeSalaryAsync(emp.Id);

                decimal baseSalary = empSalary?.BaseSalary ?? 50000m; // Default baseline if unconfigured

                var allowances = new List<SalaryItemDto>();
                var deductions = new List<SalaryItemDto>();

                if (empSalary?.Components != null && empSalary.Components.Any())
                {
                    foreach (var c in empSalary.Components)
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
                            deductions.Add(item);
                        else
                            allowances.Add(item);
                    }
                }

                // Gather attendance OT in period
                var attendances = await _context.Attendances
                    .AsNoTracking()
                    .Where(a => a.EmployeeId == emp.Id &&
                                a.AttendanceDate >= period.StartDate &&
                                a.AttendanceDate <= period.EndDate)
                    .ToListAsync();

                decimal totalOtMinutes = attendances.Sum(a => a.OTMinutes);
                decimal otHours = Math.Round(totalOtMinutes / 60m, 2);

                // Run deterministic calculation engine
                var calcResult = _salaryService.CalculateSalary(new SalaryCalculationRequestDto
                {
                    BaseSalary = baseSalary,
                    Allowances = allowances,
                    Deductions = deductions,
                    NoPayDays = 0, // In standard runs, paid leaves cover excused absences
                    WorkingDaysDivisor = request.WorkingDaysDivisor,
                    OtHours = otHours,
                    IncludeStatutoryDeductions = true
                });

                var payslip = new Payslip
                {
                    EmployeeId = emp.Id,
                    PeriodId = period.Id,
                    BaseSalary = calcResult.BaseSalary,
                    TotalAllowances = calcResult.TotalAllowances,
                    OvertimeHours = otHours,
                    OvertimePay = calcResult.OvertimePay,
                    GrossEarnings = calcResult.GrossEarnings,
                    NoPayDays = calcResult.NoPayDeduction > 0 ? 1 : 0,
                    NoPayDeduction = calcResult.NoPayDeduction,
                    GrossEpfSalary = calcResult.GrossEpfSalary,
                    EmployeeEpf = calcResult.EmployeeEpf,
                    EmployerEpf = calcResult.EmployerEpf,
                    EmployerEtf = calcResult.EmployerEtf,
                    ApitTax = calcResult.ApitTax,
                    OtherDeductions = calcResult.OtherDeductions,
                    TotalDeductions = calcResult.TotalDeductions,
                    NetSalary = calcResult.NetSalary,
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.Now
                };

                // Add itemized line items
                foreach (var a in calcResult.EarningsBreakdown)
                {
                    payslip.Items.Add(new PayslipItem
                    {
                        Name = a.Name,
                        Code = a.Code,
                        ItemType = 1,
                        Amount = a.Amount,
                        IsEpfApplicable = a.IsEpfApplicable,
                        IsTaxable = a.IsTaxable
                    });
                }

                foreach (var d in calcResult.DeductionsBreakdown)
                {
                    payslip.Items.Add(new PayslipItem
                    {
                        Name = d.Name,
                        Code = d.Code,
                        ItemType = 2,
                        Amount = d.Amount,
                        IsEpfApplicable = d.IsEpfApplicable,
                        IsTaxable = d.IsTaxable
                    });
                }

                run.Payslips.Add(payslip);

                runGross += calcResult.GrossEarnings;
                runNet += calcResult.NetSalary;
                runEmployerCost += calcResult.TotalEmployerCost;
            }

            run.TotalGross = runGross;
            run.TotalNet = runNet;
            run.TotalEmployerCost = runEmployerCost;
            run.ProcessedCount = employees.Count;

            await _payrollRepository.CreateRunAsync(run);

            return (await GetRunDetailsAsync(run.Id))!;
        }

        public async Task<IEnumerable<PayrollRunDto>> GetRunsAsync(long companyId, int? branchId)
        {
            var runs = await _payrollRepository.GetRunsAsync(companyId, branchId);
            return runs.Select(r => new PayrollRunDto
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                BranchId = r.BranchId,
                BranchName = r.Branch?.branchname,
                PayrollPeriodId = r.PayrollPeriodId,
                PeriodName = r.PayrollPeriod?.Name ?? "",
                Status = r.Status,
                TotalGross = r.TotalGross,
                TotalNet = r.TotalNet,
                TotalEmployerCost = r.TotalEmployerCost,
                ProcessedCount = r.ProcessedCount,
                ProcessedBy = r.ProcessedBy,
                ProcessedAt = r.ProcessedAt,
                ApprovedBy = r.ApprovedBy,
                ApprovedAt = r.ApprovedAt,
                Comments = r.Comments
            });
        }

        public async Task<PayrollRunDto?> GetRunDetailsAsync(long runId)
        {
            var run = await _payrollRepository.GetRunByIdAsync(runId);
            if (run == null) return null;

            var dto = new PayrollRunDto
            {
                Id = run.Id,
                CompanyId = run.CompanyId,
                BranchId = run.BranchId,
                BranchName = run.Branch?.branchname,
                PayrollPeriodId = run.PayrollPeriodId,
                PeriodName = run.PayrollPeriod?.Name ?? "",
                Status = run.Status,
                TotalGross = run.TotalGross,
                TotalNet = run.TotalNet,
                TotalEmployerCost = run.TotalEmployerCost,
                ProcessedCount = run.ProcessedCount,
                ProcessedBy = run.ProcessedBy,
                ProcessedAt = run.ProcessedAt,
                ApprovedBy = run.ApprovedBy,
                ApprovedAt = run.ApprovedAt,
                Comments = run.Comments
            };

            if (run.Payslips != null)
            {
                dto.Payslips = run.Payslips.Select(p => new PayslipSummaryDto
                {
                    Id = p.Id,
                    EmployeeId = p.EmployeeId,
                    EmployeeName = $"{p.Employee?.FirstName} {p.Employee?.LastName}".Trim(),
                    EmployeeCode = p.Employee?.EmployeeCode ?? "",
                    DepartmentName = p.Employee?.Department?.Name,
                    DesignationName = p.Employee?.Designation?.Name,
                    BaseSalary = p.BaseSalary,
                    GrossEarnings = p.GrossEarnings,
                    TotalDeductions = p.TotalDeductions,
                    NetSalary = p.NetSalary,
                    PaymentStatus = p.PaymentStatus
                }).ToList();
            }

            return dto;
        }

        public async Task<PayrollRunDto> UpdateRunStatusAsync(long runId, short newStatus, long reviewerId, string? comment)
        {
            var run = await _context.PayrollRuns
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Payslips)
                .FirstOrDefaultAsync(r => r.Id == runId);

            if (run == null)
            {
                throw new Exception("Payroll run not found.");
            }

            run.Status = newStatus;
            run.ApprovedBy = reviewerId;
            run.ApprovedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(comment))
            {
                run.Comments = string.IsNullOrWhiteSpace(run.Comments)
                    ? comment.Trim()
                    : $"{run.Comments} | {comment.Trim()}";
            }

            if (newStatus == 3) // Published
            {
                string periodName = run.PayrollPeriod?.Name ?? "Payroll Period";

                foreach (var p in run.Payslips)
                {
                    p.PaymentStatus = "Paid";
                    p.PaymentDate = DateTime.Now;

                    await _notificationService.NotifyPayrollPublishedAsync(
                        p.EmployeeId,
                        periodName,
                        p.NetSalary,
                        p.Id);

                    if (p.Employee != null && !string.IsNullOrWhiteSpace(p.Employee.Email))
                    {
                        await _emailService.SendPayslipPublishedEmailAsync(
                            p.Employee.Email,
                            $"{p.Employee.FirstName} {p.Employee.LastName}".Trim(),
                            periodName,
                            p.NetSalary);
                    }
                }
            }

            await _payrollRepository.UpdateRunAsync(run);

            var statusName = newStatus switch
            {
                0 => "Draft",
                1 => "UnderReview",
                2 => "Approved",
                3 => "Published",
                _ => $"Status_{newStatus}"
            };

            await _auditService.LogEntityActionAsync(
                reviewerId != 0 ? (int)reviewerId : null,
                null,
                newStatus == 3 ? "Publish" : (newStatus == 2 ? "Approve" : "Update"),
                "PayrollRun",
                runId.ToString(),
                $"Payroll run #{runId} status transitioned to {statusName}. Comments: {comment}",
                severity: newStatus == 3 ? "Critical" : "Info");

            return (await GetRunDetailsAsync(runId))!;
        }

        public async Task<IEnumerable<PayslipSummaryDto>> GetEmployeePayslipsAsync(long employeeId)
        {
            var payslips = await _payrollRepository.GetEmployeePayslipsAsync(employeeId);
            return payslips.Select(p => new PayslipSummaryDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeName = $"{p.Employee?.FirstName} {p.Employee?.LastName}".Trim(),
                EmployeeCode = p.Employee?.EmployeeCode ?? "",
                DepartmentName = p.PayrollPeriod?.Name,
                DesignationName = p.PayrollPeriod?.StartDate.ToString("MMM yyyy"),
                BaseSalary = p.BaseSalary,
                GrossEarnings = p.GrossEarnings,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                PaymentStatus = p.PaymentStatus
            });
        }

        public async Task<PayslipDto?> GetPayslipDetailsAsync(long payslipId)
        {
            var p = await _payrollRepository.GetPayslipByIdAsync(payslipId);
            if (p == null) return null;

            string? branchName = null;
            if (p.Employee != null && p.Employee.BranchId > 0)
            {
                var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.branchid == (int)p.Employee.BranchId);
                branchName = branch?.branchname;
            }

            var dto = new PayslipDto
            {
                Id = p.Id,
                PayrollRunId = p.PayrollRunId,
                EmployeeId = p.EmployeeId,
                EmployeeName = $"{p.Employee?.FirstName} {p.Employee?.LastName}".Trim(),
                EmployeeCode = p.Employee?.EmployeeCode ?? "",
                DepartmentName = p.Employee?.Department?.Name,
                DesignationName = p.Employee?.Designation?.Name,
                BranchName = branchName,
                PaymentMode = "Bank Transfer",

                PeriodId = p.PeriodId,
                PeriodName = p.PayrollPeriod?.Name ?? "",
                PeriodStartDate = p.PayrollPeriod?.StartDate ?? DateOnly.MinValue,
                PeriodEndDate = p.PayrollPeriod?.EndDate ?? DateOnly.MinValue,

                BaseSalary = p.BaseSalary,
                TotalAllowances = p.TotalAllowances,
                OvertimeHours = p.OvertimeHours,
                OvertimePay = p.OvertimePay,
                GrossEarnings = p.GrossEarnings,

                NoPayDays = p.NoPayDays,
                NoPayDeduction = p.NoPayDeduction,
                GrossEpfSalary = p.GrossEpfSalary,
                EmployeeEpf = p.EmployeeEpf,
                EmployerEpf = p.EmployerEpf,
                EmployerEtf = p.EmployerEtf,
                ApitTax = p.ApitTax,
                OtherDeductions = p.OtherDeductions,
                TotalDeductions = p.TotalDeductions,

                NetSalary = p.NetSalary,
                PaymentStatus = p.PaymentStatus,
                PaymentDate = p.PaymentDate
            };

            if (p.Items != null)
            {
                foreach (var item in p.Items)
                {
                    var itemDto = new PayslipItemDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        ItemType = item.ItemType,
                        Amount = item.Amount,
                        IsEpfApplicable = item.IsEpfApplicable,
                        IsTaxable = item.IsTaxable
                    };

                    if (item.ItemType == 2)
                        dto.Deductions.Add(itemDto);
                    else
                        dto.Earnings.Add(itemDto);
                }
            }

            return dto;
        }
    }
}
