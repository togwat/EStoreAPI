using EStoreAPI.Server.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InDeviceDTO
    {
        [Required]
        [Description("Device model name. Required.")]
        public required string DeviceName { get; set; }

        [Required]
        [Description("Device type (e.g. phone, tablet, laptop). Required.")]
        public required string DeviceType { get; set; }

        public Device ToModel() => new()
        {
            DeviceName = DeviceName,
            DeviceType = DeviceType.ToLower(),
        };
    }
}
