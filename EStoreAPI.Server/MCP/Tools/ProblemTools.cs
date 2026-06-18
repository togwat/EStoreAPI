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
    public async Task<ICollection<OutProblemDTO>> GetDeviceProblemsAsync(
        [Description("The ID of the device. Retrieve this by searching for the device first.")] int deviceId)
    {
        try
        {
            ICollection<Problem> problems = await _service.GetDeviceProblemsAsync(deviceId);
            return problems.Select(OutProblemDTO.FromModel).ToList();
             
        }
        catch (KeyNotFoundException ex)
        {
            throw new Exception($"Device not found: {ex.Message}");
        }
    }

    [McpServerTool, Description("Create one or more problems and add them to the problem catalogue. Check if problems exist for a device before creating.")]
    public async Task<ICollection<OutProblemDTO>> CreateProblemsAsync(
        [Description("A list of problems to create and add to the catalogue. Retrieve the DeviceId by searching for the device first.")] ICollection<InProblemDTO> dtos)
    {
        try
        {
            ICollection<Problem> problems = await _service.CreateProblemsAsync(dtos);
            return problems.Select(OutProblemDTO.FromModel).ToList();
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Update a problem in the problem catalogue. Only provide the fields that need to change. Omitted fields keep their current values.")]
    public async Task<OutProblemDTO> UpdateProblemAsync(
        [Description("The ID of the problem to update.")] int problemId,
        [Description("New problem name.")] string? problemName = null,
        [Description("New parts price.")] decimal? price = null,
        [Description("New labour cost.")] decimal? labourPrice = null)
    {
        Problem existing = await _service.GetProblemAsync(problemId)
            ?? throw new KeyNotFoundException($"Problem {problemId} not found.");

        InProblemDTO dto = new()
        {
            ProblemId = problemId,
            ProblemName = problemName ?? existing.ProblemName,
            DeviceId = existing.DeviceId,
            Price = price ?? existing.Price,
            LabourPrice = labourPrice ?? existing.LabourPrice
        };

        await _service.UpdateProblemAsync(problemId, dto);
        Problem updated = await _service.GetProblemAsync(problemId)
            ?? throw new Exception("Failed to retrieve updated problem.");
        return OutProblemDTO.FromModel(updated);
    }
}