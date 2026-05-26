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
    public async Task<ICollection<Customer>> SearchCustomersAsync(
        [Description("The search query to match against customer name, phone number, or email address. Partial matches are supported.")]string? query)
    {
        return await _service.SearchCustomersAsync(query);
    }

    [McpServerTool, Description("Create one or more new customers and add them into the database.")]
    public async Task<ICollection<Customer>> CreateCustomersAsync(
        [Description("A list of customers to create. Each customer requires: CustomerName, PhoneNumber.")] ICollection<CustomerDTO> dtos)
    {
        try
        {
            return await _service.CreateCustomersAsync(dtos);
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }
    }
}