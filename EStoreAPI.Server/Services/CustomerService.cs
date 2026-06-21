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
        
        public Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            return _repo.GetCustomerByPhoneAsync(phone);
        }

        // get all customers if empty query
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

        public async Task UpdateCustomerAsync(UpdateCustomerDTO dto)
        {
            await MergeCustomerAsync(dto);
            await _repo.ApplyUpdateAsync();
        }

        public async Task UpdateCustomersAsync(ICollection<UpdateCustomerDTO> dtos)
        {
            foreach (UpdateCustomerDTO dto in dtos)
            {
                await MergeCustomerAsync(dto);
            }

            await _repo.ApplyUpdateAsync();
        }

        // Validate, load, and apply the partial merge onto the tracked entity
        private async Task MergeCustomerAsync(UpdateCustomerDTO dto)
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

            Customer existing = await _repo.GetCustomerByIdAsync(dto.CustomerId)
            ?? throw new KeyNotFoundException($"Customer {dto.CustomerId} not found.");

            // merge
            existing.CustomerName = dto.CustomerName ?? existing.CustomerName;
            existing.PhoneNumber = dto.NormalisedPhone ?? existing.PhoneNumber;
            existing.PhoneNumberSecondary = dto.NormalisedPhoneSecondary ?? existing.PhoneNumberSecondary;
            existing.Email = dto.Email ?? existing.Email;
            existing.Address = dto.Address ?? existing.Address;
        }
    }
}