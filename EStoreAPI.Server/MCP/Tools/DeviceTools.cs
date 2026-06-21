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

    [McpServerTool, Description("Search for device models by model name or number.")]
    public async Task<ICollection<OutDeviceDTO>> SearchDevicesAsync(
        [Description("Partial matches supported.")] string name)
    {   
        ICollection<Device> devices = await _service.SearchDevicesAsync(name);
        return devices.Select(OutDeviceDTO.FromModel).ToList();
    }

    [McpServerTool, Description("Search for device models by device type.")]
    public async Task<ICollection<OutDeviceDTO>> SearchDevicesTypeAsync(
        [Description("Partial matches supported.")] string type)
    {
        ICollection<Device> devices = await _service.SearchDevicesByTypeAsync(type);
        return devices.Select(OutDeviceDTO.FromModel).ToList();
    }

    [McpServerTool, Description("Create one or more new device models and add them to the database. Check if devices exist using search before creating.")]
    public async Task<ICollection<OutDeviceDTO>> CreateDevicesAsync(
        [Description("A list of devices to create.")] ICollection<InDeviceDTO> dtos)
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

    [McpServerTool, Description("Update one or more existing device models. Only provide the fields that need to change. Omitted fields keep their current values.")]
    public async Task<ICollection<OutDeviceDTO>> UpdateDevicesAsync(
        [Description("The devices to update. Each must include its DeviceId.")] ICollection<UpdateDeviceDTO> dtos)
    {
        try
        {
            await _service.UpdateDevicesAsync(dtos);
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
        List<OutDeviceDTO> updated = new();
        foreach (UpdateDeviceDTO dto in dtos)
        {
            Device device = await _service.GetDeviceAsync(dto.DeviceId)
                ?? throw new Exception($"Failed to retrieve updated device {dto.DeviceId}.");
            updated.Add(OutDeviceDTO.FromModel(device));
        }
        return updated;
    }

}