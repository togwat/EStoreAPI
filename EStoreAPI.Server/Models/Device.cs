using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    public class Device
    {
        [Key]
        public int DeviceId { get; set; }

        [Required]
        public string deviceName { get; set; }

        [Required]
        public string deviceType { get; set; }

        public Device() {}

        public Device(string name, string type)
        {
            deviceName = name;
            deviceType = type;
        }
    }
}
