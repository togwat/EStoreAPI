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
    public class ProblemsController : ControllerBase
    {
        private readonly IProblemService _service;

        public ProblemsController(IProblemService service)
        {
            _service = service;
        }

        // GET: api/Problems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Problem>> GetProblemAsync(int id)
        {
            Problem? problem = await _service.GetProblemAsync(id);
            return problem is null ? NotFound() : Ok(problem);
        }

        // GET: api/Problems?deviceId=
        [HttpGet]
        public async Task<ActionResult<ICollection<Problem>>> GetDeviceProblemsAsync([FromQuery] int deviceId)
        {
            try
            {
                ICollection<Problem> problems = await _service.GetDeviceProblemsAsync(deviceId);
                return Ok(problems);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/Problems/create
        [HttpPost("create")]
        public async Task<ActionResult<Problem>> CreateProblemAsync(ProblemDTO dto)
        {
            try
            {
                Problem newProblem = await _service.CreateProblemAsync(dto);
                return CreatedAtAction("GetProblem", new { id = newProblem.ProblemId }, newProblem);

            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }

        // POST: api/Problems/create-bulk
        [HttpPost("create-bulk")]
        public async Task<ActionResult<ICollection<Problem>>> CreateProblemsAsync(ICollection<ProblemDTO> dtos)
        {
            try
            {
                ICollection<Problem> newProblems = await _service.CreateProblemsAsync(dtos);
                return StatusCode(201, newProblems);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Problems/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateProblemWithIdAsync(int id, ProblemDTO dto)
        {
            try
            {
                await _service.UpdateProblemAsync(id, dto);
                return NoContent();
            }
            // problem not found
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            // device not found
            catch (ValidationException)
            {
                return BadRequest();
            }
        }
    }
}
