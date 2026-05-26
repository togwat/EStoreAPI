using ModelContextProtocol.Server;
using EStoreAPI.Server.Services;
using System.ComponentModel;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using System.ComponentModel.DataAnnotations;

[McpServerToolType]
public class ProblemTools
{
    private readonly IProblemService _service;

    public ProblemTools(IProblemService service)
    {
        _service = service;
    }

    [McpServerTool, Description("Get the problem catalogue for a device, listing all potential problems and their service prices.")]
    public async Task<ICollection<Problem>> GetDeviceProblemsAsync(
        [Description("The ID of the device. Retrieve this by searching for the device first.")] int deviceId)
    {
        try
        {
            return await _service.GetDeviceProblemsAsync(deviceId);
        }
        catch (KeyNotFoundException ex)
        {
            throw new Exception($"Device not found: {ex.Message}");
        }
    }

    [McpServerTool, Description("Create one or more problems and add them to the problem catalogue.")]
    public async Task<ICollection<Problem>> CreateProblemsAsync(
        [Description("A list of problems to create and add to the catalogue. Each problem requires: ProblemName, Price, and DeviceId. Retrieve the DeviceId by searching for the device first.")] ICollection<ProblemDTO> dtos)
    {
        try
        {
            return await _service.CreateProblemsAsync(dtos);
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }
    }
}