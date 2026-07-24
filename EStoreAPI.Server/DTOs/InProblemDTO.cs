using EStoreAPI.Server.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public record InProblemDTO
    {
        // id for UpdateDeviceProblemsAsync
        [Description("Problem ID. Only required when updating an existing problem.")]
        public int? ProblemId { get; init; }

        [Required]
        [Description("Name of the problem (e.g. screen replacement). Required.")]
        public required string ProblemName { get; init; }

        [Required]
        [Description("ID of the device this problem belongs to. Required.")]
        public int DeviceId { get; init; }

        [Required]
        [Description("Overall price for this problem. Required.")]
        public decimal Price { get; init; }

        [Description("Parts price for this problem.")]
        public decimal PartsPrice { get; init; }

        [Description("Labour cost for this problem.")]
        public decimal LabourPrice { get; init; }

        [Description("Risk cost for this problem.")]
        public decimal RiskCost { get; init; }

        public Problem ToModel() => new()
        {
            ProblemId = ProblemId ?? 0, // 0 means new id, not set yet
            ProblemName = ProblemName.ToLower(),
            DeviceId = DeviceId,
            Price = Price,
            PartsPrice = PartsPrice,
            LabourPrice = LabourPrice,
            RiskCost = RiskCost
        };
    }
}
