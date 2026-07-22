using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public class OutJobDTO
    {
        public int JobId { get; set; }
        public int CustomerId { get; set; }
        public int DeviceId { get; set; }
        public DateTime ReceiveTime { get; set; }
        public DateTime? PickupTime { get; set; }
        public DateTime? EstimatedPickupTime { get; set; }
        public string? Note { get; set; }
        public ICollection<OutProblemDTO> Problems { get; set; } = [];
        public decimal? EstimatedPrice { get; set; }
        public decimal? CollectedPrice { get; set; }
        public JobStatus Status { get; set; }
        public int? WarrantyOfJobId { get; set; }

        public static OutJobDTO FromModel(Job j) => new()
        {
            JobId = j.JobId,
            CustomerId = j.CustomerId,
            DeviceId = j.DeviceId,
            ReceiveTime = j.ReceiveTime,
            PickupTime = j.PickupTime,
            EstimatedPickupTime = j.EstimatedPickupTime,
            Note = j.Note,
            Problems = j.Problems?.Select(OutProblemDTO.FromModel).ToList() ?? [],
            EstimatedPrice = j.EstimatedPrice,
            CollectedPrice = j.CollectedPrice,
            Status = j.Status,
            WarrantyOfJobId = j.WarrantyOfJobId
        };
    }
}
