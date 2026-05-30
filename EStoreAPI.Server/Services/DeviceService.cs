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

        public Task<ICollection<string>> GetDeviceTypesAsync()
        {
            return _repo.GetDeviceTypesAsync();
        }

        public Task<ICollection<Device>> SearchDevicesByNameAsync(string name)
        {
            return _repo.GetDevicesByNameAsync(name);
        }

        public Task<ICollection<Device>> SearchDevicesByTypeAsync(string type)
        {
            return _repo.GetDevicesByTypeAsync(type);
        }

        public async Task<Device> CreateDeviceAsync(InDeviceDTO dto)
        {
            Device device = dto.ToModel();

            return await _repo.AddDeviceAsync(device);
        }

        public async Task<ICollection<Device>> CreateDevicesAsync(ICollection<InDeviceDTO> dtos)
        {
            ICollection<Device> devices = dtos.Select(dto => dto.ToModel()).ToList();

            return await _repo.AddDevicesAsync(devices);
        }

        public async Task UpdateDeviceAsync(int id, InDeviceDTO dto)
        {
            // set up new device
            Device device = dto.ToModel();

            await _repo.UpdateDeviceAsync(device);
        }
    }
}