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

        public async Task<Customer> CreateCustomerAsync(CustomerDTO dto)
        {
            Customer customer = new Customer
            {
                CustomerName = dto.CustomerName,
                PhoneNumber = dto.PhoneNumber,
                PhoneNumberSecondary = dto.PhoneNumberSecondary,
                Email = dto.Email,
                Address = dto.Address
            };

            return await _repo.AddCustomerAsync(customer);
        }

        public async Task<ICollection<Customer>> CreateCustomersAsync(ICollection<CustomerDTO> dtos)
        {
            ICollection<Customer> customers = dtos.Select(dto => new Customer
            {
                CustomerName = dto.CustomerName,
                PhoneNumber = dto.PhoneNumber,
                PhoneNumberSecondary = dto.PhoneNumberSecondary,
                Email = dto.Email,
                Address = dto.Address
            }).ToList();

            return await _repo.AddCustomersAsync(customers);
        }

        public async Task UpdateCustomerAsync(int id, CustomerDTO dto)
        {
            // set up new customer
            Customer customer = new Customer
            {
                CustomerId = id,
                CustomerName = dto.CustomerName,
                PhoneNumber = dto.PhoneNumber,
                PhoneNumberSecondary = dto.PhoneNumberSecondary,
                Email = dto.Email,
                Address = dto.Address
            };

            await _repo.UpdateCustomerAsync(customer);
        }
    }
}