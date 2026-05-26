using ModelContextProtocol.Server;
using EStoreAPI.Server.Services;
using System.ComponentModel;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using System.ComponentModel.DataAnnotations;

[McpServerToolType]
public class JobTools
{
    private readonly IJobService _service;

    public JobTools(IJobService service)
    {
        _service = service;
    }

    [McpServerTool, Description("Create one or more new repair jobs and add them to the database. Each job links a customer and their device to problems selected from that device's problem catalogue.")]
    public async Task<ICollection<OutJobDTO>> CreateJobsAsync(
        [Description("Jobs to create. Each requires: CustomerId, DeviceId, and at least one ProblemId. Search the customer by name for CustomerId, search the device by name for DeviceId, then use that DeviceId to retrieve the problem catalogue and get the relevant ProblemIds. If this is a new job, collectedPrice should be empty.")] ICollection<InJobDTO> dtos)
    {
        try
        {
            ICollection<Job> newJobs = await _service.CreateJobsAsync(dtos);
            return newJobs.Select(OutJobDTO.FromModel).ToList();
        }
        catch (KeyNotFoundException ex)
        {
            throw new Exception($"Not found: {ex.Message}");
        }
        catch (ValidationException ex)
        {
            throw new Exception($"Validation failed: {ex.Message}");
        }
    }
}