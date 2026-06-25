using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class UpdateProblemDTO
    {
        [Required]
        [Description("The ID of the problem to update.")]
        public int ProblemId { get; set; }

        [Description("New problem name.")]
        public string? ProblemName { get; set; }

        [Description("New device id.")]
        public int? DeviceId { get; set; }

        [Description("New overall price.")]
        public decimal? Price { get; set; }
        
        [Description("New parts price.")]
        public decimal? PartsPrice { get; set; }

        [Description("New labour cost.")]
        public decimal? LabourPrice { get; set; }

        [Description("New risk cost.")]
        public decimal? RiskCost { get; set; }
    }
}
