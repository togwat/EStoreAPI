using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface IJobService
    {
        Task<ICollection<Job>> GetAllJobsAsync();
        Task<Job?> GetJobAsync(int id);
        Task<Job> CreateJobAsync(InJobDTO dto);
        Task<ICollection<Job>> CreateJobsAsync(ICollection<InJobDTO> dtos);
        Task UpdateJobAsync(int id, InJobDTO dto);
    }
}
