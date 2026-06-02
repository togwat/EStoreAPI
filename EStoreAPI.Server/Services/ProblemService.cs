using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using System.ComponentModel.DataAnnotations;

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
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 
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
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 

            // set new problem
            Problem problem = dto.ToModel();
            problem.ProblemId = id;

            await _repo.UpdateProblemAsync(problem);
        }

        public async Task UpdateProblemsAsync(int deviceId, ICollection<InProblemDTO> dtos)
        {
            foreach (InProblemDTO dto in dtos)
            {
                Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true); 
            }

            ICollection<Problem> existingProblems = await _repo.GetProblemsOfDeviceAsync(deviceId);
            ICollection<int> existingIds = existingProblems.Select(p => p.ProblemId).ToHashSet();
            // get all incoming ids, excluding dtos without ids
            ICollection<int> incomingIds = dtos.Where(d => d.ProblemId.HasValue)
                .Select(d => d.ProblemId!.Value).ToHashSet();

            // split dto into 3 collections: toDelete, toUpdate, toAdd
            // delete all devices in existing but not in incoming
            List<Problem> toDelete = existingProblems.Where(p => !incomingIds.Contains(p.ProblemId)).ToList();

            // update all devices in existing and in incoming
            List<Problem> toUpdate = dtos.Where(d => d.ProblemId.HasValue && existingIds.Contains(d.ProblemId.Value))   // find intersection
                .Select(d => { Problem p = d.ToModel(); p.DeviceId = deviceId; return p; })  // convert to domain model object
                .ToList();

            // add devices not in existing but in incoming
            List<Problem> toAdd = dtos.Where(d => !d.ProblemId.HasValue)    // find dtos with no id
                .Select(d => { Problem p = d.ToModel(); p.DeviceId = deviceId; return p; })
                .ToList();

            await _repo.UpdateDeviceProblemsAsync(toDelete, toUpdate, toAdd);
        }
    }
}