using EStoreAPI.Server.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class InProblemDTO
    {
        [Description("Problem ID. Only required when updating an existing problem.")]
        public int? ProblemId { get; set; }

        [Required]
        [Description("Name of the problem (e.g. screen replacement). Required.")]
        public required string ProblemName { get; set; }

        [Required]
        [Description("ID of the device this problem belongs to. Required.")]
        public int DeviceId { get; set; }

        [Required]
        [Description("Parts price for this problem. Required.")]
        public decimal Price { get; set; }

        [Required]
        [Description("Labour cost for this problem. Required.")]
        public decimal LabourPrice { get; set; }

        public Problem ToModel() => new()
        {
            ProblemId = ProblemId ?? 0, // 0 means new id, not set yet
            ProblemName = ProblemName.ToLower(),
            DeviceId = DeviceId,
            Price = Price,
            LabourPrice = LabourPrice
        };
    }
}
