using HRMS.API.Entities;

namespace HRMS.API.Service.Authentication
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user);
    }
}