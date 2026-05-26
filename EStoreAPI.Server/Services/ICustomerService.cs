using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface ICustomerService
    {
        Task<ICollection<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerAsync(int id);
        Task<ICollection<Customer>> SearchCustomersAsync(string? query);
        Task<Customer> CreateCustomerAsync(InCustomerDTO dto);
        Task<ICollection<Customer>> CreateCustomersAsync(ICollection<InCustomerDTO> dtos);
        Task UpdateCustomerAsync(int id, InCustomerDTO dto);
    }
}