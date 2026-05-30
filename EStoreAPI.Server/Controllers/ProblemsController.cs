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
        public async Task<ActionResult<OutProblemDTO>> GetProblemAsync(int id)
        {
            Problem? problem = await _service.GetProblemAsync(id);
            return problem is null ? NotFound() : Ok(OutProblemDTO.FromModel(problem));
        }

        // GET: api/Problems/device/{deviceId}
        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<ICollection<OutProblemDTO>>> GetDeviceProblemsAsync(int deviceId)
        {
            try
            {
                ICollection<Problem> problems = await _service.GetDeviceProblemsAsync(deviceId);
                return Ok(problems.Select(OutProblemDTO.FromModel).ToList());
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/Problems/create
        [HttpPost("create")]
        public async Task<ActionResult<OutProblemDTO>> CreateProblemAsync(InProblemDTO dto)
        {
            try
            {
                Problem newProblem = await _service.CreateProblemAsync(dto);
                return CreatedAtAction("GetProblem", new { id = newProblem.ProblemId }, OutProblemDTO.FromModel(newProblem));

            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }

        // POST: api/Problems/create-bulk
        [HttpPost("create-bulk")]
        public async Task<ActionResult<ICollection<OutProblemDTO>>> CreateProblemsAsync(ICollection<InProblemDTO> dtos)
        {
            try
            {
                ICollection<Problem> newProblems = await _service.CreateProblemsAsync(dtos);
                return StatusCode(201, newProblems.Select(OutProblemDTO.FromModel).ToList());
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Problems/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateProblemWithIdAsync(int id, InProblemDTO dto)
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
