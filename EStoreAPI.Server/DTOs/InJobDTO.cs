using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InJobDTO
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

        // problems must be resolved from ProblemIds by the service before calling this
        public Job ToModel(ICollection<Problem> problems) => new()
        {
            CustomerId = CustomerId,
            DeviceId = DeviceId,
            ReceiveTime = ReceiveTime,
            PickupTime = PickupTime,
            EstimatedPickupTime = EstimatedPickupTime,
            Note = Note,
            Problems = problems,
            EstimatedPrice = EstimatedPrice,
            CollectedPrice = CollectedPrice,
            IsFinished = IsFinished,
        };
    }
}
