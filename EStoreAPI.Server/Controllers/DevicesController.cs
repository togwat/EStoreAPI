using EStoreAPI.Server.Services;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _service;

        public DevicesController(IDeviceService service)
        {
            _service = service;
        }

        // GET: api/Devices
        [HttpGet]
        public async Task<ActionResult<ICollection<OutDeviceDTO>>> GetAllDevicesAsync()
        {
            ICollection<Device> devices = await _service.GetAllDevicesAsync();
            return Ok(devices.Select(OutDeviceDTO.FromModel).ToList());
        }

        // GET: api/Devices/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OutDeviceDTO?>> GetDeviceAsync(int id)
        {
            Device? device = await _service.GetDeviceAsync(id);
            return device is null ? NotFound() : Ok(OutDeviceDTO.FromModel(device));
        }

        // GET: api/Devices/types
        [HttpGet("types")]
        public async Task<ActionResult<ICollection<string>>> GetDeviceTypesAsync()
        {
            ICollection<string> types = await _service.GetDeviceTypesAsync();
            return Ok(types);
        }

        // GET: api/Devices/search?name=
        [HttpGet("search")]
        public async Task<ActionResult<ICollection<OutDeviceDTO>>> SearchDevicesAsync([FromQuery] string name)
        {
            ICollection<Device> devices = await _service.SearchDevicesAsync(name);
            return Ok(devices.Select(OutDeviceDTO.FromModel).ToList());
        }

        // GET: api/Devices/searchName?name=
        [HttpGet("searchName")]
        public async Task<ActionResult<ICollection<OutDeviceDTO>>> SearchDevicesNameAsync([FromQuery] string name)
        {
            ICollection<Device> devices = await _service.SearchDevicesByNameAsync(name);
            return Ok(devices.Select(OutDeviceDTO.FromModel).ToList());
        }

        // GET: api/Devices/searchType?type=
        [HttpGet("searchType")]
        public async Task<ActionResult<ICollection<OutDeviceDTO>>> SearchDevicesTypeAsync([FromQuery] string type)
        {
            ICollection<Device> devices = await _service.SearchDevicesByTypeAsync(type);
            return Ok(devices.Select(OutDeviceDTO.FromModel).ToList());
        }

        // POST: api/Devices/create
        [HttpPost("create")]
        public async Task<ActionResult<OutDeviceDTO>> CreateDeviceAsync(InDeviceDTO dto)
        {
            try 
            {
                Device newDevice = await _service.CreateDeviceAsync(dto);
                return CreatedAtAction("GetDevice", new { id = newDevice.DeviceId }, OutDeviceDTO.FromModel(newDevice));
            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }

        // POST: api/Devices/create-bulk
        [HttpPost("create-bulk")]
        public async Task<ActionResult<ICollection<OutDeviceDTO>>> CreateDevicesAsync(ICollection<InDeviceDTO> dtos)
        {
            try
            {
                ICollection<Device> newDevices = await _service.CreateDevicesAsync(dtos);
                // fake GetAllDevices to return newly created devices
                return CreatedAtAction("GetAllDevices", null, newDevices.Select(OutDeviceDTO.FromModel).ToList());
            }
            catch(ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Devices/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateDeviceAsync(int id, UpdateDeviceDTO dto)
        {
            try
            {
                dto.DeviceId = id;
                await _service.UpdateDeviceAsync(dto);
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
