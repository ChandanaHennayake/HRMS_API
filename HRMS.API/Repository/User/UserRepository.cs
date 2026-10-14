using HRMS.API.Data;
using HRMS.API.DTOs.User;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.UserDetails
{
    public class UserRepository : IUserRepository
    {
        private readonly DefaultContext _context;

        public UserRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(AppUser user)
        {
            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.UserId;
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<UserProfileResponse?> GetProfileAsync(int userId)
        {
            var user = await _context.AppUsers
                .Include(x => x.Role)
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return null;

            Employee? employee = null;
            if (user.EmployeeId.HasValue && user.EmployeeId.Value > 0)
            {
                employee = await _context.Employees
                    .Include(x => x.Department)
                    .Include(x => x.Designation)
                    .Include(x => x.EmploymentType)
                    .FirstOrDefaultAsync(x => x.Id == user.EmployeeId.Value);
            }

            return new UserProfileResponse
            {
                UserId = user.UserId,
                EmployeeId = user.EmployeeId,
                Username = user.Username,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Phone = user.Phone ?? employee?.Phone,
                Role = user.Role?.rolename ?? string.Empty,
                RoleId = user.RoleId,
                EmployeeCode = employee?.EmployeeCode,
                DepartmentName = employee?.Department?.Name,
                DesignationName = employee?.Designation?.Name,
                EmploymentTypeName = employee?.EmploymentType?.Name,
                JoinedDate = employee?.JoinedDate,
                BranchId = user.BranchId,
                BranchName = user.Branch?.branchname,
                BranchLatitude = user.Branch?.Latitude,
                BranchLongitude = user.Branch?.Longitude,
                GeofenceRadiusMeters = user.Branch?.GeofenceRadiusMeters
            };
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            return await _context.AppUsers
                .OrderBy(x => x.FirstName)
                .ToListAsync();
        }

        public async Task UpdateAsync(AppUser user)
        {
            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();
        }


        public async Task<AppUser?> GetUserForLoginAsync(string username)
        {
            return await _context.AppUsers
                .Include(x => x.Role)
                .Include(x => x.Company)
                .Include(x => x.Branch)
               
                .FirstOrDefaultAsync(x =>
                    x.Username == username);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}