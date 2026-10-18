using HRMS.API.DTOs.User;
using HRMS.API.Entities;
using HRMS.API.Repository.UserDetails;

namespace HRMS.API.Service.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<int> CreateAsync(CreateUserRequest request)
        {
            var username = await _userRepository.GetByUsernameAsync(request.Username);

            if (username != null)
                throw new Exception("Username already exists.");

            var email = await _userRepository.GetByEmailAsync(request.Email);

            if (email != null)
                throw new Exception("Email already exists.");

            var user = new AppUser
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                RoleId = request.RoleId,
                EmployeeId = request.EmployeeId,

                Username = request.Username,
                Email = request.Email,

                // Hash password
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

                // BCrypt already stores the salt in the hash
                PasswordSalt = null,

                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,

                IsActive = true,
                IsLocked = false,
                FailedLoginAttempts = 0,

                LockoutEnd = null,
                LastLogin = null,

                PasswordChangedDate = DateTime.Now,

                CreatedDate = DateTime.Now,
                CreatedBy = 1,      // Replace with logged-in user later

                ModifiedDate = null,
                ModifiedBy = null
            };

            return await _userRepository.CreateAsync(user);
        }




        public async Task<List<UserResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(x => new UserResponse
            {
                UserId = x.UserId,
                Username = x.Username,
                Email = x.Email,
                FullName = $"{x.FirstName} {x.LastName}".Trim(),
                Role = !string.IsNullOrWhiteSpace(x.Role?.rolename)
                    ? x.Role.rolename
                    : (x.RoleId switch
                    {
                        1 => "Admin",
                        2 => "Manager",
                        3 => "HR",
                        4 => "Employee",
                        _ => "Employee"
                    }),
                RoleId = x.RoleId,
                IsActive = x.IsActive ?? false
            }).ToList();
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return null;

            return new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Role = !string.IsNullOrWhiteSpace(user.Role?.rolename)
                    ? user.Role.rolename
                    : (user.RoleId switch
                    {
                        1 => "Admin",
                        2 => "Manager",
                        3 => "HR",
                        4 => "Employee",
                        _ => "Employee"
                    }),
                RoleId = user.RoleId,
                IsActive = user.IsActive ?? false
            };
        }

        public async Task<UserProfileResponse?> GetProfileAsync(int userId)
        {
            return await _userRepository.GetProfileAsync(userId);
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                throw new Exception("Current password is required.");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new Exception("New password is required.");

            if (request.NewPassword.Length < 6)
                throw new Exception("New password must be at least 6 characters long.");

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("User not found.");

            // Validate current password against stored BCrypt hash
            bool isValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isValid)
                throw new Exception("Current password is incorrect.");

            // Update to new password hash
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordSalt = null;
            user.PasswordChangedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = user.UserId;

            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("User not found.");

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);

            if (existingEmail != null && existingEmail.UserId != id)
                throw new Exception("Email already exists.");

            user.CompanyId = request.CompanyId;
            user.BranchId = request.BranchId;
            user.RoleId = request.RoleId;
            user.EmployeeId = request.EmployeeId;

            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;

            user.IsActive = request.IsActive;
            user.IsLocked = request.IsLocked;

            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = 1; // Replace with logged-in user later

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("User not found.");

            // Soft delete (deactivate)
            user.IsActive = false;
            user.IsLocked = true;
            user.ModifiedDate = DateTime.Now;

            await _userRepository.UpdateAsync(user);
        }
    }
}