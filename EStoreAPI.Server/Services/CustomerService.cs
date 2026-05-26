using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IEStoreRepo _repo;
        public CustomerService(IEStoreRepo repo)
        {
            _repo = repo;
        }

        public Task<ICollection<Customer>> GetAllCustomersAsync()
        {
            return _repo.GetCustomersAsync();
        }

        public Task<Customer?> GetCustomerAsync(int id)
        {
            return _repo.GetCustomerByIdAsync(id);
        }

        public Task<ICollection<Customer>> SearchCustomersAsync(string? query)
        {
            return query is null ? _repo.GetCustomersAsync() : _repo.GetCustomersByQueryAsync(query);
        }

        public async Task<Customer> CreateCustomerAsync(InCustomerDTO dto)
        {
            Customer customer = dto.ToModel();

            return await _repo.AddCustomerAsync(customer);
        }

        public async Task<ICollection<Customer>> CreateCustomersAsync(ICollection<InCustomerDTO> dtos)
        {
            ICollection<Customer> customers = dtos.Select(dto => dto.ToModel()).ToList();

            return await _repo.AddCustomersAsync(customers);
        }

        public async Task UpdateCustomerAsync(int id, InCustomerDTO dto)
        {
            // set up new customer
            Customer customer = dto.ToModel();

            await _repo.UpdateCustomerAsync(customer);
        }
    }
}