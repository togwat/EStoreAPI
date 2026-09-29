using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    // one-to-one matches form fields
    public record InFormDTO
    {
        public string? Name { get; init; }
        [Required]
        public required string PrimaryContact { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public string? Address { get; init; }
        [Required]
        public required string DeviceName { get; init; }
        [Required]
        [MinLength(1)]
        public required List<string> Problems { get; init; }
        public decimal? EstimatedPrice { get; init; }
        public DateTime? EstimatedPickupTime { get; init; }
        public string? Note { get; init; }
        public DateTime? ReceiveTime { get; init; }
    }
}
