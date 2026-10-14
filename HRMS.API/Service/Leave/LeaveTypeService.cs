using HRMS.API.Entities;
using HRMS.API.Repository.Leave;

namespace HRMS.API.Service.Leave
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly ILeaveTypeRepository _repository;

        public LeaveTypeService(
            ILeaveTypeRepository repository)
        {
            _repository = repository;
        }

        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<LeaveType>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // =====================================================
        // GET ACTIVE
        // =====================================================

        public async Task<IEnumerable<LeaveType>> GetActiveAsync()
        {
            return await _repository.GetActiveAsync();
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<LeaveType?> GetByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        // =====================================================
        // CREATE
        // =====================================================

        public async Task<LeaveType> CreateAsync(
            LeaveType leaveType)
        {
            return await _repository.CreateAsync(
                leaveType);
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public async Task UpdateAsync(
            LeaveType leaveType)
        {
            await _repository.UpdateAsync(
                leaveType);
        }
    }
}