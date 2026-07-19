using HRMS.API.DTOs.Authentication;
using HRMS.API.Repository.UserDetails;

namespace HRMS.API.Service.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository
                .GetUserForLoginAsync(request.Username);

            if (user == null)
                throw new Exception("Invalid username or password.");

            if (user.IsActive != true)
                throw new Exception("User account is inactive.");

            if (user.IsLocked == true)
            {
                if (user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value <= DateTime.Now)
                {
                    // Lockout period finished
                    user.IsLocked = false;
                    user.LockoutEnd = null;
                    user.FailedLoginAttempts = 0;

                    await _userRepository.UpdateAsync(user);
                }
                else
                {
                    throw new Exception("User account is locked.");
                }
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

            if (!passwordValid)
            {
                user.FailedLoginAttempts =
                    (user.FailedLoginAttempts ?? 0) + 1;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;

                    // Lock for 15 minutes
                    user.LockoutEnd = DateTime.Now.AddMinutes(15);
                }

                await _userRepository.UpdateAsync(user);

                throw new Exception("Invalid username or password.");
            }

            // Successful login
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockoutEnd = null;
            user.LastLogin = DateTime.Now;

            await _userRepository.UpdateAsync(user);

            var token = _tokenService.GenerateToken(user);

            var durationInMinutes = int.Parse(
                _configuration["Jwt:DurationInMinutes"] ?? "120");

            return new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,

                FullName =
                    $"{user.FirstName} {user.LastName}".Trim(),

                RoleId = user.RoleId,
                Role = user.Role?.Rolename ?? string.Empty,

                CompanyId = user.CompanyId,
                BranchId = user.BranchId,

                Token = token,

                Expiration =
                    DateTime.UtcNow.AddMinutes(durationInMinutes)
            };
        }
    }
}