namespace EStoreAPI.Server.Services
{
    public interface IUserAccessService
    {
        // whether this email may sign in. The auth flow calls this before a session cookie is issued
        Task<bool> IsEmailAllowedAsync(string? email);
    }
}