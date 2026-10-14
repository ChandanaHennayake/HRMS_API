using HRMS.API.DTOs.Lookup;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Interfaces.Services;

namespace HRMS.API.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _repository;

        public LookupService(ILookupRepository repository)
        {
            _repository = repository;
        }

        public Task<List<LookupDto>> GetDepartmentsAsync(long? companyId)
            => _repository.GetDepartmentsAsync(companyId);

        public Task<List<LookupDto>> GetDesignationsAsync(long? companyId)
            => _repository.GetDesignationsAsync(companyId);

        public Task<List<LookupDto>> GetEmploymentTypesAsync()
            => _repository.GetEmploymentTypesAsync();

        public Task<List<LookupDto>> GetRolesAsync(long? companyId)
            => _repository.GetRolesAsync(companyId);

        public Task<List<BranchLookupDto>> GetBranchesAsync(long? companyId)
            => _repository.GetBranchesAsync(companyId);
    }
}
