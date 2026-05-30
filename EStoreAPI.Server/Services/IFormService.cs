using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface IFormService
    {
        Task<OutJobDTO> SubmitFormAsync(InFormDTO dto);
    }
}
