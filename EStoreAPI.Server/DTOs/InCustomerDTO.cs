using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EStoreAPI.Server.DTOs
{
    public partial class InCustomerDTO
    {
        // regex pattern ensures phone numbers are just a string of digits
        [GeneratedRegex(@"\D")]
        private static partial Regex NonDigits();
        public string? CustomerName { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        public string? PhoneNumberSecondary { get; set; }
        public string? Email { get; set; }
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
