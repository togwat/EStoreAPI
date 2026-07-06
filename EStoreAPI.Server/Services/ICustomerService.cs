using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public interface ICustomerService
    {
        Task<ICollection<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerAsync(int id);
        Task<Customer?> GetCustomerByContactAsync(string contact);
        Task<ICollection<Customer>> SearchCustomersAsync(string? query);
        Task<Customer> CreateCustomerAsync(InCustomerDTO dto);
        Task<ICollection<Customer>> CreateCustomersAsync(ICollection<InCustomerDTO> dtos);
        Task UpdateCustomerAsync(UpdateCustomerDTO dto);
        Task UpdateCustomersAsync(ICollection<UpdateCustomerDTO> dtos);
    }
}