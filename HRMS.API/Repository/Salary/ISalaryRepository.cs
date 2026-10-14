using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.Entities;

namespace HRMS.API.Repository.Salary
{
    public interface ISalaryRepository
    {
        Task<IEnumerable<SalaryComponent>> GetComponentsAsync(long companyId);
        Task<SalaryComponent?> GetComponentByIdAsync(long id);
        Task<SalaryComponent> CreateComponentAsync(SalaryComponent component);

        Task<IEnumerable<SalaryStructure>> GetStructuresAsync(long companyId);
        Task<SalaryStructure?> GetStructureByIdAsync(long id);
        Task<SalaryStructure> CreateStructureAsync(SalaryStructure structure);

        Task<EmployeeSalary?> GetEmployeeSalaryAsync(long employeeId);
        Task<EmployeeSalary> SaveEmployeeSalaryAsync(EmployeeSalary salary);

        Task EnsureDefaultComponentsAsync(long companyId);
    }
}
