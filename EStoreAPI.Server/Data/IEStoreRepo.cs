using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Data
{
    public interface IEStoreRepo
    {
        // customer operations
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<ICollection<Customer>> GetCustomersAsync();
        // query by name, phone, or email
        Task<ICollection<Customer>> GetCustomersByQueryAsync(string query);
        Task<Customer> AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);

        // device operations
        Task<Device?> GetDeviceByIdAsync(int id);
        Task<ICollection<Device>> GetDevicesAsync();
        Task<ICollection<Device>> GetDevicesByNameAsync(string name);
        Task<ICollection<Device>> GetDevicesByTypeAsync(string type);
        Task<Device> AddDeviceAsync(Device device);
        Task UpdateDeviceAsync(Device device);

        // problem operations
        Task<Problem?> GetProblemByIdAsync(int id);
        Task<ICollection<Problem>> GetProblemsOfDeviceAsync(Device device);
        Task<Problem> AddProblemAsync(Problem problem);
        Task UpdateProblemAsync(Problem problem);

        // job operations
        Task<Job?> GetJobByIdAsync(int id);
        Task<ICollection<Job>> GetJobsAsync();
        Task<Job> GetJobByQueryAsync(string query);
        Task<Job> AddJobAsync(Job job);
        Task UpdateJobAsync(Job job);
    }
}
