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
    }
}