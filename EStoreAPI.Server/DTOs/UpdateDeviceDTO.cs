using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class UpdateDeviceDTO
    {
        [Required]
        [Description("The ID of the device to update.")]
        public required int DeviceId { get; set; }

        [Description("New device name.")]
        public string? DeviceName { get; set; }

        [Description("New model number.")]
        public string? ModelNumber { get; set; }

        [Description("New device type.")]
        public string? DeviceType { get; set; }
    }
}
