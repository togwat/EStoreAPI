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

        public async Task<ICollection<Customer>> GetCustomersByQueryAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            throw new NotImplementedException();
        }

        // device operations
        public async Task<Device> GetDeviceByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Device>> GetDevicesByTypeAsync(string type)
        {
            throw new NotImplementedException();
        }

        public async Task AddDeviceAsync(Device device)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            throw new NotImplementedException();
        }

        // problem operations
        public async Task<ICollection<Problem>> GetProblemsOfDeviceAsync(Device device)
        {
            throw new NotImplementedException();
        }

        public async Task AddProblemAsync(Problem problem)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateProblemAsync(Problem problem)
        {
            throw new NotImplementedException();
        }

        // job operations
        public async Task<ICollection<Job>> GetJobsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Job> GetJobByQueryAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task AddJobAsync(Job job)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateJobAsync(Job job)
        {
            throw new NotImplementedException();
        }
    }
}
