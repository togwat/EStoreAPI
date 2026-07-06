using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EStoreAPI.Server.DTOs
{
    public partial class UpdateCustomerDTO
    {   
        [GeneratedRegex(@"\D")]
        private static partial Regex NonDigits();

        [Required]
        [Description("The ID of the customer to update.")]
        public int CustomerId { get; set; }

        [Description("New customer name.")]
        public string? CustomerName { get; set; }

        [Description("New primary contact detail.")]
        public string? PrimaryContact { get; set; }

        [Description("New optional phone number.")]
        [RegularExpression(@".*[0-9].*", ErrorMessage = "Phone number must contain only numbers.")]
        public string? PhoneNumber { get; set; }
        public string? NormalisedPhone =>
            PhoneNumber is null ? null : NonDigits().Replace(PhoneNumber, "");

        [Description("New email address.")]
        public string? Email { get; set; }

        [Description("New street address.")]
        public string? Address { get; set; }
    }
}
