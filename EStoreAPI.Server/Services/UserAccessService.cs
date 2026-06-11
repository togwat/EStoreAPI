using EStoreAPI.Server.Data;

namespace EStoreAPI.Server.Services
{
    public class UserAccessService : IUserAccessService
    {
        private readonly IAuthRepo _repo;
        public UserAccessService(IAuthRepo repo)
        {
            _repo = repo;
        }

        public async Task<bool> IsEmailAllowedAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return await _repo.GetAllowedUserByEmailAsync(email) is not null;
        }
    }
}