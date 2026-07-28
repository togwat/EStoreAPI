using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public record OutJobDTO
    {
        public int JobId { get; init; }
        public int CustomerId { get; init; }
        public int DeviceId { get; init; }
        public DateTime ReceiveTime { get; init; }
        public DateTime? PickupTime { get; init; }
        public DateTime? EstimatedPickupTime { get; init; }
        public string? Note { get; init; }
        public ICollection<OutProblemDTO> Problems { get; init; } = [];
        public decimal? EstimatedPrice { get; init; }
        public decimal? CollectedPrice { get; init; }
        public JobStatus Status { get; init; }
        public int? WarrantyOfJobId { get; init; }
        public ICollection<OutJobLogDTO> Logs { get; init; } = [];

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
            WarrantyOfJobId = j.WarrantyOfJobId,
            Logs = j.Logs?.Select(OutJobLogDTO.FromModel).ToList() ?? []
        };
    }
}
