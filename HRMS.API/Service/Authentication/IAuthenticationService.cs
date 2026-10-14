using HRMS.API.DTOs.Authentication;

namespace HRMS.API.Service.Authentication
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);

        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    }
}