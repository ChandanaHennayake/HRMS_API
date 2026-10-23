using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Payroll
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly DefaultContext _context;

        public PayrollRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<PayrollPeriod?> GetPeriodAsync(long companyId, int year, int month)
        {
            return await _context.PayrollPeriods
                .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Year == year && p.Month == month);
        }

        public async Task<PayrollPeriod> CreatePeriodAsync(PayrollPeriod period)
        {
            await _context.PayrollPeriods.AddAsync(period);
            await _context.SaveChangesAsync();
            return period;
        }

        public async Task<IEnumerable<PayrollPeriod>> GetPeriodsAsync(long companyId, int? year)
        {
            var query = _context.PayrollPeriods
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId);

            if (year.HasValue)
            {
                query = query.Where(p => p.Year == year.Value);
            }

            return await query
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();
        }

        public async Task<PayrollRun> CreateRunAsync(PayrollRun run)
        {
            await _context.PayrollRuns.AddAsync(run);
            await _context.SaveChangesAsync();
            return run;
        }

        public async Task<PayrollRun?> GetRunByIdAsync(long id)
        {
            return await _context.PayrollRuns
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Branch)
                .Include(r => r.Payslips)
                    .ThenInclude(p => p.Employee)
                        .ThenInclude(e => e.Department)
                .Include(r => r.Payslips)
                    .ThenInclude(p => p.Employee)
                        .ThenInclude(e => e.Designation)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<PayrollRun>> GetRunsAsync(long companyId, int? branchId)
        {
            var query = _context.PayrollRuns
                .AsNoTracking()
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Branch)
                .Where(r => r.CompanyId == companyId);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(r => r.BranchId == branchId.Value);
            }

            return await query
                .OrderByDescending(r => r.ProcessedAt)
                .ToListAsync();
        }

        public async Task UpdateRunAsync(PayrollRun run)
        {
            _context.PayrollRuns.Update(run);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Payslip>> GetEmployeePayslipsAsync(long employeeId)
        {
            return await _context.Payslips
                .AsNoTracking()
                .Include(p => p.PayrollPeriod)
                .Include(p => p.PayrollRun)
                .Where(p => p.EmployeeId == employeeId && p.PayrollRun.Status == 3) // Only published
                .OrderByDescending(p => p.PayrollPeriod.Year)
                .ThenByDescending(p => p.PayrollPeriod.Month)
                .ToListAsync();
        }

        public async Task<Payslip?> GetPayslipByIdAsync(long id)
        {
            try
            {
                return await _context.Payslips
                    .Include(p => p.PayrollPeriod)
                    .Include(p => p.PayrollRun)
                    .Include(p => p.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(p => p.Employee)
                        .ThenInclude(e => e.Designation)
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception)
            {
                return await _context.Payslips
                    .Include(p => p.PayrollPeriod)
                    .Include(p => p.PayrollRun)
                    .Include(p => p.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(p => p.Employee)
                        .ThenInclude(e => e.Designation)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
        }
    }
}
