using EStoreAPI.Server.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public record InDeviceDTO
    {
        [Required]
        [Description("Device model name. Required.")]
        public required string DeviceName { get; init; }

        [Description("Device model number.")]
        public string? ModelNumber { get; init; }

        [Required]
        [Description("Device type (e.g. phone, tablet, laptop). Required.")]
        public required string DeviceType { get; init; }

        public Device ToModel() => new()
        {
            DeviceName = DeviceName,
            ModelNumber = ModelNumber,
            DeviceType = DeviceType.ToLower(),
        };
    }
}
