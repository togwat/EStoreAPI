using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InDeviceDTO
    {
        [Required]
        public required string DeviceName { get; set; }
        [Required]
        public required string DeviceType { get; set; }

        public Device ToModel() => new()
        {
            DeviceName = DeviceName,
            DeviceType = DeviceType.ToLower(),
        };
    }
}
