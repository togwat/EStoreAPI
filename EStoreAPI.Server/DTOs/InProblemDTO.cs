using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InProblemDTO
    {
        [Required]
        public string ProblemName { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public Problem ToModel() => new()
        {
            ProblemName = ProblemName,
            DeviceId = DeviceId,
            Price = Price,
        };
    }
}
