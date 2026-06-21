using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using NuGet.Packaging;
using System.ComponentModel.DataAnnotations;

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

        // search by either model name or number
        public async Task<ICollection<Device>> SearchDevicesAsync(string query)
        {
            ICollection<Device> devicesByName = await _repo.GetDevicesByNameAsync(query);
            ICollection<Device> devicesByModelNumber = await _repo.GetDevicesByModelNumberAsync(query);

            // return union of two lists, deduped by id so a device matching both name and model number appears once
            return devicesByName.UnionBy(devicesByModelNumber, d => d.DeviceId).ToList();
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
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 
            Device device = dto.ToModel();

            return await _repo.AddDeviceAsync(device);
        }

        public async Task<ICollection<Device>> CreateDevicesAsync(ICollection<InDeviceDTO> dtos)
        {
            foreach (InDeviceDTO dto in dtos)
            {
                Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 
            }

            ICollection<Device> devices = dtos.Select(dto => dto.ToModel()).ToList();

            return await _repo.AddDevicesAsync(devices);
        }

        public async Task UpdateDeviceAsync(UpdateDeviceDTO dto)
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
            
            Device existing = await _repo.GetDeviceByIdAsync(dto.DeviceId)
            ?? throw new KeyNotFoundException($"Device {dto.DeviceId} not found.");
            
            // merge
            existing.DeviceName = dto.DeviceName ?? existing.DeviceName;
            existing.ModelNumber = dto.ModelNumber ?? existing.ModelNumber;
            existing.DeviceType = dto.DeviceType ?? existing.DeviceType;

            await _repo.UpdateDeviceAsync(existing);
        }
    }
}