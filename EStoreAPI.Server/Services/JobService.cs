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

        // search jobs by customer or device name
        // leave empty to get all jobs
        public async Task<ICollection<Job>> SearchJobsAsync(string? query)
        {
            if (query is null) return await _repo.GetJobsAsync();

            // union of jobs with matching customer or device
            // can run these lookups in parallel
            Task<ICollection<Customer>> customersTask = _repo.GetCustomersByQueryAsync(query);
            Task<ICollection<Device>> devicesTask = _repo.GetDevicesByNameAsync(query);
            await Task.WhenAll(customersTask, devicesTask);

            // get matching jobs
            IEnumerable<Task<ICollection<Job>>> matchesCustomersTask = customersTask.Result.Select(c => _repo.GetJobsOfCustomerAsync(c.CustomerId));
            IEnumerable<Task<ICollection<Job>>> matchesDevicesTask = devicesTask.Result.Select(d => _repo.GetJobsOfDeviceAsync(d.DeviceId));
            ICollection<Job>[] results = await Task.WhenAll(matchesCustomersTask.Concat(matchesDevicesTask));

            // prevent duplicate results
            return results.SelectMany(j => j).DistinctBy(j => j.JobId).ToList();
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
            job.JobId = id;

            await _repo.UpdateJobAsync(job);
        }
    }
}