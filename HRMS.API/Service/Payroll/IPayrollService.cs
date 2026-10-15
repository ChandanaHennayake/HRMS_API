using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.DTOs.Payroll;

namespace HRMS.API.Service.Payroll
{
    public interface IPayrollService
    {
        Task<IEnumerable<PayrollPeriodDto>> GetPeriodsAsync(long companyId, int? year);
        Task<PayrollRunDto> ProcessPayrollAsync(ProcessPayrollRequestDto request, long processedBy);
        Task<IEnumerable<PayrollRunDto>> GetRunsAsync(long companyId, int? branchId);
        Task<PayrollRunDto?> GetRunDetailsAsync(long runId);
        Task<PayrollRunDto> UpdateRunStatusAsync(long runId, short newStatus, long reviewerId, string? comment);
        Task<IEnumerable<PayslipSummaryDto>> GetEmployeePayslipsAsync(long employeeId);
        Task<PayslipDto?> GetPayslipDetailsAsync(long payslipId);
        Task<EmployeePayrollPreviewDto> GetEmployeePayrollPreviewAsync(long employeeId, int year, int month, System.DateOnly? startDate = null, System.DateOnly? endDate = null);
        Task<PayslipDto> SaveIndividualPayrollAsync(IndividualPayrollRequestDto request, long savedBy);
        Task DeletePayslipAsync(long payslipId, long deletedBy);
    }
}
