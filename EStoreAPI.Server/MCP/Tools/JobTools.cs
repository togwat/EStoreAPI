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

    [McpServerTool, Description("Get the repair jobs a customer has.")]
    public async Task<ICollection<OutJobDTO>> GetCustomerJobsAsync(
        [Description("The ID of the customer. Retrieve this by searching for the customer first.")] int customerId)
    {
        try
        {
            ICollection<Job> jobs = await _service.GetCustomerJobsAsync(customerId);
            return jobs.Select(OutJobDTO.FromModel).ToList();
             
        }
        catch (KeyNotFoundException ex)
        {
            throw new Exception($"Customer not found: {ex.Message}");
        }
    }

    [McpServerTool, Description("Create one or more new repair jobs and add them to the database. Each job links a customer and their device to problems selected from that device's problem catalogue.")]
    public async Task<ICollection<OutJobDTO>> CreateJobsAsync(
        [Description("Jobs to create. Each requires: CustomerId, DeviceId, and at least one ProblemId. Search the customer by name for CustomerId, search the device by name for DeviceId, then use that DeviceId to retrieve the problem catalogue and get the relevant ProblemIds. Do not write in fields the user did not specify.")] ICollection<InJobDTO> dtos)
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

    [McpServerTool, Description("Update a repair job. Only provide the fields that need to change. Omitted fields keep their current values.")]
    public async Task<OutJobDTO> UpdateJobAsync(
        [Description("The ID of the job to update.")] int jobId,
        [Description("Time the device was picked up by the customer.")] DateTime? pickupTime = null,
        [Description("Updated estimated pickup time.")] DateTime? estimatedPickupTime = null,
        [Description("Updated note.")] string? note = null,
        [Description("Updated list of problem IDs. Replaces all current problems if provided.")] List<int>? problemIds = null,
        [Description("Updated estimated price.")] decimal? estimatedPrice = null,
        [Description("Updated collected price.")] decimal? collectedPrice = null,
        [Description("Whether the job is finished.")] bool? isFinished = null)
    {
        Job existing = await _service.GetJobAsync(jobId)
            ?? throw new KeyNotFoundException($"Job {jobId} not found.");

        InJobDTO dto = new()
        {
            CustomerId = existing.CustomerId,
            DeviceId = existing.DeviceId,
            ReceiveTime = existing.ReceiveTime,
            PickupTime = pickupTime ?? existing.PickupTime,
            EstimatedPickupTime = estimatedPickupTime ?? existing.EstimatedPickupTime,
            Note = note ?? existing.Note,
            ProblemIds = problemIds ?? existing.Problems.Select(p => p.ProblemId).ToList(),
            EstimatedPrice = estimatedPrice ?? existing.EstimatedPrice,
            CollectedPrice = collectedPrice ?? existing.CollectedPrice,
            IsFinished = isFinished ?? existing.IsFinished,
        };

        await _service.UpdateJobAsync(jobId, dto);
        Job updated = await _service.GetJobAsync(jobId)
            ?? throw new Exception("Failed to retrieve updated job.");
        return OutJobDTO.FromModel(updated);
    }
}