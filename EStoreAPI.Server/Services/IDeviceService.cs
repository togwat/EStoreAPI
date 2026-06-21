using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface IDeviceService
    {
        Task<ICollection<Device>> GetAllDevicesAsync();
        Task<Device?> GetDeviceAsync(int id);
        Task<ICollection<string>> GetDeviceTypesAsync();
        Task<ICollection<Device>> SearchDevicesAsync(string query);
        Task<ICollection<Device>> SearchDevicesByNameAsync(string name);
        Task<ICollection<Device>> SearchDevicesByTypeAsync(string type);
        Task<Device> CreateDeviceAsync(InDeviceDTO dto);
        Task<ICollection<Device>> CreateDevicesAsync(ICollection<InDeviceDTO> dtos);
        Task UpdateDeviceAsync(UpdateDeviceDTO dto);
        Task UpdateDevicesAsync(ICollection<UpdateDeviceDTO> dtos);
    }
}
