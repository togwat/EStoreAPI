using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Data
{
    public interface IEStoreRepo
    {
        // customer operations
        Task<ICollection<Customer>> GetCustomersAsync();
        // query by name, phone, or email
        Task<ICollection<Customer>> GetCustomersByQueryAsync(string query);
        Task<Customer> AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);

        // device operations
        Task<Device?> GetDeviceByNameAsync(string name);
        Task<ICollection<Device>> GetDevicesByTypeAsync(string type);
        Task<Device> AddDeviceAsync(Device device);
        Task UpdateDeviceAsync(Device device);

        // problem operations
        Task<ICollection<Problem>> GetProblemsOfDeviceAsync(Device device);
        Task<Problem> AddProblemAsync(Problem problem);
        Task UpdateProblemAsync(Problem problem);

        // job operations
        Task<ICollection<Job>> GetJobsAsync();
        Task<Job> GetJobByQueryAsync(string query);
        Task<Job> AddJobAsync(Job job);
        Task UpdateJobAsync(Job job);
    }
}
