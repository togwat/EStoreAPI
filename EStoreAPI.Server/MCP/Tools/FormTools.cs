using ModelContextProtocol.Server;
using EStoreAPI.Server.Services;
using System.ComponentModel;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using System.ComponentModel.DataAnnotations;

[McpServerToolType]
public class FormTools
{
    private readonly IFormService _service;

    public FormTools(IFormService service)
    {
        _service = service;
    }


    [McpServerTool, Description("Creates a repair job via form. Looks up an existing customer by phone number or creates a new one if not found. Search for the device and its problems first before filling out the form.")]
    public async Task<OutJobDTO> SubmitFormAsync(
        [Description("Customer's first phone number.")] string phoneNumber,
        [Description("Name of the device model being repaired.")] string deviceName,
        [Description("List the names of problems the device is being repaired for. At least one problem is required.")]List<string> problems,
        [Description("Name of the customer.")] string? name = null,
        [Description("Customer's second phone number.")] string? phoneNumberSecondary = null,
        [Description("Customer's email address")] string? email = null,
        [Description("Customer's street address")] string? address = null,
        [Description("Estimated total repair price.")] decimal? estimatedPrice = null,
        [Description("Estimated device pickup time for the customer.")] DateTime? estimatedPickupTime = null,
        [Description("Any notes on the repair job.")] string? note = null)
    {
        // construct dto
        InFormDTO dto = new()
        {
            Name = name,
            PhoneNumber = phoneNumber,
            PhoneNumberSecondary = phoneNumberSecondary,
            Email = email,
            Address = address,
            DeviceName = deviceName,
            Problems = problems,
            EstimatedPrice = estimatedPrice,
            EstimatedPickupTime = estimatedPickupTime,
            Note = note
        };

        return await _service.SubmitFormAsync(dto);
    }
}