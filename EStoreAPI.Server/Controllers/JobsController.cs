using EStoreAPI.Server.Data;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IEStoreRepo _Repo;

        public JobsController(IEStoreRepo repo)
        {
            _Repo = repo;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<ActionResult<ICollection<Job>>> GetAllJobsAsync()
        {
            ICollection<Job> jobs = await _Repo.GetJobsAsync();
            return Ok(jobs);
        }

        // GET: api/Jobs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJobAsync(int id)
        {
            Job? job = await _Repo.GetJobByIdAsync(id);

            if (job == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(job);
            }
        }

        // POST: api/Jobs/create
        [HttpPost("create")]
        public async Task<ActionResult<Job>> CreateJobAsync(JobDTO dto)
        {
            // validate number of problems
            ICollection<Problem> problems = await _Repo.GetProblemsByIdsAsync(dto.ProblemIds);
            if (problems.Count != dto.ProblemIds.Count)
                return BadRequest("One or more problem IDs are invalid.");

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

            try
            {
                Job newJob = await _Repo.AddJobAsync(job);
                return CreatedAtAction("GetJob", new { id = newJob.JobId }, newJob);
            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }

        // POST: api/Jobs/create-bulk
        [HttpPost("create-bulk")]
        public async Task<ActionResult<ICollection<Job>>> CreateJobsAsync(ICollection<JobDTO> dtos)
        {
            List<Job> jobs = new();
            foreach (var dto in dtos)
            {
                // validate number of problems
                ICollection<Problem> problems = await _Repo.GetProblemsByIdsAsync(dto.ProblemIds);
                if (problems.Count != dto.ProblemIds.Count)
                    return BadRequest($"One or more problem IDs are invalid for job with customerId {dto.CustomerId}.");

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

            try
            {
                ICollection<Job> newJobs = await _Repo.AddJobsAsync(jobs);
                // fake GetAllJobs to return newly created jobs
                return CreatedAtAction("GetAllJobs", null, newJobs);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Jobs/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateJobByIdAsync(int id, JobDTO dto)
        {
            // validate number of problems
            ICollection<Problem> problems = await _Repo.GetProblemsByIdsAsync(dto.ProblemIds);
            if (problems.Count != dto.ProblemIds.Count)
                return BadRequest("One or more problem IDs are invalid.");

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

            try
            {
                await _Repo.UpdateJobAsync(job);
                return NoContent();
            }
            // job not found
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            // invalid data
            catch (ValidationException)
            {
                return BadRequest();
            }
        }
    }
}