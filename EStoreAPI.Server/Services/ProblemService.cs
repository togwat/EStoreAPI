using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;

namespace EStoreAPI.Server.Services
{
    public class ProblemService : IProblemService
    {
        private readonly IEStoreRepo _repo;
        public ProblemService(IEStoreRepo repo)
        {
            _repo = repo;
        }

        public Task<Problem?> GetProblemAsync(int id)
        {
            return _repo.GetProblemByIdAsync(id);
        }

        public async Task<ICollection<Problem>> GetDeviceProblemsAsync(int deviceId)
        {
            // check if device exists
            Device? device = await _repo.GetDeviceByIdAsync(deviceId);

            if (device is null)
            {
                throw new KeyNotFoundException($"Device {deviceId} not found.");
            }
            else
            {
                return await _repo.GetProblemsOfDeviceAsync(deviceId);
            }
        }

        public async Task<Problem> CreateProblemAsync(InProblemDTO dto)
        {
            Problem problem = dto.ToModel();

            return await _repo.AddProblemAsync(problem);
        }

        public async Task<ICollection<Problem>> CreateProblemsAsync(ICollection<InProblemDTO> dtos)
        {
            ICollection<Problem> problems = dtos.Select(dto => dto.ToModel()).ToList();

            return await _repo.AddProblemsAsync(problems);
        }

        public async Task UpdateProblemAsync(int id, InProblemDTO dto)
        {
            // set new problem
            Problem problem = dto.ToModel();

            await _repo.UpdateProblemAsync(problem);
        }
    }
}