using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class JobDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        public DateTime ReceiveTime { get; set; }

        public DateTime? PickupTime { get; set; }

        public DateTime? EstimatedPickupTime { get; set; }

        public string? Note { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> ProblemIds { get; set; } = new();

        public decimal? EstimatedPrice { get; set; }

        public decimal? CollectedPrice { get; set; }

        public bool IsFinished { get; set; } = false;
    }
}
