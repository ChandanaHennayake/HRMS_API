using HRMS.API.DTOs.User;

namespace HRMS.API.Service.User
{
    public interface IUserService
    {
        Task<int> CreateAsync(CreateUserRequest request);

        Task<List<UserResponse>> GetAllAsync();

        Task<UserResponse?> GetByIdAsync(int id);

        Task<UserProfileResponse?> GetProfileAsync(int userId);

        Task ChangePasswordAsync(ChangePasswordRequest request);

        Task AdminResetPasswordAsync(AdminResetPasswordRequest request);

        Task UpdateAsync(int id, UpdateUserRequest request);

        Task DeleteAsync(int id);
    }
}
