using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public class OutDeviceDTO
    {
        public int DeviceId { get; set; }
        public required string DeviceName { get; set; }
        public string? ModelNumber { get; set; }
        public required string DeviceType { get; set; }

        public static OutDeviceDTO FromModel(Device d) => new()
        {
            DeviceId = d.DeviceId,
            DeviceName = d.DeviceName,
            ModelNumber = d.ModelNumber,
            DeviceType = d.DeviceType,
        };
    }
}
