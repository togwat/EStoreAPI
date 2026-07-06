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
        [Description("Primary contact detail. Required.")]
        public required string PrimaryContact { get; set; }

        [Description("Optional phone number.")]
        [RegularExpression(@".*[0-9].*", ErrorMessage = "Phone number must contain only numbers.")]
        public string? PhoneNumber { get; set; }

        [Description("Email address.")]
        public string? Email { get; set; }

        [Description("Street address.")]
        public string? Address { get; set; }

        public Customer ToModel() => new()
        {
            CustomerName = CustomerName,
            PrimaryContact = PrimaryContact,
            PhoneNumber = PhoneNumber is null ? null : NonDigits().Replace(PhoneNumber, ""),
            Email = Email,
            Address = Address,
        };
    }
}
