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
                throw new KeyNotFoundException($"Customer {customerId} not found.");
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
            ICollection<Customer> customers = await _repo.GetCustomersByQueryAsync(query);
            ICollection<Device> devices = await _repo.GetDevicesByNameAsync(query);

            // get matching jobs
            List<Job> results = new();
            foreach (Customer c in customers) results.AddRange(await _repo.GetJobsOfCustomerAsync(c.CustomerId));
            foreach (Device d in devices) results.AddRange(await _repo.GetJobsOfDeviceAsync(d.DeviceId));

            // prevent duplicate results
            return results.DistinctBy(j => j.JobId).ToList();
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
            // if warranty, validate warranty parent exists
            if (dto.WarrantyOfJobId != null)
            {
                await ValidateWarrantyLink(dto.WarrantyOfJobId.Value, null);
            }

            Job job = dto.ToModel(problems);
            job.Logs = [CreateLog(job)];

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
                // if warranty, validate warranty parents exist
                if (dto.WarrantyOfJobId != null)
                {
                    await ValidateWarrantyLink(dto.WarrantyOfJobId.Value, null);
                }

                Job job = dto.ToModel(problems);
                job.Logs = [CreateLog(job)];
                jobs.Add(job);
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

                // Replace in place: mutate the tracked collection instead of swapping the
                // reference, so EF diffs the join table correctly — and so nothing downstream
                // clears the very collection it's about to read from.
                existing.Problems.Clear();
                foreach (Problem p in problems) existing.Problems.Add(p);
            }
            // if warranty, validate warranty parent
            if (dto.WarrantyOfJobId != null)
            {
                await ValidateWarrantyLink(dto.WarrantyOfJobId.Value, dto.JobId);
            }
            
            // record old values for logging
            // treat null as 0 money
            JobStatus oldStatus = existing.Status;
            string? oldNote = existing.Note;
            decimal oldCollected = existing.CollectedPrice ?? 0m;

            existing.ReceiveTime = dto.ReceiveTime ?? existing.ReceiveTime;
            existing.PickupTime = dto.PickupTime ?? existing.PickupTime;
            existing.EstimatedPickupTime = dto.EstimatedPickupTime ?? existing.EstimatedPickupTime;
            existing.Note = dto.Note ?? existing.Note;
            existing.EstimatedPrice = dto.EstimatedPrice ?? existing.EstimatedPrice;
            existing.CollectedPrice = dto.CollectedPrice ?? existing.CollectedPrice;
            existing.Status = dto.Status ?? existing.Status;
            existing.WarrantyOfJobId = dto.WarrantyOfJobId ?? existing.WarrantyOfJobId;

            // update/transction logging
            JobStatus? statusChange = existing.Status != oldStatus ? existing.Status : null;
            string? noteChange = existing.Note != oldNote ? existing.Note : null;
            // 0 change in money is set to null
            decimal? moneyChange = (existing.CollectedPrice ?? 0m) - oldCollected;
            moneyChange = moneyChange == 0m ? null : moneyChange;

            // guard against no change updates
            if (statusChange != null || noteChange != null || moneyChange != null)
            {
                // guard against old jobs not having a logging system
                // models already does this, this stops tests from complaining
                // because the fixture can't initialise virtual list
                existing.Logs ??= new List<JobLog>();
                existing.Logs.Add(UpdateLog(statusChange, noteChange, moneyChange));
            }
        }

        // check if warranty exists, and check if a job isn't linking itself
        private async Task ValidateWarrantyLink(int parentId, int? selfId)
        {
            // check for self linking
            if (selfId == parentId)
            {
                throw new ValidationException("A job cannot link itself as warranty.");
            }
            // check parent exists
            Job? parent = await _repo.GetJobByIdAsync(parentId);
            if (parent is null)
            {
                throw new KeyNotFoundException($"Job {parentId} not found when linking for warranty.");
            }
        }

        // making new log objects
        private static JobLog CreateLog(Job job) => new()
        {
            Timestamp = DateTime.UtcNow,
            Status = job.Status,
            Note = job.Note
        };

        private static JobLog UpdateLog(JobStatus? status, string? note, decimal? moneyChange) => new()
        {
            Timestamp = DateTime.UtcNow,
            Status = status,
            Note = note,
            MoneyChange = moneyChange
        };
    }
}