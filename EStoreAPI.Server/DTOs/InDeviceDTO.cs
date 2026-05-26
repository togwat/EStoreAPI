using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InDeviceDTO
    {
        [Required]
        public string DeviceName { get; set; }

        [Required]
        public string DeviceType { get; set; }

        public Device ToModel() => new()
        {
            DeviceName = DeviceName,
            DeviceType = DeviceType,
        };
    }
}
