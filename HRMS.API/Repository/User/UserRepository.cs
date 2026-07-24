using HRMS.API.Data;
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