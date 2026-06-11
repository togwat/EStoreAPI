using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Data
{
    // auth-related data access
    public interface IAuthRepo
    {
        Task<User?> GetAllowedUserByEmailAsync(string email);
    }
}