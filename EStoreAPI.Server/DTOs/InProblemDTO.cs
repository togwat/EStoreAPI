using EStoreAPI.Server.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InProblemDTO
    {
        // id for UpdateDeviceProblemsAsync
        [Description("Problem ID. Only required when updating an existing problem.")]
        public int? ProblemId { get; set; }

        [Required]
        [Description("Name of the problem (e.g. screen replacement). Required.")]
        public required string ProblemName { get; set; }

        [Required]
        [Description("ID of the device this problem belongs to. Required.")]
        public int DeviceId { get; set; }

        [Required]
        [Description("Overall price for this problem. Required.")]
        public decimal Price { get; set; }

        [Description("Parts price for this problem.")]
        public decimal PartsPrice { get; set; }

        [Description("Labour cost for this problem.")]
        public decimal LabourPrice { get; set; }

        [Description("Risk cost for this problem.")]
        public decimal RiskCost { get; set; }

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
