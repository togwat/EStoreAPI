using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public record OutProblemDTO
    {
        public int ProblemId { get; init; }
        public required string ProblemName { get; init; }
        public int DeviceId { get; init; }
        public decimal Price { get; init; }
        public decimal PartsPrice { get; init; }
        public decimal LabourPrice { get; init; }
        public decimal RiskCost { get; init; }

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
