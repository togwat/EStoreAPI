using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class CustomerDTO
    {
        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string? PhoneNumberSecondary { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }
    }
}
