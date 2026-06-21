using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface IJobService
    {
        Task<ICollection<Job>> GetAllJobsAsync();
        Task<Job?> GetJobAsync(int id);
        Task<ICollection<Job>> GetCustomerJobsAsync(int customerId);
        Task<ICollection<Job>> SearchJobsAsync(string? query);
        Task<Job> CreateJobAsync(InJobDTO dto);
        Task<ICollection<Job>> CreateJobsAsync(ICollection<InJobDTO> dtos);
        Task UpdateJobAsync(UpdateJobDTO dto);
    }
}
