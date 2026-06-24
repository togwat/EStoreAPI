using EStoreAPI.Server.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InJobDTO
    {
        [Required]
        [Description("ID of the customer being serviced. Required.")]
        public int CustomerId { get; set; }

        [Required]
        [Description("ID of the device being repaired. Required.")]
        public int DeviceId { get; set; }

        [Description("Time the device was received. Defaults to now if omitted.")]
        public DateTime? ReceiveTime { get; set; }

        [Description("Time the device was picked up by the customer.")]
        public DateTime? PickupTime { get; set; }

        [Description("Estimated pickup time.")]
        public DateTime? EstimatedPickupTime { get; set; }

        [Description("Additional notes about the job.")]
        public string? Note { get; set; }

        [Required]
        [MinLength(1)]
        [Description("List of problem IDs to fix. At least one required. Retrieve problem IDs from the device's problem catalogue.")]
        public List<int> ProblemIds { get; set; } = new();

        [Description("Estimated price. Defaults to the sum of the selected problems' prices if omitted.")]
        public decimal? EstimatedPrice { get; set; }

        [Description("Price collected from the customer.")]
        public decimal? CollectedPrice { get; set; }

        [Description("Whether the job is finished. Defaults to false.")]
        public bool IsFinished { get; set; } = false;

        [Description("ID of the prior job this one is a warranty for. Set only when the repair is a warranty follow-up to an earlier job, omit for normal jobs. Search that customer's jobs to find the ID.")]
        public int? WarrantyOfJobId { get; set; }

        // problems must be resolved from ProblemIds by the service before calling this
        public Job ToModel(ICollection<Problem> problems) => new()
        {
            CustomerId = CustomerId,
            DeviceId = DeviceId,
            ReceiveTime = ReceiveTime ?? DateTime.UtcNow,
            PickupTime = PickupTime,
            EstimatedPickupTime = EstimatedPickupTime,
            Note = Note,
            Problems = problems,
            EstimatedPrice = EstimatedPrice ?? problems.Sum(p => p.Price + p.LabourPrice + p.RiskCost),
            CollectedPrice = CollectedPrice,
            IsFinished = IsFinished,
            WarrantyOfJobId = WarrantyOfJobId
        };
    }
}
