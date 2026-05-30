using System.ComponentModel.DataAnnotations;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace EStoreAPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly IFormService _service;

        public FormController(IFormService service)
        {
            _service = service;
        } 

        // POST: api/Form/submit
        [HttpPost("submit")]
        public async Task<ActionResult<OutJobDTO>> SubmitFormAsync(InFormDTO dto)
        {
            try
            {
                OutJobDTO newJob = await _service.SubmitFormAsync(dto);
                return Ok(newJob);
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
    }
}