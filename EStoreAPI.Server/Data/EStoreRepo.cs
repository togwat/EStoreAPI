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
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            Customer? customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            return customer;
        }

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
            Customer? customerToChange = await GetCustomerByIdAsync(customer.CustomerId);

            if (customerToChange != null)
            {
                customerToChange.CustomerName = customer.CustomerName;
                customerToChange.PhoneNumbers = customer.PhoneNumbers;
                customerToChange.Email = customer.Email;
                customerToChange.Address = customer.Address;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Customer not found.");
            }
        }

        // device operations
        public async Task<Device?> GetDeviceByIdAsync(int id)
        {
            Device? device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.DeviceId == id);
            return device;
        }

        public async Task<ICollection<Device>> GetDevicesAsync()
        {
            ICollection<Device> devices = await _dbContext.Devices.ToListAsync();
            return devices;
        }

        public async Task<ICollection<Device>> GetDevicesByNameAsync(string name)
        {
            ICollection<Device> devices = await _dbContext.Devices.Where(d => d.DeviceName.Contains(name)).ToListAsync();
            return devices;
        }

        public async Task<ICollection<Device>> GetDevicesByTypeAsync(string type)
        {
            ICollection<Device> devices = await _dbContext.Devices.Where(d => d.DeviceType == type).ToListAsync();
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
            Device? deviceToChange = await GetDeviceByIdAsync(device.DeviceId);

            if (deviceToChange != null)
            {
                deviceToChange.DeviceName = device.DeviceName;
                deviceToChange.DeviceType = device.DeviceType;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Device not found.");
            }
        }

        // problem operations
        public async Task<Problem?> GetProblemByIdAsync(int id)
        {
            Problem? problem = await _dbContext.Problems.FirstOrDefaultAsync(p => p.ProblemId == id);
            return problem;
        }

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
            Problem? problemToChange = await GetProblemByIdAsync(problem.ProblemId);

            if (problemToChange != null)
            {
                problemToChange.ProblemName = problem.ProblemName;
                problemToChange.Device = problem.Device;
                problemToChange.Price = problem.Price;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Problem not found.");
            }
        }

        // job operations
        public async Task<Job?> GetJobByIdAsync(int id)
        {
            Job? job = await _dbContext.Jobs.FirstOrDefaultAsync(j => j.JobId == id);
            return job;
        }

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
            Job? jobToChange = await GetJobByIdAsync(job.JobId);

            if (jobToChange != null)
            {
                jobToChange.PickupTime = job.PickupTime;
                jobToChange.EstimatedPickupTime = job.EstimatedPickupTime;
                jobToChange.Note = job.Note;
                jobToChange.Problems = job.Problems;
                jobToChange.EstimatedPrice = job.EstimatedPrice;
                jobToChange.CollectedPrice = job.CollectedPrice;
                jobToChange.IsFinished = job.IsFinished;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Job not found.");
            }
        }
    }
}
