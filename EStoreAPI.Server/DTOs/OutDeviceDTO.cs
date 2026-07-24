using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public record OutDeviceDTO
    {
        public int DeviceId { get; init; }
        public required string DeviceName { get; init; }
        public string? ModelNumber { get; init; }
        public required string DeviceType { get; init; }

        public static OutDeviceDTO FromModel(Device d) => new()
        {
            DeviceId = d.DeviceId,
            DeviceName = d.DeviceName,
            ModelNumber = d.ModelNumber,
            DeviceType = d.DeviceType,
        };
    }
}
