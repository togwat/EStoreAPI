using EStoreAPI.Server.Data;
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
        private readonly IEStoreRepo _Repo;

        public DevicesController(IEStoreRepo repo)
        {
            _Repo = repo;
        }

        // GET: api/Devices
        [HttpGet]
        public async Task<ActionResult<ICollection<Device>>> GetAllDevicesAsync()
        {
            ICollection<Device> devices = await _Repo.GetDevicesAsync();
            return Ok(devices);
        }

        // GET: api/Devices/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDeviceAsync(int id)
        {
            Device? device = await _Repo.GetDeviceByIdAsync(id);

            if (device == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(device);
            }
        }

        // GET: api/Devices/searchName?name=
        [HttpGet("searchName")]
        public async Task<ActionResult<ICollection<Device>>> SearchDevicesNameAsync([FromQuery] string name)
        {
            ICollection<Device> devices = await _Repo.GetDevicesByNameAsync(name);
            return Ok(devices);
        }

        // GET: api/Devices/searchType?type=
        [HttpGet("searchType")]
        public async Task<ActionResult<ICollection<Device>>> SearchDevicesTypeAsync([FromQuery] string type)
        {
            ICollection<Device> devices = await _Repo.GetDevicesByTypeAsync(type);
            return Ok(devices);
        }

        // POST: api/Devices/create
        [HttpPost("create")]
        public async Task<ActionResult<Device>> CreateDeviceAsync(DeviceDTO dto)
        {
            Device device = new Device
            {
                DeviceName = dto.DeviceName,
                DeviceType = dto.DeviceType
            };

            try
            {
                Device newDevice = await _Repo.AddDeviceAsync(device);
                return CreatedAtAction("GetDevice", new { id = newDevice.DeviceId }, newDevice);
            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }

        // POST: api/Devices/create-bulk
        [HttpPost("create-bulk")]
        public async Task<ActionResult<ICollection<Device>>> CreateDevicesAsync(ICollection<DeviceDTO> dtos)
        {
            ICollection<Device> devices = dtos.Select(dto => new Device
            {
                DeviceName = dto.DeviceName,
                DeviceType = dto.DeviceType
            }).ToList();

            try
            {
                ICollection<Device> newDevices = await _Repo.AddDevicesAsync(devices);
                // fake GetAllDevices to return newly created devices
                return CreatedAtAction("GetAllDevices", null, newDevices);
            }
            catch(ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Devices/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateDeviceByIdAsync(int id, DeviceDTO dto)
        {
            // set up new device
            Device device = new Device
            {
                DeviceId = id,
                DeviceName = dto.DeviceName,
                DeviceType = dto.DeviceType
            };
            
            try
            {
                await _Repo.UpdateDeviceAsync(device);
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
