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
        [Description("The search query to match against customer name, phone number, or email address. Partial matches are supported.")]string? query)
    {
        ICollection<Customer> customers = await _service.SearchCustomersAsync(query);
        return customers.Select(OutCustomerDTO.FromModel).ToList();
    }

    [McpServerTool, Description("Create one or more new customers and add them into the database. Check if customers exist using search before creating.")]
    public async Task<ICollection<OutCustomerDTO>> CreateCustomersAsync(
        [Description("A list of customers to create. Each customer requires: CustomerName, PhoneNumber.")] ICollection<InCustomerDTO> dtos)
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
}