using HRMS.API.Entities;

namespace HRMS.API.Repository.UserDetails
{
    public interface IUserRepository
    {
        // User CRUD
        Task<int> CreateAsync(AppUser user);

        Task<AppUser?> GetByIdAsync(int id);

        Task<AppUser?> GetByUsernameAsync(string username);

        Task<AppUser?> GetByEmailAsync(string email);

        Task<List<AppUser>> GetAllAsync();

        Task UpdateAsync(AppUser user);

        // Authentication
        Task<AppUser?> GetUserForLoginAsync(string username);

        Task SaveChangesAsync();
    }
}