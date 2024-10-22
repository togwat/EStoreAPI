using EStoreAPI.Server.Data;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<Device>> CreateDeviceAsync(Device device)
        {
            Device newDevice = await _Repo.AddDeviceAsync(device);

            return CreatedAtAction(nameof(GetDeviceAsync), new { id = newDevice.DeviceId }, newDevice);
        }

        // PUT: api/Devices/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateDeviceByIdAsync(int id, Device device)
        {
            if (id != device.DeviceId)
            {
                return BadRequest();
            }

            try
            {
                await _Repo.UpdateDeviceAsync(device);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
