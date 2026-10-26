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

            int? targetBranchId = (request.BranchId.HasValue && request.BranchId.Value > 0) ? request.BranchId.Value : null;
            if (targetBranchId.HasValue)
            {
                var branchExists = await _context.Branches.AnyAsync(b => b.branchid == targetBranchId.Value);
                if (!branchExists) targetBranchId = null;
            }

            // 3. Create Draft PayrollRun entity
            var run = new PayrollRun
            {
                CompanyId = request.CompanyId,
                BranchId = targetBranchId,
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
            int? filterBranchId = (branchId.HasValue && branchId.Value > 0) ? branchId : null;
            var runs = await _payrollRepository.GetRunsAsync(companyId, filterBranchId);
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

        public async Task<EmployeePayrollPreviewDto> GetEmployeePayrollPreviewAsync(
            long employeeId,
            int year,
            int month,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            var emp = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (emp == null)
            {
                throw new Exception("Employee not found.");
            }

            string? branchName = null;
            if (emp.BranchId > 0)
            {
                var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.branchid == (int)emp.BranchId);
                branchName = branch?.branchname;
            }

            var start = startDate ?? new DateOnly(year, month, 1);
            var end = endDate ?? new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            if (end < start)
            {
                var temp = start;
                start = end;
                end = temp;
            }

            var salary = await _salaryRepository.GetEmployeeSalaryAsync(employeeId);
            decimal baseSalary = salary?.BaseSalary ?? 0m;
            decimal workingDays = 26m;
            decimal dailyRate = baseSalary > 0 ? Math.Round(baseSalary / workingDays, 2) : 0m;
            decimal hourlyOtRate = dailyRate > 0 ? Math.Round((dailyRate / 8m) * 1.5m, 2) : 0m;

            // Company Holidays in date range
            var holidayDates = await _context.Holidays
                .Where(h => h.CompanyId == emp.CompanyId && h.IsActive && h.HolidayDate >= start && h.HolidayDate <= end)
                .Select(h => h.HolidayDate)
                .ToListAsync();
            var holidaySet = new HashSet<DateOnly>(holidayDates);

            // Attendance records in date range
            var attendances = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.AttendanceDate >= start && a.AttendanceDate <= end)
                .ToListAsync();

            // Total calendar days
            int totalCalendarDays = (end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).Days + 1;

            // Count Sundays and Holidays
            int sundaysCount = 0;
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Sunday)
                {
                    sundaysCount++;
                }
            }

            // Worked days: Present (Status == 1) or checked in or worked minutes > 0
            int workedDays = attendances.Count(a => a.Status == 1 || a.CheckInTime != null || a.WorkedMinutes > 0);

            // Total Overtime hours
            int totalOtMinutes = attendances.Sum(a => a.OTMinutes);
            decimal otHours = Math.Round((decimal)totalOtMinutes / 60m, 2);
            decimal suggestedOtPay = Math.Round(otHours * hourlyOtRate, 2);

            // Detected absent / no-pay days
            // NEVER count Company Holidays or Sundays as Absent!
            decimal absentDays = attendances.Count(a => a.Status == 0 && !holidaySet.Contains(a.AttendanceDate) && a.AttendanceDate.DayOfWeek != DayOfWeek.Sunday);

            // Check unpaid approved leaves
            var unpaidLeaves = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employeeId && l.Status == 1 && l.LeaveType != null && !l.LeaveType.IsPaid && l.FromDate <= end && l.ToDate >= start)
                .ToListAsync();

            foreach (var leave in unpaidLeaves)
            {
                var lStart = leave.FromDate < start ? start : leave.FromDate;
                var lEnd = leave.ToDate > end ? end : leave.ToDate;
                for (var ld = lStart; ld <= lEnd; ld = ld.AddDays(1))
                {
                    if (ld.DayOfWeek != DayOfWeek.Sunday && !holidaySet.Contains(ld))
                    {
                        if (!attendances.Any(a => a.AttendanceDate == ld && a.Status == 0))
                        {
                            absentDays += 1;
                        }
                    }
                }
            }

            decimal suggestedNoPay = Math.Round(absentDays * dailyRate, 2);

            var preview = new EmployeePayrollPreviewDto
            {
                EmployeeId = emp.Id,
                EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                EmployeeCode = emp.EmployeeCode,
                DepartmentName = emp.Department?.Name,
                DesignationName = emp.Designation?.Name,
                BranchName = branchName,
                BankName = salary?.BankName ?? emp.City,
                BankAccountNo = salary?.BankAccountNo ?? emp.AddressLine2,
                BaseSalary = baseSalary,
                StartDate = start,
                EndDate = end,
                TotalCalendarDays = totalCalendarDays,
                WorkedDays = workedDays,
                HolidaysCount = holidaySet.Count,
                SundaysCount = sundaysCount,
                WorkingDaysDivisor = workingDays,
                SuggestedDailyRate = dailyRate,
                DetectedAbsentDays = absentDays,
                SuggestedNoPayDeduction = suggestedNoPay,
                OtHours = otHours,
                HourlyOtRate = hourlyOtRate,
                SuggestedOtPay = suggestedOtPay
            };

            if (salary?.Components != null)
            {
                foreach (var c in salary.Components)
                {
                    try
                    {
                        if (c.SalaryComponent != null && c.SalaryComponent.IsActive)
                        {
                            if (c.SalaryComponent.ComponentType == 1) // Earning
                            {
                                preview.StandardAllowances.Add(new PayrollManualItemDto
                                {
                                    Name = c.SalaryComponent.Name,
                                    Amount = c.Amount,
                                    IsEpfApplicable = c.SalaryComponent.IsEpfApplicable
                                });
                            }
                            else if (c.SalaryComponent.ComponentType == 2) // Deduction
                            {
                                preview.StandardDeductions.Add(new PayrollManualItemDto
                                {
                                    Name = c.SalaryComponent.Name,
                                    Amount = c.Amount
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Non-fatal if single component cannot be evaluated
                    }
                }
            }

            return preview;
        }

        public async Task<PayslipDto> SaveIndividualPayrollAsync(IndividualPayrollRequestDto request, long savedBy)
        {
            var emp = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId);

            if (emp == null)
            {
                throw new Exception("Employee not found.");
            }

            // 1. Ensure or retrieve PayrollPeriod
            var period = await _payrollRepository.GetPeriodAsync(emp.CompanyId, request.Year, request.Month);
            if (period == null)
            {
                var startDate = request.StartDate ?? new DateOnly(request.Year, request.Month, 1);
                var endDate = request.EndDate ?? new DateOnly(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));
                var periodName = $"{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}";

                period = await _payrollRepository.CreatePeriodAsync(new PayrollPeriod
                {
                    CompanyId = emp.CompanyId,
                    Year = request.Year,
                    Month = request.Month,
                    Name = periodName,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsClosed = false
                });
            }

            // 2. Ensure or retrieve a PayrollRun for this period and branch
            int? empBranchId = emp.BranchId > 0 ? (int)emp.BranchId : null;
            if (empBranchId.HasValue)
            {
                var branchExists = await _context.Branches.AnyAsync(b => b.branchid == empBranchId.Value);
                if (!branchExists) empBranchId = null;
            }

            var run = await _context.PayrollRuns
                .FirstOrDefaultAsync(r => r.PayrollPeriodId == period.Id && (r.BranchId == empBranchId || r.BranchId == null));

            if (run == null)
            {
                run = new PayrollRun
                {
                    CompanyId = emp.CompanyId,
                    BranchId = empBranchId,
                    PayrollPeriodId = period.Id,
                    Status = 0, // Draft
                    ProcessedCount = 0,
                    ProcessedBy = savedBy,
                    ProcessedAt = DateTime.Now,
                    Comments = $"Payroll Run - {period.Name}"
                };

                await _context.PayrollRuns.AddAsync(run);
                await _context.SaveChangesAsync();
            }

            // 3. Mathematical Calculations
            decimal totalAllowances = request.Allowances?.Sum(a => a.Amount) ?? 0m;
            decimal grossEarnings = request.BaseSalary + totalAllowances + request.OvertimePay;

            decimal noPayDeduction = request.NoPayDeduction > 0
                ? request.NoPayDeduction
                : (request.NoPayDays * request.PerDayRate);

            decimal otherDeductions = request.Deductions?.Sum(d => d.Amount) ?? 0m;
            decimal totalDeductions = noPayDeduction + otherDeductions;
            decimal netSalary = Math.Max(0m, grossEarnings - totalDeductions);

            decimal employeeEpf = 0m;
            decimal employerEpf = 0m;
            decimal employerEtf = 0m;
            decimal grossEpfSalary = 0m;

            // 4. Find existing Payslip or create new
            Payslip? payslip = null;
            try
            {
                payslip = await _context.Payslips
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.EmployeeId == request.EmployeeId && p.PeriodId == period.Id);
            }
            catch (Exception)
            {
                payslip = await _context.Payslips
                    .FirstOrDefaultAsync(p => p.EmployeeId == request.EmployeeId && p.PeriodId == period.Id);
            }

            if (payslip == null)
            {
                payslip = new Payslip
                {
                    PayrollRunId = run.Id,
                    PeriodId = period.Id,
                    EmployeeId = request.EmployeeId,
                    CreatedAt = DateTime.Now
                };

                await _context.Payslips.AddAsync(payslip);
            }
            else
            {
                // Remove existing items to replace with updated breakdown
                try
                {
                    if (payslip.Items != null && payslip.Items.Any())
                    {
                        _context.PayslipItems.RemoveRange(payslip.Items);
                    }
                }
                catch (Exception)
                {
                }
            }

            payslip.PayrollRunId = run.Id;
            payslip.PeriodId = period.Id;
            payslip.BaseSalary = request.BaseSalary;
            payslip.TotalAllowances = totalAllowances;
            payslip.OvertimeHours = request.OvertimeHours;
            payslip.OvertimePay = request.OvertimePay;
            payslip.GrossEarnings = grossEarnings;

            payslip.NoPayDays = request.NoPayDays;
            payslip.NoPayDeduction = noPayDeduction;
            payslip.GrossEpfSalary = grossEpfSalary;
            payslip.EmployeeEpf = employeeEpf;
            payslip.EmployerEpf = employerEpf;
            payslip.EmployerEtf = employerEtf;
            payslip.OtherDeductions = otherDeductions;
            payslip.TotalDeductions = totalDeductions;
            payslip.NetSalary = netSalary;
            payslip.PaymentStatus = request.PaymentStatus ?? "Unpaid";
            payslip.PaymentDate = request.PaymentStatus == "Paid" ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            // 5. Populate Payslip Items
            var itemsToAdd = new List<PayslipItem>();

            if (request.Allowances != null)
            {
                foreach (var al in request.Allowances.Where(x => x.Amount > 0))
                {
                    var code = string.IsNullOrWhiteSpace(al.Name) ? "ALLOWANCE" : al.Name.Replace(" ", "_").ToUpper().Trim();
                    if (code.Length > 30) code = code.Substring(0, 30);
                    var name = al.Name.Length > 100 ? al.Name.Substring(0, 100) : al.Name;

                    itemsToAdd.Add(new PayslipItem
                    {
                        PayslipId = payslip.Id,
                        Name = name,
                        Code = code,
                        ItemType = 1, // Earning
                        Amount = al.Amount,
                        IsEpfApplicable = al.IsEpfApplicable,
                        IsTaxable = true
                    });
                }
            }

            if (request.Deductions != null)
            {
                foreach (var de in request.Deductions.Where(x => x.Amount > 0))
                {
                    var code = string.IsNullOrWhiteSpace(de.Name) ? "DEDUCTION" : de.Name.Replace(" ", "_").ToUpper().Trim();
                    if (code.Length > 30) code = code.Substring(0, 30);
                    var name = de.Name.Length > 100 ? de.Name.Substring(0, 100) : de.Name;

                    itemsToAdd.Add(new PayslipItem
                    {
                        PayslipId = payslip.Id,
                        Name = name,
                        Code = code,
                        ItemType = 2, // Deduction
                        Amount = de.Amount,
                        IsEpfApplicable = false,
                        IsTaxable = false
                    });
                }
            }

            if (noPayDeduction > 0)
            {
                var name = $"No-Pay Deduction ({request.NoPayDays:0.#} days @ {request.PerDayRate:0.00}/day)";
                if (name.Length > 100) name = name.Substring(0, 100);

                itemsToAdd.Add(new PayslipItem
                {
                    PayslipId = payslip.Id,
                    Name = name,
                    Code = "NO_PAY",
                    ItemType = 2,
                    Amount = noPayDeduction,
                    IsEpfApplicable = false,
                    IsTaxable = false
                });
            }

            if (employeeEpf > 0)
            {
                var name = $"EPF Employee ({request.EmployeeEpfRate}%)";
                if (name.Length > 100) name = name.Substring(0, 100);

                itemsToAdd.Add(new PayslipItem
                {
                    PayslipId = payslip.Id,
                    Name = name,
                    Code = "EPF_EMP",
                    ItemType = 2,
                    Amount = employeeEpf,
                    IsEpfApplicable = true,
                    IsTaxable = false
                });
            }

            if (itemsToAdd.Any())
            {
                await _context.PayslipItems.AddRangeAsync(itemsToAdd);
                await _context.SaveChangesAsync();
            }

            // 6. Recalculate Run totals
            var allSlips = await _context.Payslips
                .Where(p => p.PayrollRunId == run.Id)
                .ToListAsync();

            run.TotalGross = allSlips.Sum(s => s.GrossEarnings);
            run.TotalNet = allSlips.Sum(s => s.NetSalary);
            run.TotalEmployerCost = allSlips.Sum(s => s.GrossEarnings);
            run.ProcessedCount = allSlips.Count;

            await _context.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(
                (int)savedBy,
                "PayrollUser",
                "SaveIndividualPayroll",
                $"Saved individual payslip for employee #{emp.Id} ({emp.FirstName} {emp.LastName}) for period {period.Name}: Net {netSalary:0.00}",
                null,
                true);

            return (await GetPayslipDetailsAsync(payslip.Id))!;
        }

        public async Task DeletePayslipAsync(long payslipId, long deletedBy)
        {
            var payslip = await _context.Payslips
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == payslipId);

            if (payslip == null)
            {
                throw new Exception("Payslip not found.");
            }

            var runId = payslip.PayrollRunId;

            if (payslip.Items != null && payslip.Items.Any())
            {
                _context.PayslipItems.RemoveRange(payslip.Items);
            }

            _context.Payslips.Remove(payslip);
            await _context.SaveChangesAsync();

            var run = await _context.PayrollRuns.FindAsync(runId);
            if (run != null)
            {
                var remaining = await _context.Payslips
                    .Where(p => p.PayrollRunId == runId)
                    .ToListAsync();

                run.TotalGross = remaining.Sum(s => s.GrossEarnings);
                run.TotalNet = remaining.Sum(s => s.NetSalary);
                run.TotalEmployerCost = remaining.Sum(s => s.GrossEarnings + s.EmployerEpf + s.EmployerEtf);
                run.ProcessedCount = remaining.Count;

                await _context.SaveChangesAsync();
            }

            await _auditService.LogSecurityEventAsync(
                (int)deletedBy,
                "PayrollUser",
                "DeletePayslip",
                $"Deleted payslip #{payslipId}",
                null,
                true);
        }
    }
}
