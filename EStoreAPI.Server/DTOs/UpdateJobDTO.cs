using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class UpdateJobDTO
    {   
        [Required]
        [Description("The ID of the job to update.")]
        public int JobId { get; set; }

        [Description("Time the device was picked up by the customer.")]
        public DateTime? PickupTime { get; set; }

        [Description("Updated estimated pickup time.")]
        public DateTime? EstimatedPickupTime { get; set; }

        [Description("Updated note.")]
        public string? Note { get; set; }

        [Description("Updated list of problem IDs. Replaces all current problems if provided.")]
        public List<int>? ProblemIds { get; set; }

        [Description("Updated estimated price.")]
        public decimal? EstimatedPrice { get; set; }

        [Description("Updated collected price.")]
        public decimal? CollectedPrice { get; set; }

        [Description("Whether the job is finished.")]
        public bool? IsFinished { get; set; }
    }
}
