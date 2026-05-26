using EStoreAPI.Server.Services;
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
        private readonly IJobService _service;

        public JobsController(IJobService service)
        {
            _service = service;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<ActionResult<ICollection<OutJobDTO>>> GetAllJobsAsync()
        {
            ICollection<Job> jobs = await _service.GetAllJobsAsync();
            return Ok(jobs.Select(OutJobDTO.FromModel).ToList());
        }

        // GET: api/Jobs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OutJobDTO>> GetJobAsync(int id)
        {
            Job? job = await _service.GetJobAsync(id);
            return job is null ? NotFound() : Ok(OutJobDTO.FromModel(job));
        }

        // POST: api/Jobs/create
        [HttpPost("create")]
        public async Task<ActionResult<OutJobDTO>> CreateJobAsync(InJobDTO dto)
        {
            try
            {
                Job newJob = await _service.CreateJobAsync(dto);
                return CreatedAtAction("GetJob", new { id = newJob.JobId }, OutJobDTO.FromModel(newJob));
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }

        // POST: api/Jobs/create-bulk
        [HttpPost("create-bulk")]
        public async Task<ActionResult<ICollection<OutJobDTO>>> CreateJobsAsync(ICollection<InJobDTO> dtos)
        {
            try
            {
                ICollection<Job> newJobs = await _service.CreateJobsAsync(dtos);
                // fake GetAllJobs to return newly created jobs
                return CreatedAtAction("GetAllJobs", null, newJobs.Select(OutJobDTO.FromModel).ToList());
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Jobs/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateJobAsync(int id, InJobDTO dto)
        {
            try
            {
                await _service.UpdateJobAsync(id, dto);
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