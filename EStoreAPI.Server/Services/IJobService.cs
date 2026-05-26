using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface IJobService
    {
        Task<ICollection<Job>> GetAllJobsAsync();
        Task<Job?> GetJobAsync(int id);
        Task<Job> CreateJobAsync(JobDTO dto);
        Task<ICollection<Job>> CreateJobsAsync(ICollection<JobDTO> dtos);
        Task UpdateJobAsync(int id, JobDTO dto);
    }
}
