using HRMS.API.DTOs.Employee;
using HRMS.API.Entities;

namespace HRMS.API.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee> CreateAsync(Employee employee);

        Task<IEnumerable<EmployeeDto>> GetAllAsync();

        Task<EmployeeDto?> GetByIdAsync(long id);

        Task<Employee?> GetEntityByIdAsync(long id);
       
        Task<IEnumerable<Employee>>
           GetAllActiveEntitiesAsync();

        Task<bool> EmployeeCodeExistsAsync(
            long companyId,
            string employeeCode,
            long? excludeId = null);

        Task UpdateAsync(Employee employee);

        Task DeleteAsync(Employee employee);
    }
}