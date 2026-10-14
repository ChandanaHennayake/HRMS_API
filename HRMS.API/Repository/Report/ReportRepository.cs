using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.DTOs.Report;
using HRMS.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Report
{
    public class ReportRepository : IReportRepository
    {
        private readonly DefaultContext _context;

        public ReportRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<AttendanceReportResponseDto> GetAttendanceReportAsync(AttendanceReportFilterDto filter)
        {
            var now = DateTime.UtcNow;
            var start = filter.StartDate ?? new DateOnly(now.Year, now.Month, 1);
            var end = filter.EndDate ?? DateOnly.FromDateTime(now);

            var query = _context.Attendances
                .AsNoTracking()
                .Include(a => a.Employee)
                .Where(a => a.AttendanceDate >= start && a.AttendanceDate <= end);

            if (filter.BranchId.HasValue && filter.BranchId.Value > 0)
            {
                query = query.Where(a => a.Employee != null && a.Employee.BranchId == filter.BranchId.Value);
            }

            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value > 0)
            {
                query = query.Where(a => a.Employee != null && a.Employee.DepartmentId == filter.DepartmentId.Value);
            }

            var list = await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Employee!.FirstName)
                .ToListAsync();

            var branches = await _context.Branches.AsNoTracking().ToDictionaryAsync(b => b.branchid, b => b.branchname);
            var departments = await _context.Departments.AsNoTracking().ToDictionaryAsync(d => d.Id, d => d.Name);

            var items = new List<AttendanceReportItemDto>();
            int presentCount = 0;
            int lateCount = 0;
            int earlyDepartureCount = 0;
            int absentCount = 0;
            double totalWorkedHours = 0;
            double totalOtHours = 0;

            foreach (var a in list)
            {
                var emp = a.Employee;
                string branchName = "-";
                if (emp != null && branches.TryGetValue((int)emp.BranchId, out var bn))
                {
                    branchName = bn;
                }

                string deptName = "-";
                if (emp != null && emp.DepartmentId.HasValue && departments.TryGetValue(emp.DepartmentId.Value, out var dn))
                {
                    deptName = dn;
                }

                var workedHours = Math.Round(a.WorkedMinutes / 60.0, 2);
                var otHours = Math.Round(a.OTMinutes / 60.0, 2);

                totalWorkedHours += workedHours;
                totalOtHours += otHours;

                if (a.Status == 1) presentCount++;
                else if (a.Status == 3) absentCount++;

                if (a.IsLate) lateCount++;
                if (a.IsEarlyDeparture) earlyDepartureCount++;

                items.Add(new AttendanceReportItemDto
                {
                    Date = a.AttendanceDate,
                    EmployeeId = a.EmployeeId,
                    EmployeeCode = emp?.EmployeeCode ?? "",
                    EmployeeName = $"{emp?.FirstName} {emp?.LastName}".Trim(),
                    BranchName = branchName,
                    DepartmentName = deptName,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    WorkedHours = workedHours,
                    IsLate = a.IsLate,
                    LateMinutes = a.LateMinutes,
                    OTMinutes = a.OTMinutes,
                    Status = a.Status switch
                    {
                        1 => "Present",
                        2 => "On Leave",
                        3 => "Absent",
                        4 => "Holiday",
                        _ => "Pending"
                    }
                });
            }

            double punctualityRate = presentCount > 0
                ? Math.Round(((double)(presentCount - lateCount) / presentCount) * 100.0, 1)
                : 100.0;

            var summary = new AttendanceReportSummaryDto
            {
                TotalRecords = list.Count,
                PresentCount = presentCount,
                LateCount = lateCount,
                AbsentCount = absentCount,
                EarlyDepartureCount = earlyDepartureCount,
                TotalWorkedHours = Math.Round(totalWorkedHours, 2),
                TotalOvertimeHours = Math.Round(totalOtHours, 2),
                PunctualityRate = punctualityRate
            };

            return new AttendanceReportResponseDto
            {
                Summary = summary,
                Items = items
            };
        }

        public async Task<LeaveReportResponseDto> GetLeaveReportAsync(LeaveReportFilterDto filter)
        {
            var now = DateTime.UtcNow;
            var start = filter.StartDate ?? new DateOnly(now.Year, 1, 1);
            var end = filter.EndDate ?? DateOnly.FromDateTime(now);

            var query = _context.LeaveRequests
                .AsNoTracking()
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.FromDate <= end && lr.ToDate >= start);

            if (filter.BranchId.HasValue && filter.BranchId.Value > 0)
            {
                query = query.Where(lr => lr.Employee != null && lr.Employee.BranchId == filter.BranchId.Value);
            }

            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value > 0)
            {
                query = query.Where(lr => lr.Employee != null && lr.Employee.DepartmentId == filter.DepartmentId.Value);
            }

            if (filter.LeaveTypeId.HasValue && filter.LeaveTypeId.Value > 0)
            {
                query = query.Where(lr => lr.LeaveTypeId == filter.LeaveTypeId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(lr => lr.Status == filter.Status.Value);
            }

            var list = await query
                .OrderByDescending(lr => lr.FromDate)
                .ToListAsync();

            var branches = await _context.Branches.AsNoTracking().ToDictionaryAsync(b => b.branchid, b => b.branchname);

            var items = new List<LeaveReportItemDto>();
            decimal approvedDays = 0;
            decimal pendingDays = 0;
            decimal rejectedDays = 0;
            decimal paidDays = 0;
            decimal unpaidDays = 0;
            var byTypeMap = new Dictionary<string, decimal>();

            foreach (var lr in list)
            {
                var emp = lr.Employee;
                string branchName = "-";
                if (emp != null && branches.TryGetValue((int)emp.BranchId, out var bn))
                {
                    branchName = bn;
                }
                var typeName = lr.LeaveType?.Name ?? "Leave";
                bool isPaid = lr.LeaveType?.IsPaid ?? true;

                if (lr.Status == (short)LeaveRequestStatus.Approved)
                {
                    approvedDays += lr.TotalDays;
                    if (isPaid) paidDays += lr.TotalDays;
                    else unpaidDays += lr.TotalDays;

                    if (!byTypeMap.ContainsKey(typeName)) byTypeMap[typeName] = 0;
                    byTypeMap[typeName] += lr.TotalDays;
                }
                else if (lr.Status == (short)LeaveRequestStatus.Pending)
                {
                    pendingDays += lr.TotalDays;
                }
                else if (lr.Status == (short)LeaveRequestStatus.Rejected)
                {
                    rejectedDays += lr.TotalDays;
                }

                items.Add(new LeaveReportItemDto
                {
                    Id = lr.Id,
                    EmployeeId = lr.EmployeeId,
                    EmployeeCode = emp?.EmployeeCode ?? "",
                    EmployeeName = $"{emp?.FirstName} {emp?.LastName}".Trim(),
                    BranchName = branchName,
                    LeaveTypeName = typeName,
                    FromDate = lr.FromDate,
                    ToDate = lr.ToDate,
                    TotalDays = lr.TotalDays,
                    IsPaid = isPaid,
                    Status = ((LeaveRequestStatus)lr.Status).ToString(),
                    ApprovedAt = lr.ApprovedAt
                });
            }

            var summary = new LeaveReportSummaryDto
            {
                TotalRequests = list.Count,
                ApprovedDays = approvedDays,
                PendingDays = pendingDays,
                RejectedDays = rejectedDays,
                PaidDays = paidDays,
                UnpaidDays = unpaidDays,
                ByTypeBreakdown = byTypeMap
            };

            return new LeaveReportResponseDto
            {
                Summary = summary,
                Items = items
            };
        }

        public async Task<PayrollReportResponseDto> GetPayrollReportAsync(PayrollReportFilterDto filter)
        {
            var query = _context.Payslips
                .AsNoTracking()
                .Include(p => p.Employee)
                .Include(p => p.PayrollRun)
                    .ThenInclude(r => r!.PayrollPeriod)
                .AsQueryable();

            if (filter.PeriodId.HasValue && filter.PeriodId.Value > 0)
            {
                query = query.Where(p => p.PayrollRun != null && p.PayrollRun.PayrollPeriodId == filter.PeriodId.Value);
            }
            else if (filter.Year.HasValue && filter.Year.Value > 0)
            {
                query = query.Where(p => p.PayrollRun != null && p.PayrollRun.PayrollPeriod != null && p.PayrollRun.PayrollPeriod.StartDate.Year == filter.Year.Value);
                if (filter.Month.HasValue && filter.Month.Value > 0)
                {
                    query = query.Where(p => p.PayrollRun != null && p.PayrollRun.PayrollPeriod != null && p.PayrollRun.PayrollPeriod.StartDate.Month == filter.Month.Value);
                }
            }

            if (filter.BranchId.HasValue && filter.BranchId.Value > 0)
            {
                query = query.Where(p => p.Employee != null && p.Employee.BranchId == filter.BranchId.Value);
            }

            var list = await query
                .OrderBy(p => p.Employee!.FirstName)
                .ToListAsync();

            var branches = await _context.Branches.AsNoTracking().ToDictionaryAsync(b => b.branchid, b => b.branchname);
            var departments = await _context.Departments.AsNoTracking().ToDictionaryAsync(d => d.Id, d => d.Name);

            var items = new List<PayrollReportItemDto>();
            decimal totalBasic = 0;
            decimal totalGross = 0;
            decimal totalDeductions = 0;
            decimal totalNet = 0;
            string periodName = list.FirstOrDefault()?.PayrollRun?.PayrollPeriod?.Name ?? "Selected Period";

            foreach (var p in list)
            {
                var emp = p.Employee;
                string branchName = "-";
                if (emp != null && branches.TryGetValue((int)emp.BranchId, out var bn))
                {
                    branchName = bn;
                }

                string deptName = "-";
                if (emp != null && emp.DepartmentId.HasValue && departments.TryGetValue(emp.DepartmentId.Value, out var dn))
                {
                    deptName = dn;
                }

                totalBasic += p.BaseSalary;
                totalGross += p.GrossEarnings;
                totalDeductions += p.TotalDeductions;
                totalNet += p.NetSalary;

                items.Add(new PayrollReportItemDto
                {
                    PayslipId = p.Id,
                    EmployeeCode = emp?.EmployeeCode ?? "",
                    EmployeeName = $"{emp?.FirstName} {emp?.LastName}".Trim(),
                    BranchName = branchName,
                    DepartmentName = deptName,
                    BaseSalary = p.BaseSalary,
                    GrossEarnings = p.GrossEarnings,
                    TotalDeductions = p.TotalDeductions,
                    NetSalary = p.NetSalary,
                    PaymentStatus = p.PaymentStatus
                });
            }

            // Sri Lankan statutory employer costs (12% EPF, 3% ETF on gross)
            decimal totalEpfEmployer = Math.Round(totalGross * 0.12m, 2);
            decimal totalEtfEmployer = Math.Round(totalGross * 0.03m, 2);
            decimal totalCompanyCost = totalGross + totalEpfEmployer + totalEtfEmployer;

            var summary = new PayrollReportSummaryDto
            {
                PeriodName = periodName,
                EmployeeCount = list.Count,
                TotalBasicSalary = Math.Round(totalBasic, 2),
                TotalGrossSalary = Math.Round(totalGross, 2),
                TotalDeductions = Math.Round(totalDeductions, 2),
                TotalNetSalary = Math.Round(totalNet, 2),
                TotalEPFEmployer = totalEpfEmployer,
                TotalETFEmployer = totalEtfEmployer,
                TotalCompanyCost = Math.Round(totalCompanyCost, 2)
            };

            return new PayrollReportResponseDto
            {
                Summary = summary,
                Items = items
            };
        }

        public async Task<HeadcountReportDto> GetHeadcountReportAsync(int? branchId = null)
        {
            var empQuery = _context.Employees
                .AsNoTracking()
                .Where(e => !e.IsDeleted);

            if (branchId.HasValue && branchId.Value > 0)
            {
                empQuery = empQuery.Where(e => e.BranchId == branchId.Value);
            }

            var employees = await empQuery.ToListAsync();
            var total = employees.Count;
            var active = employees.Count(e => e.IsActive);
            var inactive = total - active;

            var branches = await _context.Branches.AsNoTracking().ToDictionaryAsync(b => b.branchid, b => b.branchname);
            var departments = await _context.Departments.AsNoTracking().ToDictionaryAsync(d => d.Id, d => d.Name);
            var designations = await _context.Designations.AsNoTracking().ToDictionaryAsync(d => d.Id, d => d.Name);
            var empTypes = await _context.EmploymentTypes.AsNoTracking().ToDictionaryAsync(t => t.Id, t => t.Name);

            // Group by Branch
            var branchDist = employees
                .GroupBy(e => e.BranchId)
                .Select(g =>
                {
                    var name = branches.TryGetValue((int)g.Key, out var bn) ? bn : "Unassigned";
                    var count = g.Count();
                    var pct = total > 0 ? Math.Round(((double)count / total) * 100.0, 1) : 0;
                    return new DistributionItemDto { Name = name, Count = count, Percentage = pct };
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Group by Department
            var deptDist = employees
                .GroupBy(e => e.DepartmentId)
                .Select(g =>
                {
                    var name = (g.Key.HasValue && departments.TryGetValue(g.Key.Value, out var dn)) ? dn : "Unassigned";
                    var count = g.Count();
                    var pct = total > 0 ? Math.Round(((double)count / total) * 100.0, 1) : 0;
                    return new DistributionItemDto { Name = name, Count = count, Percentage = pct };
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Group by Designation
            var desigDist = employees
                .GroupBy(e => e.DesignationId)
                .Select(g =>
                {
                    var name = (g.Key.HasValue && designations.TryGetValue(g.Key.Value, out var dn)) ? dn : "Unassigned";
                    var count = g.Count();
                    var pct = total > 0 ? Math.Round(((double)count / total) * 100.0, 1) : 0;
                    return new DistributionItemDto { Name = name, Count = count, Percentage = pct };
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Group by Employment Type
            var typeDist = employees
                .GroupBy(e => e.EmploymentTypeId)
                .Select(g =>
                {
                    var name = (g.Key.HasValue && empTypes.TryGetValue(g.Key.Value, out var tn)) ? tn : "Permanent";
                    var count = g.Count();
                    var pct = total > 0 ? Math.Round(((double)count / total) * 100.0, 1) : 0;
                    return new DistributionItemDto { Name = name, Count = count, Percentage = pct };
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new HeadcountReportDto
            {
                TotalEmployees = total,
                ActiveEmployees = active,
                InactiveEmployees = inactive,
                BranchDistribution = branchDist,
                DepartmentDistribution = deptDist,
                DesignationDistribution = desigDist,
                EmploymentTypeDistribution = typeDist
            };
        }
    }
}
