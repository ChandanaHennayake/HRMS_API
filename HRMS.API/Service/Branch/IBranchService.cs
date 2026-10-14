using HRMS.API.DTOs.Branch;

namespace HRMS.API.Service.Branch
{
    public interface IBranchService
    {
        Task<BranchDto> CreateAsync(
            CreateBranchDto request);

        Task<IEnumerable<BranchDto>> GetAllAsync();

        Task<BranchDto?> GetByIdAsync(int id);

        Task<BranchDto> UpdateAsync(
            int id,
            UpdateBranchDto request);

        Task DeleteAsync(int id);

        Task<IEnumerable<BranchManagerDto>> GetManagersByBranchAsync(int branchId);

        Task<IEnumerable<BranchDto>> GetBranchesByManagerAsync(int userId);

        Task<BranchManagerDto> AssignManagerAsync(AssignManagerDto request);

        Task RemoveManagerAsync(int userId, int branchId);
    }
}