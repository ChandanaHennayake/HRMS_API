using HRMS.API.Entities;

namespace HRMS.API.Interfaces.Repositories
{
    public interface IBranchRepository
    {
        Task<Branch> CreateAsync(Branch branch);

        Task<IEnumerable<Branch>> GetAllAsync();

        Task<Branch?> GetByIdAsync(int id);

        Task<Branch?> GetEntityByIdAsync(int id);

        Task<bool> BranchCodeExistsAsync(
            int companyId,
            string branchCode,
            int? excludeId = null);

        Task UpdateAsync(Branch branch);

        Task DeleteAsync(Branch branch);
    }
}