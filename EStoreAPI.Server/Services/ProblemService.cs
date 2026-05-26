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

        public async Task<Problem> CreateProblemAsync(ProblemDTO dto)
        {
            Problem problem = new Problem
            {
                ProblemName = dto.ProblemName,
                DeviceId = dto.DeviceId,
                Price = dto.Price
            };

            return await _repo.AddProblemAsync(problem);
        }

        public async Task<ICollection<Problem>> CreateProblemsAsync(ICollection<ProblemDTO> dtos)
        {
            ICollection<Problem> problems = dtos.Select(dto => new Problem
            {
                ProblemName = dto.ProblemName,
                DeviceId = dto.DeviceId,
                Price = dto.Price
            }).ToList();

            return await _repo.AddProblemsAsync(problems);
        }

        public async Task UpdateProblemAsync(int id, ProblemDTO dto)
        {
            // set new problem
            Problem problem = new Problem
            {
                ProblemId = id,
                ProblemName = dto.ProblemName,
                DeviceId = dto.DeviceId,
                Price = dto.Price
            };

            await _repo.UpdateProblemAsync(problem);
        }
    }
}