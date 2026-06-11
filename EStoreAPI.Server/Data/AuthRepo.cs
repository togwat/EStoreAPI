using EStoreAPI.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EStoreAPI.Server.Data
{
    public class AuthRepo : IAuthRepo
    {
        private readonly EStoreDbContext _dbContext;

        public AuthRepo(EStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetAllowedUserByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}