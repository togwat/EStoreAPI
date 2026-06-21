using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface IProblemService
    {
        Task<Problem?> GetProblemAsync(int id);
        Task<ICollection<Problem>> GetDeviceProblemsAsync(int deviceId);
        Task<Problem> CreateProblemAsync(InProblemDTO dto);
        Task<ICollection<Problem>> CreateProblemsAsync(ICollection<InProblemDTO> dtos);
        Task UpdateProblemAsync(UpdateProblemDTO dto);
        Task UpdateDeviceProblemsAsync(int deviceId, ICollection<InProblemDTO> dtos);
    }
}
