using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.DTOs.Salary;
using HRMS.API.Entities;

namespace HRMS.API.Service.Salary
{
    public interface ISalaryService
    {
        SalaryCalculationResultDto CalculateSalary(SalaryCalculationRequestDto request);
        Task<EmployeeSalaryDto?> GetEmployeeSalaryAsync(long employeeId);
        Task<EmployeeSalaryDto> SaveEmployeeSalaryAsync(SaveEmployeeSalaryDto request);
        Task<IEnumerable<SalaryComponentDto>> GetComponentsAsync(long companyId);
        Task<SalaryComponentDto> CreateComponentAsync(SalaryComponentDto request);
        Task<IEnumerable<SalaryStructure>> GetStructuresAsync(long companyId);
    }
}
