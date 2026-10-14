using HRMS.API.Data;
using HRMS.API.DTOs.Lookup;
using HRMS.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    /// <summary>
    /// Read-only queries for master lookup tables.
    /// Only active, non-deleted records are returned.
    /// </summary>
    public class LookupRepository : ILookupRepository
    {
        private readonly DefaultContext _context;

        public LookupRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<List<LookupDto>> GetDepartmentsAsync(long? companyId)
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Where(x => !companyId.HasValue || x.CompanyId == companyId.Value)
                .OrderBy(x => x.Name)
                .Select(x => new LookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    CompanyId = x.CompanyId
                })
                .ToListAsync();
        }

        public async Task<List<LookupDto>> GetDesignationsAsync(long? companyId)
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Where(x => !companyId.HasValue || x.CompanyId == companyId.Value)
                .OrderBy(x => x.Name)
                .Select(x => new LookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    CompanyId = x.CompanyId
                })
                .ToListAsync();
        }

        public async Task<List<LookupDto>> GetEmploymentTypesAsync()
        {
            return await _context.EmploymentTypes
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new LookupDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        public async Task<List<LookupDto>> GetRolesAsync(long? companyId)
        {
            // Roles with a null company are global/system roles.
            return await _context.Roles
                .AsNoTracking()
                .Where(x => x.isactive != false)
                .Where(x => !companyId.HasValue
                    || x.companyid == null
                    || x.companyid == companyId.Value)
                .OrderBy(x => x.roleid)
                .Select(x => new LookupDto
                {
                    Id = x.roleid,
                    Name = x.rolename,
                    CompanyId = x.companyid
                })
                .ToListAsync();
        }

        public async Task<List<BranchLookupDto>> GetBranchesAsync(long? companyId)
        {
            return await _context.Branches
                .AsNoTracking()
                .Where(x => x.isactive != false)
                .Where(x => !companyId.HasValue || x.companyid == companyId.Value)
                .OrderBy(x => x.branchname)
                .Select(x => new BranchLookupDto
                {
                    Id = x.branchid,
                    Name = x.branchname,
                    Code = x.branchcode,
                    CompanyId = x.companyid,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    GeofenceRadiusMeters = x.GeofenceRadiusMeters
                })
                .ToListAsync();
        }
    }
}
