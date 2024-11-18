using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    public class Device
    {
        [Key]
        public int DeviceId { get; set; }

        [Required]
        public string DeviceName { get; set; }

        [Required]
        public string DeviceType { get; set; }

        public Device() {}

        public Device(string name, string type)
        {
            DeviceName = name;
            DeviceType = type;
        }
    }
}
