using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

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

        public async Task<ICollection<Job>> GetCustomerJobsAsync(int customerId)
        {
            // check if customer exists
            Customer? customer = await _repo.GetCustomerByIdAsync(customerId);

            if (customer is null)
            {
                throw new KeyNotFoundException($"Customer {customer} not found.");
            }
            else
            {
                return await _repo.GetJobsOfCustomerAsync(customerId);
            }
        }

        public async Task<Job> CreateJobAsync(InJobDTO dto)
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 

            // validate number of problems
            ICollection<Problem> problems = await _repo.GetProblemsByIdsAsync(dto.ProblemIds);
            if (problems.Count != dto.ProblemIds.Count)
            {
                throw new KeyNotFoundException("One or more problem IDs are invalid.");
            }

            Job job = dto.ToModel(problems);

            return await _repo.AddJobAsync(job);
        }

        public async Task<ICollection<Job>> CreateJobsAsync(ICollection<InJobDTO> dtos)
        {
            List<Job> jobs = new();
            foreach (InJobDTO dto in dtos)
            {
                Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 

                // validate number of problems
                ICollection<Problem> problems = await _repo.GetProblemsByIdsAsync(dto.ProblemIds);
                if (problems.Count != dto.ProblemIds.Count)
                {
                    throw new KeyNotFoundException($"One or more problem IDs are invalid for job with customerId {dto.CustomerId}.");
                }

                jobs.Add(dto.ToModel(problems));
            }

            return await _repo.AddJobsAsync(jobs);
        }

        public async Task UpdateJobAsync(int id, InJobDTO dto)
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 
            
            // validate number of problems
            ICollection<Problem> problems = await _repo.GetProblemsByIdsAsync(dto.ProblemIds);
            if (problems.Count != dto.ProblemIds.Count)
                throw new KeyNotFoundException("One or more problem IDs are invalid.");

            Job job = dto.ToModel(problems);

            await _repo.UpdateJobAsync(job);
        }
    }
}