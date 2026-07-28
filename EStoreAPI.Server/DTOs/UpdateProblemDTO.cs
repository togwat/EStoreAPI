using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public record UpdateProblemDTO
    {
        [Required]
        [Description("The ID of the problem to update.")]
        public int ProblemId { get; set; }

        [Description("New problem name.")]
        public string? ProblemName { get; init; }

        [Description("New device id.")]
        public int? DeviceId { get; init; }

        [Description("New overall price.")]
        public decimal? Price { get; init; }
        
        [Description("New parts price.")]
        public decimal? PartsPrice { get; init; }

        [Description("New labour cost.")]
        public decimal? LabourPrice { get; init; }

        [Description("New risk cost.")]
        public decimal? RiskCost { get; init; }
    }
}
