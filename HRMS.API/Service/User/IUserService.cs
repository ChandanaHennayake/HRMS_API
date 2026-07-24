using HRMS.API.DTOs.User;

namespace HRMS.API.Service.User
{
    public interface IUserService
    {
        Task<int> CreateAsync(CreateUserRequest request);

        Task<List<UserResponse>> GetAllAsync();

        Task<UserResponse?> GetByIdAsync(int id);

        Task UpdateAsync(int id, UpdateUserRequest request);

        Task DeleteAsync(int id);
    }
}
