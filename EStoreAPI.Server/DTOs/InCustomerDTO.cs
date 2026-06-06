using EStoreAPI.Server.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EStoreAPI.Server.DTOs
{
    public partial class InCustomerDTO
    {
        // regex pattern ensures phone numbers are just a string of digits
        [GeneratedRegex(@"\D")]
        private static partial Regex NonDigits();

        [Description("Customer name.")]
        public string? CustomerName { get; set; }

        [Required]
        [Description("Primary phone number. Required.")]
        [RegularExpression(@".*[0-9].*", ErrorMessage = "Phone number must contain only numbers.")]
        public required string PhoneNumber { get; set; }

        [Description("Secondary phone number.")]
        [RegularExpression(@".*[0-9].*", ErrorMessage = "Phone number must contain only numbers.")]
        public string? PhoneNumberSecondary { get; set; }

        [Description("Email address.")]
        public string? Email { get; set; }

        [Description("Street address.")]
        public string? Address { get; set; }

        public Customer ToModel() => new()
        {
            CustomerName = CustomerName,
            PhoneNumber = NonDigits().Replace(PhoneNumber, ""),
            PhoneNumberSecondary = PhoneNumberSecondary is null ? null : NonDigits().Replace(PhoneNumberSecondary, ""),
            Email = Email,
            Address = Address,
        };
    }
}
