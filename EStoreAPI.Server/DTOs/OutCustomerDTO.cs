using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.DTOs
{
    public record OutCustomerDTO
    {
        public int CustomerId { get; init; }
        public string? CustomerName { get; init; }
        public required string PrimaryContact { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public string? Address { get; init; }

        public static OutCustomerDTO FromModel(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            CustomerName = c.CustomerName,
            PrimaryContact = c.PrimaryContact,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Address = c.Address,
        };
    }
}
