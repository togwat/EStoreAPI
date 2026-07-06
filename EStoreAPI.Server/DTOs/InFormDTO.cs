using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    // one-to-one matches form fields
    public class InFormDTO
    {
        public string? Name { get; set; }
        [Required]
        public required string PrimaryContact { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        [Required]
        public required string DeviceName { get; set; }
        [Required]
        [MinLength(1)]
        public required List<string> Problems { get; set; }
        public decimal? EstimatedPrice { get; set; }
        public DateTime? EstimatedPickupTime { get; set; }
        public string? Note { get; set; }
    }
}
