using HRMS.API.DTOs.Lookup;

namespace HRMS.API.Interfaces.Services
{
    public interface ILookupService
    {
        Task<List<LookupDto>> GetDepartmentsAsync(long? companyId);
        Task<List<LookupDto>> GetDesignationsAsync(long? companyId);
        Task<List<LookupDto>> GetEmploymentTypesAsync();
        Task<List<LookupDto>> GetRolesAsync(long? companyId);
        Task<List<BranchLookupDto>> GetBranchesAsync(long? companyId);
    }
}
