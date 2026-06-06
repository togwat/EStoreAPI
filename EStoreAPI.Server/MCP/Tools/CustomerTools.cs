using ModelContextProtocol.Server;
using EStoreAPI.Server.Services;
using System.ComponentModel;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using System.ComponentModel.DataAnnotations;

[McpServerToolType]
public class CustomerTools
{
    private readonly ICustomerService _service;

    public CustomerTools(ICustomerService service)
    {
        _service = service;
    }
    
    [McpServerTool, Description("Search for customers by name, phone number, or email address.")]
    public async Task<ICollection<OutCustomerDTO>> SearchCustomersAsync(
        [Description("The search query to match against customer name, phone number, or email address. Partial matches are supported. Leave empty to get all customers.")]string? query)
    {
        ICollection<Customer> customers = await _service.SearchCustomersAsync(query);
        return customers.Select(OutCustomerDTO.FromModel).ToList();
    }

    [McpServerTool, Description("Create one or more new customers and add them into the database. Check if customers exist using search before creating.")]
    public async Task<ICollection<OutCustomerDTO>> CreateCustomersAsync(
        [Description("A list of customers to create.")] ICollection<InCustomerDTO> dtos)
    {
        try
        {
            ICollection<Customer> customers = await _service.CreateCustomersAsync(dtos);
            return customers.Select(OutCustomerDTO.FromModel).ToList();
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Update a customer's details. Only provide the fields that need to change. Omitted fields keep their current values.")]
    public async Task<OutCustomerDTO> UpdateCustomerAsync(
        [Description("The ID of the customer to update.")] int customerId,
        [Description("New customer name.")] string? customerName = null,
        [Description("New primary phone number.")] string? phoneNumber = null,
        [Description("New secondary phone number.")] string? phoneNumberSecondary = null,
        [Description("New email address.")] string? email = null,
        [Description("New street address.")] string? address = null)
    {
        Customer existing = await _service.GetCustomerAsync(customerId)
            ?? throw new KeyNotFoundException($"Customer {customerId} not found.");

        InCustomerDTO dto = new()
        {
            CustomerName = customerName ?? existing.CustomerName,
            PhoneNumber = phoneNumber ?? existing.PhoneNumber,
            PhoneNumberSecondary = phoneNumberSecondary ?? existing.PhoneNumberSecondary,
            Email = email ?? existing.Email,
            Address = address ?? existing.Address,
        };

        await _service.UpdateCustomerAsync(customerId, dto);
        Customer updated = await _service.GetCustomerAsync(customerId)
            ?? throw new Exception("Failed to retrieve updated customer.");
        return OutCustomerDTO.FromModel(updated);
    }
}