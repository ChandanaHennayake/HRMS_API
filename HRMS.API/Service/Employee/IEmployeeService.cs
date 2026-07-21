using HRMS.API.DTOs.Employee;

namespace HRMS.API.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> CreateAsync(
            CreateEmployeeDto request);

        Task<IEnumerable<EmployeeDto>>
            GetAllAsync();

        Task<EmployeeDto?> GetByIdAsync(
            long id);

        Task<EmployeeDto?> UpdateAsync(
            long id,
            UpdateEmployeeDto request);

        Task<bool> DeleteAsync(
            long id,
            long? deletedBy);
    }
}