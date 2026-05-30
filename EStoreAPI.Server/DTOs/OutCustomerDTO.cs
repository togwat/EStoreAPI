using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public class OutCustomerDTO
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public required string PhoneNumber { get; set; }
        public string? PhoneNumberSecondary { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public static OutCustomerDTO FromModel(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            CustomerName = c.CustomerName,
            PhoneNumber = c.PhoneNumber,
            PhoneNumberSecondary = c.PhoneNumberSecondary,
            Email = c.Email,
            Address = c.Address,
        };
    }
}
