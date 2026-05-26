using ModelContextProtocol.Server;
using EStoreAPI.Server.Services;
using System.ComponentModel;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using System.ComponentModel.DataAnnotations;

[McpServerToolType]
public class DeviceTools
{
    private readonly IDeviceService _service;

    public DeviceTools(IDeviceService service)
    {
        _service = service;
    }

    [McpServerTool, Description("Search for device models by model name.")]
    public async Task<ICollection<Device>> SearchDevicesNameAsync(
        [Description("Partial matches supported.")] string name)
    {
        return await _service.SearchDevicesByNameAsync(name);
    }

    [McpServerTool, Description("Search for device models by device type.")]
    public async Task<ICollection<Device>> SearchDevicesByTypeAsync(
        [Description("Partial matches supported.")] string type)
    {
        return await _service.SearchDevicesByTypeAsync(type);
    }

    [McpServerTool, Description("Create one or more new device models and add them to the database.")]
    public async Task<ICollection<Device>> CreateDevicesAsync(
        [Description("A list of devices to create. Each device requires: deviceName, deviceType.")] ICollection<DeviceDTO> dtos)
    {
        try
        {
            return await _service.CreateDevicesAsync(dtos);
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }
    }
}