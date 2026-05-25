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
    public class ProblemsController : ControllerBase
    {
        private readonly IEStoreRepo _Repo;

        public ProblemsController(IEStoreRepo repo)
        {
            _Repo = repo;
        }

        // GET: api/Problems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Problem>> GetProblemAsync(int id)
        {
            Problem? problem = await _Repo.GetProblemByIdAsync(id);

            if (problem == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(problem);
            }
        }

        // GET: api/Problems?deviceId=
        [HttpGet]
        public async Task<ActionResult<ICollection<Problem>>> GetDeviceProblemsAsync([FromQuery] int deviceId)
        {
            // check if device exists
            Device? device = await _Repo.GetDeviceByIdAsync(deviceId);

            if (device == null)
            {
                return BadRequest();
            }
            else
            {
                ICollection<Problem> problems = await _Repo.GetProblemsOfDeviceAsync(deviceId);
                return Ok(problems);
            }
        }

        // POST: api/Problems/create
        [HttpPost("create")]
        public async Task<ActionResult<Problem>> CreateProblemAsync(ProblemDTO dto)
        {
            Problem problem = new Problem
            {
                ProblemName = dto.ProblemName,
                DeviceId = dto.DeviceId,
                Price = dto.Price
            };

            try
            {
                Problem newProblem = await _Repo.AddProblemAsync(problem);
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
            ICollection<Problem> problems = dtos.Select(dto => new Problem
            {
                ProblemName = dto.ProblemName,
                DeviceId = dto.DeviceId,
                Price = dto.Price
            }).ToList();

            try
            {
                ICollection<Problem> newProblems = await _Repo.AddProblemsAsync(problems);
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
            // set new problem
            Problem problem = new Problem
            {
                ProblemId = id,
                ProblemName = dto.ProblemName,
                DeviceId = dto.DeviceId,
                Price = dto.Price
            };
            
            try
            {
                await _Repo.UpdateProblemAsync(problem);
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
