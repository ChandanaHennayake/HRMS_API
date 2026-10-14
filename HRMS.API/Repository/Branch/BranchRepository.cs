using HRMS.API.Data;
using HRMS.API.Entities;
using HRMS.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly DefaultContext _context;

        public BranchRepository(DefaultContext context)
        {
            _context = context;
        }


        // =====================================================
        // CREATE
        // =====================================================

        public async Task<Branch> CreateAsync(Branch branch)
        {
            await _context.Branches.AddAsync(branch);

            await _context.SaveChangesAsync();

            return branch;
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<Branch>> GetAllAsync()
        {
            return await _context.Branches
                .AsNoTracking()
                .OrderByDescending(x => x.branchid)
                .ToListAsync();
        }


        // =====================================================
        // GET BY ID - READ ONLY
        // =====================================================

        public async Task<Branch?> GetByIdAsync(int id)
        {
            return await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.branchid == id);
        }


        // =====================================================
        // GET ENTITY BY ID - UPDATE / DELETE
        // =====================================================

        public async Task<Branch?> GetEntityByIdAsync(int id)
        {
            return await _context.Branches
                .FirstOrDefaultAsync(x =>
                    x.branchid == id);
        }


        // =====================================================
        // CHECK BRANCH CODE
        // =====================================================

        public async Task<bool> BranchCodeExistsAsync(
            int companyId,
            string branchCode,
            int? excludeId = null)
        {
            return await _context.Branches
                .AnyAsync(x =>
                    x.companyid == companyId &&
                    x.branchcode == branchCode &&
                    (
                        !excludeId.HasValue ||
                        x.branchid != excludeId.Value
                    ));
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task UpdateAsync(Branch branch)
        {
            _context.Branches.Update(branch);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // DELETE / DEACTIVATE
        // =====================================================

        public async Task DeleteAsync(Branch branch)
        {
            branch.isactive = false;

            _context.Branches.Update(branch);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // MANAGER ASSIGNMENTS
        // =====================================================

        public async Task<IEnumerable<UserBranch>> GetManagersByBranchAsync(int branchId)
        {
            return await _context.UserBranches
                .AsNoTracking()
                .Include(ub => ub.user)
                .Include(ub => ub.branch)
                .Where(ub => ub.branchid == branchId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserBranch>> GetBranchesByManagerAsync(int userId)
        {
            return await _context.UserBranches
                .AsNoTracking()
                .Include(ub => ub.user)
                .Include(ub => ub.branch)
                .Where(ub => ub.userid == userId)
                .ToListAsync();
        }

        public async Task<UserBranch?> GetUserBranchAsync(int userId, int branchId)
        {
            return await _context.UserBranches
                .FirstOrDefaultAsync(ub => ub.userid == userId && ub.branchid == branchId);
        }

        public async Task<UserBranch> AssignManagerAsync(UserBranch userBranch)
        {
            await _context.UserBranches.AddAsync(userBranch);
            await _context.SaveChangesAsync();
            return userBranch;
        }

        public async Task RemoveManagerAsync(UserBranch userBranch)
        {
            _context.UserBranches.Remove(userBranch);
            await _context.SaveChangesAsync();
        }
    }
}