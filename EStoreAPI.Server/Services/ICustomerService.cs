using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface ICustomerService
    {
        Task<ICollection<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerAsync(int id);
        Task<ICollection<Customer>> SearchCustomersAsync(string? query);
        Task<Customer> CreateCustomerAsync(CustomerDTO dto);
        Task<ICollection<Customer>> CreateCustomersAsync(ICollection<CustomerDTO> dtos);
        Task UpdateCustomerAsync(int id, CustomerDTO dto);
    }
}