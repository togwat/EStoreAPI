using ModelContextProtocol;
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
            throw new McpException($"Customer not found: {ex.Message}");
        }
    }

    [McpServerTool, Description("Search for repair jobs by customer name, phone number, email, or device name.")]
    public async Task<ICollection<OutJobDTO>> SearchJobsAsync(
        [Description("Partial matches supported.")] string query)
    {
        ICollection<Job> jobs = await _service.SearchJobsAsync(query);
        return jobs.Select(OutJobDTO.FromModel).ToList();
    }

    [McpServerTool, Description("Create one or more new repair jobs and add them to the database. Each job links a customer and their device to problems selected from that device's problem catalogue.")]
    public async Task<ICollection<OutJobDTO>> CreateJobsAsync(
        [Description("Jobs to create. Search the customer by name for CustomerId, search the device by name for DeviceId, then use that DeviceId to retrieve the problem catalogue and get the relevant ProblemIds. Do not write in fields the user did not specify.")] ICollection<InJobDTO> dtos)
    {
        try
        {
            ICollection<Job> newJobs = await _service.CreateJobsAsync(dtos);
            return newJobs.Select(OutJobDTO.FromModel).ToList();
        }
        catch (KeyNotFoundException ex)
        {
            throw new McpException($"Not found: {ex.Message}");
        }
        catch (ValidationException ex)
        {
            throw new McpException($"Validation failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Update one or more existing repair jobs. Only provide the fields that need to change. Omitted fields keep their current values. Providing problemIds completely replaces that job's set of problems.")]
    public async Task<ICollection<OutJobDTO>> UpdateJobsAsync(
        [Description("The jobs to update. Each must include its JobId.")] ICollection<UpdateJobDTO> dtos)
    {
        try
        {
            await _service.UpdateJobsAsync(dtos);
        }
        catch (KeyNotFoundException ex)
        {
            throw new McpException($"Not found: {ex.Message}");
        }
        catch (ValidationException ex)
        {
            throw new McpException($"Validation failed: {ex.Message}");
        }

        // return the updated records
        List<OutJobDTO> updated = new();
        foreach (UpdateJobDTO dto in dtos)
        {
            Job job = await _service.GetJobAsync(dto.JobId)
                ?? throw new McpException($"Failed to retrieve updated job {dto.JobId}.");
            updated.Add(OutJobDTO.FromModel(job));
        }
        return updated;
    }
}