using EStoreAPI.Server.DTOs;

namespace EStoreAPI.Server.Services
{
    public interface IReadOnlyQueryService
    {
        Task<OutQueryResultDTO> ExecuteQueryAsync(string sql);
    }
}
