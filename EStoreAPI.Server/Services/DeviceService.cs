using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IEStoreRepo _repo;
        public DeviceService(IEStoreRepo repo)
        {
            _repo = repo;
        }

        public Task<ICollection<Device>> GetAllDevicesAsync()
        {
            return _repo.GetDevicesAsync();
        }

        public Task<Device?> GetDeviceAsync(int id)
        {
            return _repo.GetDeviceByIdAsync(id);
        }

        public Task<ICollection<Device>> SearchDevicesByNameAsync(string name)
        {
            return _repo.GetDevicesByNameAsync(name);
        }

        public Task<ICollection<Device>> SearchDevicesByTypeAsync(string type)
        {
            return _repo.GetDevicesByTypeAsync(type);
        }

        public async Task<Device> CreateDeviceAsync(DeviceDTO dto)
        {
            Device device = new Device
            {
                DeviceName = dto.DeviceName,
                DeviceType = dto.DeviceType
            };

            return await _repo.AddDeviceAsync(device);
        }

        public async Task<ICollection<Device>> CreateDevicesAsync(ICollection<DeviceDTO> dtos)
        {
            ICollection<Device> devices = dtos.Select(dto => new Device
            {
                DeviceName = dto.DeviceName,
                DeviceType = dto.DeviceType
            }).ToList();

            return await _repo.AddDevicesAsync(devices);
        }

        public async Task UpdateDeviceAsync(int id, DeviceDTO dto)
        {
            // set up new device
            Device device = new Device
            {
                DeviceId = id,
                DeviceName = dto.DeviceName,
                DeviceType = dto.DeviceType
            };

            await _repo.UpdateDeviceAsync(device);
        }
    }
}