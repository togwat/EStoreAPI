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
    public async Task<ICollection<OutDeviceDTO>> SearchDevicesNameAsync(
        [Description("Partial matches supported.")] string name)
    {   
        ICollection<Device> devices = await _service.SearchDevicesByNameAsync(name);
        return devices.Select(OutDeviceDTO.FromModel).ToList();
    }

    [McpServerTool, Description("Search for device models by device type.")]
    public async Task<ICollection<OutDeviceDTO>> SearchDevicesByTypeAsync(
        [Description("Partial matches supported.")] string type)
    {
        ICollection<Device> devices = await _service.SearchDevicesByTypeAsync(type);
        return devices.Select(OutDeviceDTO.FromModel).ToList();
    }

    [McpServerTool, Description("Create one or more new device models and add them to the database. Check if devices exist using search before creating.")]
    public async Task<ICollection<OutDeviceDTO>> CreateDevicesAsync(
        [Description("A list of devices to create. Each device requires: deviceName, deviceType.")] ICollection<InDeviceDTO> dtos)
    {
        try
        {
            ICollection<Device> devices = await _service.CreateDevicesAsync(dtos);
            return devices.Select(OutDeviceDTO.FromModel).ToList();
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }
    }
}