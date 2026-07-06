using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public class FormService : IFormService
    {
        private readonly IJobService _jobService;
        private readonly ICustomerService _customerService;
        private readonly IDeviceService _deviceService;
        private readonly IProblemService _problemService;

        public FormService(IJobService jobService, ICustomerService customerService, IDeviceService deviceService, IProblemService problemService)
        {
            _jobService = jobService;
            _customerService = customerService;
            _deviceService = deviceService;
            _problemService = problemService;
        }

        public async Task<OutJobDTO> SubmitFormAsync(InFormDTO dto)
        {
            // validate device name
            ICollection<Device> devices = await _deviceService.SearchDevicesByNameAsync(dto.DeviceName);
            Device device = devices.FirstOrDefault(d => d.DeviceName.Equals(dto.DeviceName, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException($"Device {dto.DeviceName} not found.");

            // validate problems
            ICollection<Problem> problems = await _problemService.GetDeviceProblemsAsync(device.DeviceId);
            // map form problems to existing problems, getting ids
            List<int> problemIds = dto.Problems
                .Select(name => problems.FirstOrDefault(p =>
                    p.ProblemName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ?? throw new KeyNotFoundException($"Problem {name} not found for device {device.DeviceName}."))
                .Select(p => p.ProblemId)
                .ToList();

            // validate customer (keep as last validation)
            // look for customer via primary contact, use if found
            Customer customer = await _customerService.GetCustomerByContactAsync(dto.PrimaryContact)
            // create new customer if not found
            ?? await _customerService.CreateCustomerAsync(new InCustomerDTO
            {
               CustomerName = dto.Name,
               PrimaryContact = dto.PrimaryContact,
               PhoneNumber = dto.PhoneNumber,
               Email = dto.Email,
               Address = dto.Address
            });

            // add new job
            Job job = await _jobService.CreateJobAsync(new InJobDTO
            {
                CustomerId = customer.CustomerId,
                DeviceId = device.DeviceId,
                EstimatedPickupTime = dto.EstimatedPickupTime,
                Note = dto.Note,
                ProblemIds = problemIds,
                EstimatedPrice = dto.EstimatedPrice
            });

            return OutJobDTO.FromModel(job);
        }
    }
}
