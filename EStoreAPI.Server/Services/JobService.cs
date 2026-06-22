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
            if (string.IsNullOrEmpty(query)) return await _repo.GetJobsAsync();

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

        public async Task UpdateJobAsync(UpdateJobDTO dto)
        {
            await MergeJobAsync(dto);
            await _repo.ApplyUpdateAsync();
        }

        public async Task UpdateJobsAsync(ICollection<UpdateJobDTO> dtos)
        {
            foreach (UpdateJobDTO dto in dtos)
            {
                await MergeJobAsync(dto);
            }

            await _repo.ApplyUpdateAsync();
        }

        // Validate, load, and apply the partial merge onto the tracked entity
        private async Task MergeJobAsync(UpdateJobDTO dto)
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 

            Job existing = await _repo.GetJobByIdAsync(dto.JobId)
            ?? throw new KeyNotFoundException($"Job {dto.JobId} not found.");

            // merge
            if (dto.ProblemIds != null)
            {
                // validate the supplied problem ids all resolve to real problems
                ICollection<Problem> problems = await _repo.GetProblemsByIdsAsync(dto.ProblemIds);
                if (problems.Count != dto.ProblemIds.Count)
                {
                    throw new KeyNotFoundException("One or more problem IDs are invalid.");
                }
                if (problems.Count < 1)
                {
                    throw new ValidationException("A job must have at least one problem.");
                }

                // Replace in place: mutate the tracked collection instead of swapping the
                // reference, so EF diffs the join table correctly — and so nothing downstream
                // clears the very collection it's about to read from.
                existing.Problems.Clear();
                foreach (Problem p in problems) existing.Problems.Add(p);
            }
            existing.PickupTime = dto.PickupTime ?? existing.PickupTime;
            existing.EstimatedPickupTime = dto.EstimatedPickupTime ?? existing.EstimatedPickupTime;
            existing.Note = dto.Note ?? existing.Note;
            existing.EstimatedPrice = dto.EstimatedPrice ?? existing.EstimatedPrice;
            existing.CollectedPrice = dto.CollectedPrice ?? existing.CollectedPrice;
            existing.IsFinished = dto.IsFinished ?? existing.IsFinished;
        }
    }
}