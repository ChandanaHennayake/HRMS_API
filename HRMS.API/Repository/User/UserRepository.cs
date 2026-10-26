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
                .Include(x => x.Role)
                .Include(x => x.Branch)
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
                    .FirstOrDefaultAsync(x => x.Id == user.EmployeeId.Value && !x.IsDeleted);
            }

            // Auto-link employee by email or employee code if not explicitly linked on user
            if (employee == null && !string.IsNullOrWhiteSpace(user.Email))
            {
                employee = await _context.Employees
                    .Include(x => x.Department)
                    .Include(x => x.Designation)
                    .Include(x => x.EmploymentType)
                    .FirstOrDefaultAsync(x => x.Email == user.Email && !x.IsDeleted);

                if (employee != null)
                {
                    user.EmployeeId = (int)employee.Id;
                    try
                    {
                        await UpdateAsync(user);
                    }
                    catch
                    {
                        // Best effort auto-linking
                    }
                }
            }

            // Fallback branch resolution from employee if user branch is null
            var branch = user.Branch;
            if (branch == null && employee != null && employee.BranchId > 0)
            {
                branch = await _context.Branches.FirstOrDefaultAsync(b => b.branchid == employee.BranchId);
            }

            // Fallback role resolution
            var roleName = !string.IsNullOrWhiteSpace(user.Role?.rolename)
                ? user.Role.rolename
                : (user.RoleId switch
                {
                    1 => "Admin",
                    2 => "Manager",
                    3 => "HR",
                    4 => "Employee",
                    _ => "Employee"
                });

            bool hasPending = employee != null && await _context.ProfileChangeRequests.AnyAsync(r => r.EmployeeId == employee.Id && r.Status == 0);

            return new UserProfileResponse
            {
                UserId = user.UserId,
                EmployeeId = user.EmployeeId ?? (employee != null ? (int)employee.Id : null),
                Username = user.Username,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Phone = user.Phone ?? employee?.Phone,
                Role = roleName,
                RoleId = user.RoleId,
                FirstName = employee?.FirstName ?? user.FirstName,
                LastName = employee?.LastName ?? user.LastName,
                EmployeeCode = employee?.EmployeeCode,
                DepartmentName = employee?.Department?.Name,
                DesignationName = employee?.Designation?.Name,
                EmploymentTypeName = employee?.EmploymentType?.Name,
                JoinedDate = employee?.JoinedDate,
                DateOfBirth = employee?.DateOfBirth,
                Gender = employee?.Gender,
                AddressLine1 = employee?.AddressLine1,
                AddressLine2 = employee?.AddressLine2,
                City = employee?.City,
                Qualification = employee?.Qualification,
                BankName = employee?.BankName,
                BankAccountNo = employee?.BankAccountNo,
                BankBranch = employee?.BankBranch,
                HasPendingChangeRequest = hasPending,
                BranchId = user.BranchId ?? (int?)employee?.BranchId,
                BranchName = branch?.branchname,
                BranchLatitude = branch?.Latitude,
                BranchLongitude = branch?.Longitude,
                GeofenceRadiusMeters = branch?.GeofenceRadiusMeters
            };
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            return await _context.AppUsers
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            return await _context.AppUsers
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            return await _context.AppUsers
                .Include(x => x.Role)
                .OrderBy(x => x.FirstName)
                .ToListAsync();
        }

        public async Task UpdateAsync(AppUser user)
        {
            var entry = _context.Entry(user);
            if (entry.State == EntityState.Detached)
            {
                _context.AppUsers.Attach(user);
                entry.State = EntityState.Modified;
            }
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