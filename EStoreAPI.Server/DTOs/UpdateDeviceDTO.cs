using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public record UpdateDeviceDTO
    {
        [Required]
        [Description("The ID of the device to update.")]
        public int DeviceId { get; set; }

        [Description("New device name.")]
        public string? DeviceName { get; init; }

        [Description("New model number.")]
        public string? ModelNumber { get; init; }

        [Description("New device type.")]
        public string? DeviceType { get; init; }
    }
}
