using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{   
    // Transaction log entries for job entities
    public class JobLog
    {
        [Key]
        public int JobLogId { get; set; }

        // UTC
        public DateTime Timestamp { get; set; }

        [Required]
        public int JobId { get; set; }
        public virtual Job Job { get; set; }

        public JobStatus? Status { get; set; }

        public string? Note { get; set; }

        public decimal? MoneyChange { get; set; }
    }
}