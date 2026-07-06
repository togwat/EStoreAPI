using EStoreAPI.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

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

        public async Task<Customer?> GetCustomerByContactAsync(string contact)
        {
            return await _dbContext.Customers.FirstOrDefaultAsync(c => c.PrimaryContact == contact);
        }

        public async Task<ICollection<Customer>> GetCustomersAsync()
        {
            ICollection<Customer> customers = await _dbContext.Customers.ToListAsync();
            return customers;
        }

        // query by name, primary contact, or email
        public async Task<ICollection<Customer>> GetCustomersByQueryAsync(string query)
        {
            query = query.ToLower();

            ICollection<Customer> customers = await _dbContext.Customers.Where(
                    c => c.CustomerName.ToLower().Contains(query) || c.PrimaryContact.ToLower().Contains(query) || c.Email.ToLower().Contains(query)
                ).ToListAsync();

            return customers;
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            if (customer.CustomerName != null && customer.PrimaryContact != null)
            {
                EntityEntry<Customer> e = await _dbContext.Customers.AddAsync(customer);
                Customer c = e.Entity;
                await _dbContext.SaveChangesAsync();
                return c;
            }
            else
            {
                throw new ValidationException();
            }
        }

        public async Task<ICollection<Customer>> AddCustomersAsync(ICollection<Customer> customers)
        {
            // validation
            var customerList = customers.ToList();
            for (int i = 0; i < customerList.Count; i++)
            {
                Customer customer = customerList[i];

                if (customer.CustomerName == null)
                {
                    throw new ValidationException($"Customer at index {i} is missing a name.");
                }
                if (customer.PrimaryContact == null)
                {
                    throw new ValidationException($"Customer at index {i} is missing a primary contact.");
                }
            }

            await _dbContext.Customers.AddRangeAsync(customerList);
            await _dbContext.SaveChangesAsync();
            return customerList;
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

        public async Task<ICollection<string>> GetDeviceTypesAsync()
        {
            ICollection<string> types = await _dbContext.Devices.Select(d => d.DeviceType).Distinct().ToListAsync();
            return types;
        }

        public async Task<ICollection<Device>> GetDevicesByNameAsync(string name)
        {
            name = name.ToLower();

            ICollection<Device> devices = await _dbContext.Devices.Where(d => d.DeviceName.ToLower().Contains(name)).ToListAsync();
            return devices;
        }

        public async Task<ICollection<Device>> GetDevicesByModelNumberAsync(string modelNumber)
        {
            modelNumber = modelNumber.ToLower();
            
            ICollection<Device> devices = await _dbContext.Devices.Where(d => d.ModelNumber != null && d.ModelNumber.ToLower().Contains(modelNumber)).ToListAsync();
            return devices;
        }

        public async Task<ICollection<Device>> GetDevicesByTypeAsync(string type)
        {
            ICollection<Device> devices = await _dbContext.Devices.Where(d => d.DeviceType == type).ToListAsync();
            return devices;
        }

        public async Task<Device> AddDeviceAsync(Device device)
        {
            if (device.DeviceName != null && device.DeviceType != null)
            {
                EntityEntry<Device> e = await _dbContext.Devices.AddAsync(device);
                Device d = e.Entity;
                await _dbContext.SaveChangesAsync();
                return d;
            }
            else
            {
                throw new ValidationException();
            }
        }

        public async Task<ICollection<Device>> AddDevicesAsync(ICollection<Device> devices)
        {
            // validation
            var devicesList = devices.ToList();
            for (int i = 0; i < devicesList.Count; i++)
            {
                Device device = devicesList[i];

                if (device.DeviceName == null)
                {
                    throw new ValidationException($"Device at index {i} is missing a name.");
                }
                if (device.DeviceType == null)
                {
                    throw new ValidationException($"Device at index {i} is missing a type.");
                }
            }

            await _dbContext.Devices.AddRangeAsync(devicesList);
            await _dbContext.SaveChangesAsync();
            return devicesList;
        }

        // problem operations
        public async Task<Problem?> GetProblemByIdAsync(int id)
        {
            Problem? problem = await _dbContext.Problems.FirstOrDefaultAsync(p => p.ProblemId == id);
            return problem;
        }

        public async Task<ICollection<Problem>> GetProblemsByIdsAsync(ICollection<int> ids)
        {
            return await _dbContext.Problems.Where(p => ids.Contains(p.ProblemId)).ToListAsync();
        }

        public async Task<ICollection<Problem>> GetProblemsOfDeviceAsync(int deviceId)
        {
            ICollection<Problem> problems = await _dbContext.Problems.Where(p => p.DeviceId == deviceId).ToListAsync();
            return problems;
        }

        public async Task<Problem> AddProblemAsync(Problem problem)
        {
            // check if device exists
            Device? device = await GetDeviceByIdAsync(problem.DeviceId);
            if (device != null)
            {
                EntityEntry<Problem> e = await _dbContext.Problems.AddAsync(problem);
                Problem p = e.Entity;
                await _dbContext.SaveChangesAsync();
                return p;
            }
            else
            {
                throw new ValidationException();
            }
        }

        public async Task<ICollection<Problem>> AddProblemsAsync(ICollection<Problem> problems)
        {
            // validation
            var problemList = problems.ToList();
            await ValidateNewProblems(problemList);

            await _dbContext.Problems.AddRangeAsync(problemList);
            await _dbContext.SaveChangesAsync();
            return problemList;
        }

        public async Task UpdateDeviceProblemsAsync(ICollection<Problem> toDelete, ICollection<Problem> toUpdate, ICollection<Problem> toAdd)
        {
                _dbContext.Problems.RemoveRange(toDelete);

                foreach (Problem problem in toUpdate)
                {
                    Problem? existing = await _dbContext.Problems.FindAsync(problem.ProblemId);
                    if (existing is not null)
                    {
                        existing.ProblemName = problem.ProblemName;
                        existing.Price = problem.Price;
                        existing.PartsPrice = problem.PartsPrice;
                        existing.LabourPrice = problem.LabourPrice;
                        existing.RiskCost = problem.RiskCost;
                    }
                }

                var toAddList = toAdd.ToList();
                await ValidateNewProblems(toAddList);
                await _dbContext.Problems.AddRangeAsync(toAddList);

                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                // make sure no deleted problem is being used in a job.
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Database update error. {ex.Message}");
                }
        }

        private async Task ValidateNewProblems(List<Problem> problems)
        {
            for (int i = 0; i < problems.Count; i++)
            {
                Problem problem = problems[i];

                Device? device = await GetDeviceByIdAsync(problem.DeviceId);
                if (problem.ProblemName == null)
                {
                    throw new ValidationException($"Problem at index {i} is missing a name.");
                }
                if (device == null)
                {
                    throw new ValidationException($"Problem at index {i} is missing a valid device.");
                }
            }
        }

        // job operations
        public async Task<Job?> GetJobByIdAsync(int id)
        {
            Job? job = await _dbContext.Jobs.Include(j => j.Problems).FirstOrDefaultAsync(j => j.JobId == id);
            return job;
        }

        public async Task<ICollection<Job>> GetJobsAsync()
        {
            ICollection<Job> jobs = await _dbContext.Jobs.Include(j => j.Problems).ToListAsync();
            return jobs;
        }

        public async Task<ICollection<Job>> GetJobsOfCustomerAsync(int customerId)
        {
            ICollection<Job> jobs = await _dbContext.Jobs.Include(j => j.Problems).Where(j => j.CustomerId == customerId).ToListAsync();
            return jobs;
        }

        public async Task<ICollection<Job>> GetJobsOfDeviceAsync(int deviceId)
        {
            ICollection<Job> jobs = await _dbContext.Jobs.Include(j => j.Problems).Where(j => j.DeviceId == deviceId).ToListAsync();
            return jobs;
        }

        public async Task<Job> AddJobAsync(Job job)
        {
            // check if required attributes are entered
            Customer? customer = await GetCustomerByIdAsync(job.CustomerId);
            Device? device = await GetDeviceByIdAsync(job.DeviceId);
            if (customer != null && device != null)
            {
                EntityEntry<Job> e = await _dbContext.Jobs.AddAsync(job);
                Job j = e.Entity;
                await _dbContext.SaveChangesAsync();
                return j;
            }
            else
            {
                throw new ValidationException();
            }
        }

        public async Task<ICollection<Job>> AddJobsAsync(ICollection<Job> jobs)
        {
            // validation
            var jobList = jobs.ToList();
            for (int i = 0; i < jobList.Count; i++)
            {
                Job job = jobList[i];

                Customer? customer = await GetCustomerByIdAsync(job.CustomerId);
                Device? device = await GetDeviceByIdAsync(job.DeviceId);
                if (customer == null)
                {
                    throw new ValidationException($"Job at index {i} is missing a valid customer.");
                }
                if (device == null)
                {
                    throw new ValidationException($"Job at index {i} is missing a valid device.");
                }
            }

            await _dbContext.Jobs.AddRangeAsync(jobList);
            await _dbContext.SaveChangesAsync();
            return jobList;
        }
    
        public async Task ApplyUpdateAsync()
        {
            // use for any update only operations that mutate an object directly
            // object must be retrieved from the repo for EF-core to work
            // validation must happen at the service layer
            await _dbContext.SaveChangesAsync();
        }
    }
}
