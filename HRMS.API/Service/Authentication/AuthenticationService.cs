using System.Security.Cryptography;
using HRMS.API.Data;
using HRMS.API.DTOs.Authentication;
using HRMS.API.Entities;
using HRMS.API.Repository.UserDetails;
using HRMS.API.Service.Audit;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Service.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly DefaultContext _context;
        private readonly IAuditService _auditService;

        public AuthenticationService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IConfiguration configuration,
            DefaultContext context,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _context = context;
            _auditService = auditService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository
                .GetUserForLoginAsync(request.Username);

            if (user == null)
            {
                await _auditService.LogSecurityEventAsync(null, request.Username, "LoginFailed", $"Failed login attempt: User '{request.Username}' not found.", null, false);
                throw new Exception("Invalid username or password.");
            }

            if (user.IsActive != true || user.IsDeleted == true)
            {
                await _auditService.LogSecurityEventAsync(user.UserId, request.Username, "LoginBlocked", $"Login blocked: User '{request.Username}' account is inactive or deleted.", null, false);
                throw new Exception("This account has been deleted or deactivated. Access is denied. Please contact the administrator.");
            }

            // Verify if linked employee record is deleted or inactive
            Employee? matchingEmp = null;
            if (user.EmployeeId.HasValue && user.EmployeeId.Value > 0)
            {
                matchingEmp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value);
            }
            if (matchingEmp == null && !string.IsNullOrWhiteSpace(user.Email))
            {
                matchingEmp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            }

            if (matchingEmp != null && (matchingEmp.IsDeleted || !matchingEmp.IsActive))
            {
                user.IsActive = false;
                user.IsDeleted = true;
                user.IsLocked = true;
                try { await _userRepository.UpdateAsync(user); } catch { }

                await _auditService.LogSecurityEventAsync(user.UserId, request.Username, "LoginBlocked",
                    $"Login blocked: Employee '{matchingEmp.FirstName} {matchingEmp.LastName}' has been deleted or deactivated.", null, false);
                throw new Exception("This account has been deleted or deactivated. Access is denied.");
            }

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

                try
                {
                    await _userRepository.UpdateAsync(user);
                }
                catch
                {
                    // Fail gracefully if DB row is temporarily locked so the user receives an immediate 400 Bad Request instead of a 30s timeout
                }

                if (user.IsLocked == true)
                {
                    await _auditService.LogSecurityEventAsync(user.UserId, user.Username, "AccountLocked", $"User account '{user.Username}' locked after {user.FailedLoginAttempts} failed attempts.", null, false);
                }
                else
                {
                    await _auditService.LogSecurityEventAsync(user.UserId, user.Username, "LoginFailed", $"Invalid password attempt ({user.FailedLoginAttempts}/5) for user '{user.Username}'.", null, false);
                }

                throw new Exception("Invalid username or password.");
            }

            // Successful login
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockoutEnd = null;
            user.LastLogin = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _userRepository.UpdateAsync(user);

            await _auditService.LogSecurityEventAsync(user.UserId, user.Username, "Login", $"User '{user.Username}' successfully authenticated.", null, true);

            var token = _tokenService.GenerateToken(user);
            var refreshTokenString = GenerateSecureRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                userid = user.UserId,
                token = refreshTokenString,
                expirydate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                isrevoked = false,
                createddate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            var durationInMinutes = int.Parse(
                _configuration["Jwt:DurationInMinutes"] ?? "120");

            if (!user.EmployeeId.HasValue || user.EmployeeId.Value <= 0)
            {
                matchingEmp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email && !e.IsDeleted);
                if (matchingEmp != null)
                {
                    user.EmployeeId = (int)matchingEmp.Id;
                    try { await _userRepository.UpdateAsync(user); } catch {}
                }
            }

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

            var branchName = user.Branch?.branchname;
            if (string.IsNullOrWhiteSpace(branchName) && matchingEmp != null && matchingEmp.BranchId > 0)
            {
                var empBranch = await _context.Branches.FirstOrDefaultAsync(b => b.branchid == matchingEmp.BranchId);
                branchName = empBranch?.branchname;
            }
            if (string.IsNullOrWhiteSpace(branchName) && user.BranchId.HasValue && user.BranchId.Value > 0)
            {
                var userBranch = await _context.Branches.FirstOrDefaultAsync(b => b.branchid == user.BranchId.Value);
                branchName = userBranch?.branchname;
            }

            return new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                EmployeeId = user.EmployeeId.HasValue ? (int)user.EmployeeId.Value : 0,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                RoleId = user.RoleId,
                Role = roleName,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId ?? (matchingEmp != null ? (int)matchingEmp.BranchId : null),
                BranchName = branchName,
                Token = token,
                RefreshToken = refreshTokenString,
                Expiration = DateTime.Now.AddMinutes(durationInMinutes)
            };
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new Exception("Refresh token is required.");

            var existingToken = await _context.RefreshTokens
                .Include(x => x.user)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(x => x.token == refreshToken.Trim());

            if (existingToken == null)
                throw new Exception("Invalid refresh token.");

            if (existingToken.isrevoked == true)
                throw new Exception("Refresh token has been revoked.");

            if (existingToken.expirydate <= DateTime.Now)
                throw new Exception("Refresh token has expired.");

            var user = existingToken.user;
            if (user == null || user.IsActive != true || user.IsLocked == true || user.IsDeleted == true)
                throw new Exception("User account is inactive, locked, or deleted.");

            Employee? refreshEmp = null;
            if (user.EmployeeId.HasValue && user.EmployeeId.Value > 0)
            {
                refreshEmp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value);
            }
            if (refreshEmp == null && !string.IsNullOrWhiteSpace(user.Email))
            {
                refreshEmp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            }

            if (refreshEmp != null && (refreshEmp.IsDeleted || !refreshEmp.IsActive))
            {
                existingToken.isrevoked = true;
                user.IsActive = false;
                user.IsLocked = true;
                await _context.SaveChangesAsync();
                throw new Exception("This employee account has been deleted or deactivated. Access is denied.");
            }

            // Revoke the old refresh token (rotation)
            existingToken.isrevoked = true;

            // Generate new token & new refresh token
            var newJwt = _tokenService.GenerateToken(user);
            var newRefreshTokenString = GenerateSecureRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                userid = user.UserId,
                token = newRefreshTokenString,
                expirydate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                isrevoked = false,
                createddate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            await _context.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            var durationInMinutes = int.Parse(
                _configuration["Jwt:DurationInMinutes"] ?? "120");

            Employee? matchingEmp = null;
            if (!user.EmployeeId.HasValue || user.EmployeeId.Value <= 0)
            {
                matchingEmp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email && !e.IsDeleted);
                if (matchingEmp != null)
                {
                    user.EmployeeId = (int)matchingEmp.Id;
                    try { await _userRepository.UpdateAsync(user); } catch {}
                }
            }
            else
            {
                matchingEmp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value && !e.IsDeleted);
            }

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

            var branchName = user.Branch?.branchname;
            if (string.IsNullOrWhiteSpace(branchName) && matchingEmp != null && matchingEmp.BranchId > 0)
            {
                var empBranch = await _context.Branches.FirstOrDefaultAsync(b => b.branchid == matchingEmp.BranchId);
                branchName = empBranch?.branchname;
            }
            if (string.IsNullOrWhiteSpace(branchName) && user.BranchId.HasValue && user.BranchId.Value > 0)
            {
                var userBranch = await _context.Branches.FirstOrDefaultAsync(b => b.branchid == user.BranchId.Value);
                branchName = userBranch?.branchname;
            }

            return new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                EmployeeId = user.EmployeeId.HasValue ? (int)user.EmployeeId.Value : 0,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                RoleId = user.RoleId,
                Role = roleName,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId ?? (matchingEmp != null ? (int)matchingEmp.BranchId : null),
                BranchName = branchName,
                Token = newJwt,
                RefreshToken = newRefreshTokenString,
                Expiration = DateTime.UtcNow.AddMinutes(durationInMinutes)
            };
        }

        private static string GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}