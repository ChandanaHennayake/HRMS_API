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
    }
}