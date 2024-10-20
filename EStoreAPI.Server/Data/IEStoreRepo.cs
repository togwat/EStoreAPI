using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Data
{
    public interface IEStoreRepo
    {
        // customer operations
        Task<ICollection<Customer>> GetCustomersAsync();
        Task<ICollection<Customer>> GetCustomersByQueryAsync(string query);
        Task AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);

        // device operations
        Task<Device> GetDeviceByNameAsync(string name);
        Task<ICollection<Device>> GetDevicesByTypeAsync(string type);
        Task AddDeviceAsync(Device device);
        Task UpdateDeviceAsync(Device device);

        // problem operations
        Task<ICollection<Problem>> GetProblemsOfDeviceAsync(Device device);
        Task AddProblemAsync(Problem problem);
        Task UpdateProblemAsync(Problem problem);

        // job operations
        Task<ICollection<Job>> GetJobsAsync();
        Task<Job> GetJobByQueryAsync(string query);
        Task AddJobAsync(Job job);
        Task UpdateJobAsync(Job job);
    }
}
