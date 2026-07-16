using ModelContextProtocol;
using ModelContextProtocol.Server;
using EStoreAPI.Server.Services;
using System.ComponentModel;
using EStoreAPI.Server.DTOs;

[McpServerToolType]
public class FormTools
{
    private readonly IFormService _service;

    public FormTools(IFormService service)
    {
        _service = service;
    }


    [McpServerTool, Description("Creates a repair job via form. Looks up an existing customer by their primary contact detail or creates a new one if not found. Search for the device and its problems first before filling out the form.")]
    public async Task<OutJobDTO> SubmitFormAsync(
        [Description("Customer's primary contact detail.")] string primaryContact,
        [Description("Name of the device model being repaired.")] string deviceName,
        [Description("List the names of problems the device is being repaired for. At least one problem is required.")]List<string> problems,
        [Description("Name of the customer.")] string? name = null,
        [Description("Customer's optional phone number.")] string? phoneNumber = null,
        [Description("Customer's email address")] string? email = null,
        [Description("Customer's street address")] string? address = null,
        [Description("Estimated total repair price.")] decimal? estimatedPrice = null,
        [Description("Estimated device pickup time for the customer, in UTC.")] DateTime? estimatedPickupTime = null,
        [Description("Any notes on the repair job.")] string? note = null)
    {
        // construct dto
        InFormDTO dto = new()
        {
            Name = name,
            PrimaryContact = primaryContact,
            PhoneNumber = phoneNumber,
            Email = email,
            Address = address,
            DeviceName = deviceName,
            Problems = problems,
            EstimatedPrice = estimatedPrice,
            EstimatedPickupTime = estimatedPickupTime,
            Note = note
        };

        try
        {
            return await _service.SubmitFormAsync(dto);
        }
        catch (KeyNotFoundException ex)
        {
            throw new McpException($"Submission failed: {ex.Message}");
        }
    }
}