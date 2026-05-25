using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class ProblemDTO
    {
        [Required]
        public string ProblemName { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
