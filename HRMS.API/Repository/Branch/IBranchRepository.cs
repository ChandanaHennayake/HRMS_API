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

        Task<IEnumerable<UserBranch>> GetManagersByBranchAsync(int branchId);

        Task<IEnumerable<UserBranch>> GetBranchesByManagerAsync(int userId);

        Task<UserBranch?> GetUserBranchAsync(int userId, int branchId);

        Task<UserBranch> AssignManagerAsync(UserBranch userBranch);

        Task RemoveManagerAsync(UserBranch userBranch);
    }
}