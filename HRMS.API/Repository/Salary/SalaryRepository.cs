using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Salary
{
    public class SalaryRepository : ISalaryRepository
    {
        private readonly DefaultContext _context;

        public SalaryRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SalaryComponent>> GetComponentsAsync(long companyId)
        {
            return await _context.SalaryComponents
                .AsNoTracking()
                .Where(c => (c.CompanyId == companyId || c.CompanyId == 0) && c.IsActive)
                .OrderBy(c => c.ComponentType)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<SalaryComponent?> GetComponentByIdAsync(long id)
        {
            return await _context.SalaryComponents.FindAsync(id);
        }

        public async Task<SalaryComponent> CreateComponentAsync(SalaryComponent component)
        {
            await _context.SalaryComponents.AddAsync(component);
            await _context.SaveChangesAsync();
            return component;
        }

        public async Task<IEnumerable<SalaryStructure>> GetStructuresAsync(long companyId)
        {
            return await _context.SalaryStructures
                .AsNoTracking()
                .Include(s => s.StructureComponents)
                .ThenInclude(sc => sc.SalaryComponent)
                .Where(s => (s.CompanyId == companyId || s.CompanyId == 0) && s.IsActive)
                .ToListAsync();
        }

        public async Task<SalaryStructure?> GetStructureByIdAsync(long id)
        {
            return await _context.SalaryStructures
                .Include(s => s.StructureComponents)
                .ThenInclude(sc => sc.SalaryComponent)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SalaryStructure> CreateStructureAsync(SalaryStructure structure)
        {
            await _context.SalaryStructures.AddAsync(structure);
            await _context.SaveChangesAsync();
            return structure;
        }

        public async Task<EmployeeSalary?> GetEmployeeSalaryAsync(long employeeId)
        {
            return await _context.EmployeeSalaries
                .Include(es => es.SalaryStructure)
                .Include(es => es.Components)
                .ThenInclude(esc => esc.SalaryComponent)
                .Where(es => es.EmployeeId == employeeId && es.IsActive)
                .OrderByDescending(es => es.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<EmployeeSalary> SaveEmployeeSalaryAsync(EmployeeSalary salary)
        {
            var existing = await _context.EmployeeSalaries
                .Include(es => es.Components)
                .FirstOrDefaultAsync(es => es.EmployeeId == salary.EmployeeId && es.IsActive);

            if (existing != null)
            {
                existing.IsActive = false;
                existing.EffectiveTo = salary.EffectiveFrom.AddDays(-1);
                existing.UpdatedAt = DateTime.Now;
                _context.EmployeeSalaries.Update(existing);
            }

            salary.CreatedAt = DateTime.Now;
            salary.IsActive = true;
            await _context.EmployeeSalaries.AddAsync(salary);
            await _context.SaveChangesAsync();
            return salary;
        }

        public async Task EnsureDefaultComponentsAsync(long companyId)
        {
            var exists = await _context.SalaryComponents.AnyAsync(c => c.CompanyId == companyId);
            if (exists) return;

            var defaultComponents = new List<SalaryComponent>
            {
                new() { CompanyId = companyId, Name = "Basic Salary", Code = "BASIC", ComponentType = 1, IsEpfApplicable = true, IsTaxable = true, IsFixed = true, IsActive = true },
                new() { CompanyId = companyId, Name = "Budgetary Relief Allowance", Code = "BRA", ComponentType = 1, IsEpfApplicable = true, IsTaxable = true, IsFixed = true, IsActive = true },
                new() { CompanyId = companyId, Name = "Transport Allowance", Code = "TRA_ALLW", ComponentType = 1, IsEpfApplicable = false, IsTaxable = true, IsFixed = true, IsActive = true },
                new() { CompanyId = companyId, Name = "Attendance Allowance", Code = "ATT_ALLW", ComponentType = 1, IsEpfApplicable = false, IsTaxable = true, IsFixed = true, IsActive = true },
                new() { CompanyId = companyId, Name = "Performance Allowance", Code = "PERF_ALLW", ComponentType = 1, IsEpfApplicable = false, IsTaxable = true, IsFixed = false, IsActive = true },
                new() { CompanyId = companyId, Name = "Staff Loan Deduction", Code = "LOAN_DED", ComponentType = 2, IsEpfApplicable = false, IsTaxable = false, IsFixed = true, IsActive = true },
                new() { CompanyId = companyId, Name = "Salary Advance", Code = "SAL_ADV", ComponentType = 2, IsEpfApplicable = false, IsTaxable = false, IsFixed = false, IsActive = true },
            };

            await _context.SalaryComponents.AddRangeAsync(defaultComponents);
            await _context.SaveChangesAsync();
        }
    }
}
