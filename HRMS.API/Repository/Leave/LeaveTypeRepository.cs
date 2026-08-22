using HRMS.API.Data;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Leave
{
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly DefaultContext _context;

        public LeaveTypeRepository(DefaultContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<LeaveType>> GetAllAsync()
        {
            return await _context.LeaveTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<LeaveType?> GetByIdAsync(long id)
        {
            return await _context.LeaveTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // =====================================================
        // GET ACTIVE
        // =====================================================

        public async Task<IEnumerable<LeaveType>> GetActiveAsync()
        {
            return await _context.LeaveTypes
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        // =====================================================
        // CREATE
        // =====================================================

        public async Task<LeaveType> CreateAsync(
            LeaveType leaveType)
        {
            await _context.LeaveTypes.AddAsync(
                leaveType);

            await _context.SaveChangesAsync();

            return leaveType;
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public async Task UpdateAsync(
            LeaveType leaveType)
        {
            _context.LeaveTypes.Update(
                leaveType);

            await _context.SaveChangesAsync();
        }
    }
}