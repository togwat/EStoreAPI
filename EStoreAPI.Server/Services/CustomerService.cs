using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

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
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
            Customer customer = dto.ToModel();

            return await _repo.AddCustomerAsync(customer);
        }

        public async Task<ICollection<Customer>> CreateCustomersAsync(ICollection<InCustomerDTO> dtos)
        {
            foreach (InCustomerDTO dto in dtos)
            {
                Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 
            }

            ICollection<Customer> customers = dtos.Select(dto => dto.ToModel()).ToList();

            return await _repo.AddCustomersAsync(customers);
        }

        public async Task UpdateCustomerAsync(int id, InCustomerDTO dto)
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

            // set up new customer
            Customer customer = dto.ToModel();
            customer.CustomerId = id;

            await _repo.UpdateCustomerAsync(customer);
        }
    }
}