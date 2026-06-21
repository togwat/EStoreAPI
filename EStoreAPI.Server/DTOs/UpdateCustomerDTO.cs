using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.DTOs
{
    public class UpdateCustomerDTO
    {   
        [Required]
        [Description("The ID of the customer to update.")]
        public required int CustomerId { get; set; }

        [Description("New customer name.")]
        public string? CustomerName { get; set; }

        [Description("New primary phone number.")]
        [RegularExpression(@".*[0-9].*", ErrorMessage = "Phone number must contain only numbers.")]
        public string? PhoneNumber { get; set; }

        [Description("New secondary phone number.")]
        [RegularExpression(@".*[0-9].*", ErrorMessage = "Phone number must contain only numbers.")]
        public string? PhoneNumberSecondary { get; set; }

        [Description("New email address.")]
        public string? Email { get; set; }

        [Description("New street address.")]
        public string? Address { get; set; }
    }
}
