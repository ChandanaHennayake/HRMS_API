using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.Entities;

namespace HRMS.API.Repository.Payroll
{
    public interface IPayrollRepository
    {
        Task<PayrollPeriod?> GetPeriodAsync(long companyId, int year, int month);
        Task<PayrollPeriod> CreatePeriodAsync(PayrollPeriod period);
        Task<IEnumerable<PayrollPeriod>> GetPeriodsAsync(long companyId, int? year);

        Task<PayrollRun> CreateRunAsync(PayrollRun run);
        Task<PayrollRun?> GetRunByIdAsync(long id);
        Task<IEnumerable<PayrollRun>> GetRunsAsync(long companyId, int? branchId);
        Task UpdateRunAsync(PayrollRun run);

        Task<IEnumerable<Payslip>> GetEmployeePayslipsAsync(long employeeId);
        Task<Payslip?> GetPayslipByIdAsync(long id);
    }
}
