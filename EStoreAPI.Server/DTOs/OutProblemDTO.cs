using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public class OutProblemDTO
    {
        public int ProblemId { get; set; }
        public required string ProblemName { get; set; }
        public int DeviceId { get; set; }
        public decimal Price { get; set; }
        public decimal PartsPrice { get; set; }
        public decimal LabourPrice { get; set; }
        public decimal RiskCost { get; set; }

        public static OutProblemDTO FromModel(Problem p) => new()
        {
            ProblemId = p.ProblemId,
            ProblemName = p.ProblemName,
            DeviceId = p.DeviceId,
            Price = p.Price,
            PartsPrice = p.PartsPrice,
            LabourPrice = p.LabourPrice,
            RiskCost = p.RiskCost
        };
    }
}
