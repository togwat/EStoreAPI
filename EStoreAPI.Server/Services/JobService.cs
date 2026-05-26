using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public class JobService : IJobService
    {
        private readonly IEStoreRepo _repo;
        public JobService(IEStoreRepo repo)
        {
            _repo = repo;
        }

        public Task<ICollection<Job>> GetAllJobsAsync()
        {
            return _repo.GetJobsAsync();
        }

        public Task<Job?> GetJobAsync(int id)
        {
            return _repo.GetJobByIdAsync(id);
        }

        public async Task<Job> CreateJobAsync(JobDTO dto)
        {
            // validate number of problems
            ICollection<Problem> problems = await _repo.GetProblemsByIdsAsync(dto.ProblemIds);
            if (problems.Count != dto.ProblemIds.Count)
            {
                throw new KeyNotFoundException("One or more problem IDs are invalid.");
            }

            Job job = new Job
            {
                CustomerId = dto.CustomerId,
                DeviceId = dto.DeviceId,
                ReceiveTime = dto.ReceiveTime,
                PickupTime = dto.PickupTime,
                EstimatedPickupTime = dto.EstimatedPickupTime,
                Note = dto.Note,
                Problems = problems,
                EstimatedPrice = dto.EstimatedPrice,
                CollectedPrice = dto.CollectedPrice,
                IsFinished = dto.IsFinished
            };

            return await _repo.AddJobAsync(job);
        }

        public async Task<ICollection<Job>> CreateJobsAsync(ICollection<JobDTO> dtos)
        {
            List<Job> jobs = new();
            foreach (var dto in dtos)
            {
                // validate number of problems
                ICollection<Problem> problems = await _repo.GetProblemsByIdsAsync(dto.ProblemIds);
                if (problems.Count != dto.ProblemIds.Count)
                {
                    throw new KeyNotFoundException($"One or more problem IDs are invalid for job with customerId {dto.CustomerId}.");
                }

                jobs.Add(new Job
                {
                    CustomerId = dto.CustomerId,
                    DeviceId = dto.DeviceId,
                    ReceiveTime = dto.ReceiveTime,
                    PickupTime = dto.PickupTime,
                    EstimatedPickupTime = dto.EstimatedPickupTime,
                    Note = dto.Note,
                    Problems = problems,
                    EstimatedPrice = dto.EstimatedPrice,
                    CollectedPrice = dto.CollectedPrice,
                    IsFinished = dto.IsFinished
                });
            }

            return await _repo.AddJobsAsync(jobs);
        }

        public async Task UpdateJobAsync(int id, JobDTO dto)
        {
            // validate number of problems
            ICollection<Problem> problems = await _repo.GetProblemsByIdsAsync(dto.ProblemIds);
            if (problems.Count != dto.ProblemIds.Count)
                throw new KeyNotFoundException("One or more problem IDs are invalid.");

            Job job = new Job
            {
                JobId = id,
                CustomerId = dto.CustomerId,
                DeviceId = dto.DeviceId,
                ReceiveTime = dto.ReceiveTime,
                PickupTime = dto.PickupTime,
                EstimatedPickupTime = dto.EstimatedPickupTime,
                Note = dto.Note,
                Problems = problems,
                EstimatedPrice = dto.EstimatedPrice,
                CollectedPrice = dto.CollectedPrice,
                IsFinished = dto.IsFinished
            };

            await _repo.UpdateJobAsync(job);
        }
    }
}