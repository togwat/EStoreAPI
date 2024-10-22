using EStoreAPI.Server.Data;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                ICollection<Problem> problems = await _Repo.GetProblemsOfDeviceAsync(device);
                return Ok(problems);
            }
        }

        // POST: api/Problems/create
        [HttpPost("create")]
        public async Task<ActionResult<Problem>> CreateProblemAsync(Problem problem)
        {
            Problem newProblem = await _Repo.AddProblemAsync(problem);

            return CreatedAtAction(nameof(GetProblemAsync), new { id = newProblem.ProblemId }, newProblem);
        }

        // PUT: api/Problems/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateProblemWithIdAsync(int id, Problem problem)
        {
            if (id != problem.ProblemId)
            {
                return BadRequest();
            }

            try
            {
                await _Repo.UpdateProblemAsync(problem);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
