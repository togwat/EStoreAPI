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
        [Description("The search query to match against customer name, phone number, or email address. Partial matches are supported.")] string query)
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

    [McpServerTool, Description("Update one or more existing customers. Only provide the fields that need to change. Omitted fields keep their current values.")]
    public async Task<ICollection<OutCustomerDTO>> UpdateCustomersAsync(
        [Description("The customers to update. Each must include its CustomerId.")] ICollection<UpdateCustomerDTO> dtos)
    {
        try
        {
            await _service.UpdateCustomersAsync(dtos);
        }
        catch (KeyNotFoundException ex)
        {
            throw new Exception($"Not found: {ex.Message}");
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }

        // return the updated records
        List<OutCustomerDTO> updated = new();
        foreach (UpdateCustomerDTO dto in dtos)
        {
            Customer customer = await _service.GetCustomerAsync(dto.CustomerId)
                ?? throw new Exception($"Failed to retrieve updated customer {dto.CustomerId}.");
            updated.Add(OutCustomerDTO.FromModel(customer));
        }
        return updated;
    }
}