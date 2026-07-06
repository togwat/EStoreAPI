using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Data
{
    public interface IEStoreRepo
    {
        // customer operations
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByContactAsync(string contact);
        Task<ICollection<Customer>> GetCustomersAsync();
        // query by name, phone, or email
        Task<ICollection<Customer>> GetCustomersByQueryAsync(string query);
        Task<Customer> AddCustomerAsync(Customer customer);
        // bulk
        Task<ICollection<Customer>> AddCustomersAsync(ICollection<Customer> customers);

        // device operations
        Task<Device?> GetDeviceByIdAsync(int id);
        Task<ICollection<Device>> GetDevicesAsync();
        Task<ICollection<string>> GetDeviceTypesAsync();
        Task<ICollection<Device>> GetDevicesByNameAsync(string name);
        Task<ICollection<Device>> GetDevicesByModelNumberAsync(string modelNumber);
        Task<ICollection<Device>> GetDevicesByTypeAsync(string type);
        Task<Device> AddDeviceAsync(Device device);
        // bulk
        Task<ICollection<Device>> AddDevicesAsync(ICollection<Device> devices);

        // problem operations
        Task<Problem?> GetProblemByIdAsync(int id);
        Task<ICollection<Problem>> GetProblemsByIdsAsync(ICollection<int> ids);
        Task<ICollection<Problem>> GetProblemsOfDeviceAsync(int deviceId);
        Task<Problem> AddProblemAsync(Problem problem);
        // bulk
        Task<ICollection<Problem>> AddProblemsAsync(ICollection<Problem> problems);
        Task UpdateDeviceProblemsAsync(ICollection<Problem> toDelete, ICollection<Problem> toUpdate, ICollection<Problem> toAdd);

        // job operations
        Task<Job?> GetJobByIdAsync(int id);
        Task<ICollection<Job>> GetJobsAsync();
        Task<ICollection<Job>> GetJobsOfCustomerAsync(int customerId);
        Task<ICollection<Job>> GetJobsOfDeviceAsync(int deviceId);
        Task<Job> AddJobAsync(Job job);
        // bulk
        Task<ICollection<Job>> AddJobsAsync(ICollection<Job> jobs);
        Task ApplyUpdateAsync();
    }
}
