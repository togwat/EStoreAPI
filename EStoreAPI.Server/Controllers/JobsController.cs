using EStoreAPI.Server.Data;
using EStoreAPI.Server.Models;
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
        public async Task<ActionResult<Job>> CreateJobAsync(Job job)
        {
            Job newJob = await _Repo.AddJobAsync(job);

            return CreatedAtAction(nameof(GetJobAsync), new { id = newJob.JobId }, newJob);
        }

        // PUT: api/Jobs/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateJobByIdAsync(int id, Job job)
        {
            // set up new job
            job.JobId = id;
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
