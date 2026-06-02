using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InProblemDTO
    {
        public int? ProblemId { get; set; }
        [Required]
        public required string ProblemName { get; set; }
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public decimal Price { get; set; }

        public Problem ToModel() => new()
        {
            ProblemId = ProblemId ?? 0, // 0 means new id, not set yet
            ProblemName = ProblemName.ToLower(),
            DeviceId = DeviceId,
            Price = Price,
        };
    }
}
