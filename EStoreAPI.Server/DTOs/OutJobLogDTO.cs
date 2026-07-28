using EStoreAPI.Server.Models;


namespace EStoreAPI.Server.DTOs
{   
    // Transaction log entries for job entities
    public record OutJobLogDTO
    {
        public int JobLogId { get; init; }
        // UTC
        public DateTime Timestamp { get; init; }
        public JobStatus? Status { get; init; }
        public string? Note { get; init; }
        public decimal? MoneyChange { get; init; }
    
        public static OutJobLogDTO FromModel(JobLog l) => new()
        {
            JobLogId = l.JobLogId,
            Timestamp = l.Timestamp,
            Status = l.Status,
            Note = l.Note,
            MoneyChange = l.MoneyChange
        };
    }
}