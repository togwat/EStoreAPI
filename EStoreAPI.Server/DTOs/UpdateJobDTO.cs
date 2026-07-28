using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public record UpdateJobDTO
    {   
        [Required]
        [Description("The ID of the job to update.")]
        public int JobId { get; set; }

        [Description("Time the device was picked up by the customer, in UTC.")]
        public DateTime? PickupTime { get; init; }

        [Description("Updated estimated pickup time, in UTC.")]
        public DateTime? EstimatedPickupTime { get; init; }

        [Description("Updated note.")]
        public string? Note { get; init; }

        [Description("Updated list of problem IDs. Replaces all current problems if provided.")]
        public List<int>? ProblemIds { get; init; }

        [Description("Updated estimated price.")]
        public decimal? EstimatedPrice { get; init; }

        [Description("Updated collected price.")]
        public decimal? CollectedPrice { get; init; }

        [EnumDataType(typeof(JobStatus))]
        [Description("The status of the job.")]
        public JobStatus? Status { get; init; }

        [Description("ID of the prior job this one is a warranty for. Set to link this job as a warranty follow-up. Search the customer's jobs to find the ID.")]
        public int? WarrantyOfJobId { get; init; }
    }
}
