using EStoreAPI.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EStoreAPI.Server.Data
{
    public class EStoreRepo : IEStoreRepo
    {
        private readonly EStoreDbContext _dbContext;

        public EStoreRepo(EStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // customer operations
        public async Task<ICollection<Customer>> GetCustomersAsync()
        {
            ICollection<Customer> customers = await _dbContext.Customers.ToListAsync();
            return customers;
        }

        // query by name, phone, or email
        public async Task<ICollection<Customer>> GetCustomersByQueryAsync(string query)
        {
            ICollection<Customer> customers = await _dbContext.Customers.Where(
                    c => c.CustomerName.Contains(query) || c.PhoneNumbers.Contains(query) || c.Email.Contains(query)
                ).ToListAsync();

            return customers;
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            EntityEntry<Customer> e = await _dbContext.Customers.AddAsync(customer);
            Customer c = e.Entity;
            await _dbContext.SaveChangesAsync();
            return c;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await _dbContext.Customers.Where(c => c.CustomerId == customer.CustomerId).ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.CustomerName, customer.CustomerName)
                .SetProperty(c => c.PhoneNumbers, customer.PhoneNumbers)
                .SetProperty(c => c.Email, customer.Email)
                .SetProperty(c => c.Address, customer.Address)
            );
        }

        // device operations
        public async Task<Device?> GetDeviceByNameAsync(string name)
        {
            Device? device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.deviceName == name);
            return device;
        }

        public async Task<ICollection<Device>> GetDevicesByTypeAsync(string type)
        {
            ICollection<Device> devices = await _dbContext.Devices.Where(d => d.deviceType == type).ToListAsync();
            return devices;
        }

        public async Task<Device> AddDeviceAsync(Device device)
        {
            EntityEntry<Device> e = await _dbContext.Devices.AddAsync(device);
            Device d = e.Entity;
            await _dbContext.SaveChangesAsync();
            return d;
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            await _dbContext.Devices.Where(d => d.DeviceId == device.DeviceId).ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.deviceName, device.deviceName)
                .SetProperty(d => d.deviceType, device.deviceType)
            );
        }

        // problem operations
        public async Task<ICollection<Problem>> GetProblemsOfDeviceAsync(Device device)
        {
            ICollection<Problem> problems = await _dbContext.Problems.Where(p => p.Device == device).ToListAsync();
            return problems;
        }

        public async Task<Problem> AddProblemAsync(Problem problem)
        {
            EntityEntry<Problem> e = await _dbContext.Problems.AddAsync(problem);
            Problem p = e.Entity;
            await _dbContext.SaveChangesAsync();
            return p;
        }

        public async Task UpdateProblemAsync(Problem problem)
        {
            await _dbContext.Problems.Where(p => p.ProblemId == problem.ProblemId).ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.ProblemName, problem.ProblemName)
                .SetProperty(p => p.Device, problem.Device)
                .SetProperty(p => p.Price, problem.Price)
            );
        }

        // job operations
        public async Task<ICollection<Job>> GetJobsAsync()
        {
            ICollection<Job> jobs = await _dbContext.Jobs.ToListAsync();
            return jobs;
        }

        public async Task<Job> GetJobByQueryAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<Job> AddJobAsync(Job job)
        {
            EntityEntry<Job> e = await _dbContext.Jobs.AddAsync(job);
            Job j = e.Entity;
            await _dbContext.SaveChangesAsync();
            return j;
        }

        public async Task UpdateJobAsync(Job job)
        {
            await _dbContext.Jobs.Where(j => j.JobId == job.JobId).ExecuteUpdateAsync(setters => setters
                .SetProperty(j => j.PickupTime, job.PickupTime)
                .SetProperty(j => j.EstimatedPickupTime, job.EstimatedPickupTime)
                .SetProperty(j => j.Note, job.Note)
                .SetProperty(j => j.Problems, job.Problems)
                .SetProperty(j => j.EstimatedPrice, job.EstimatedPrice)
                .SetProperty(j => j.CollectedPrice, job.CollectedPrice)
                .SetProperty(j => j.IsFinished, job.IsFinished)
            );
        }
    }
}
