using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class DeviceDTO
    {
        [Required]
        public string DeviceName { get; set; }

        [Required]
        public string DeviceType { get; set; }
    }
}
