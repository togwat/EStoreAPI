using AutoFixture;
using AutoFixture.AutoMoq;
using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.Services;
using EStoreAPI.Tests.APITests;
using Moq;

namespace EStoreAPI.Tests.ServiceTests
{
    public class ProblemServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IEStoreRepo> _repo;
        private readonly ProblemService _problemService;

        public ProblemServiceTests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new NoCircularReferencesCustomization())
                .Customize(new IgnoreVirtualMembersCustomization());

            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _problemService = new ProblemService(_repo.Object);
        }

        // an existing id resolves to that problem
        [Fact]
        public async Task GetProblem_Exists_ReturnsProblem()
        {
            var problem = _fixture.Create<Problem>();
            _repo.Setup(r => r.GetProblemByIdAsync(problem.ProblemId)).ReturnsAsync(problem);

            var result = await _problemService.GetProblemAsync(problem.ProblemId);

            Assert.Equal(problem.ProblemId, result?.ProblemId);
        }

        // an unknown id returns null
        [Fact]
        public async Task GetProblem_NotFound_ReturnsNull()
        {
            _repo.Setup(r => r.GetProblemByIdAsync(It.IsAny<int>())).ReturnsAsync((Problem?)null);

            var result = await _problemService.GetProblemAsync(404);

            Assert.Null(result);
        }

        // the problem catalogue for a device is forwarded from the repo
        [Fact]
        public async Task GetDeviceProblems_ReturnsProblems()
        {
            var problems = _fixture.CreateMany<Problem>(3).ToList();
            _repo.Setup(r => r.GetProblemsOfDeviceAsync(7)).ReturnsAsync(problems);

            var result = await _problemService.GetDeviceProblemsAsync(7);

            Assert.Equal(3, result.Count);
        }

        // creating a problem persists it and returns the stored entity
        [Fact]
        public async Task CreateProblem_PersistsAndReturns()
        {
            var dto = _fixture.Create<InProblemDTO>();
            var created = _fixture.Create<Problem>();
            _repo.Setup(r => r.AddProblemAsync(It.IsAny<Problem>())).ReturnsAsync(created);

            var result = await _problemService.CreateProblemAsync(dto);

            Assert.Equal(created.ProblemId, result.ProblemId);
            _repo.Verify(r => r.AddProblemAsync(It.IsAny<Problem>()), Times.Once);
        }

        // bulk create persists every problem
        [Fact]
        public async Task CreateProblems_Bulk_PersistsAll()
        {
            var dtos = _fixture.CreateMany<InProblemDTO>(3).ToList();
            var created = _fixture.CreateMany<Problem>(3).ToList();
            _repo.Setup(r => r.AddProblemsAsync(It.IsAny<ICollection<Problem>>())).ReturnsAsync(created);

            var result = await _problemService.CreateProblemsAsync(dtos);

            Assert.Equal(3, result.Count);
            _repo.Verify(r => r.AddProblemsAsync(It.IsAny<ICollection<Problem>>()), Times.Once);
        }

        // a partially-filled update DTO overwrites only the provided fields, leaving the rest untouched
        [Fact]
        public async Task UpdateProblem_PartialDto_OnlyOverwritesProvidedFields()
        {
            var original = _fixture.Create<Problem>();
            var originalName = original.ProblemName;
            var originalLabour = original.LabourPrice;
            var originalRisk = original.RiskCost;
            var originalDeviceId = original.DeviceId;
            _repo.Setup(r => r.GetProblemByIdAsync(original.ProblemId)).ReturnsAsync(original);

            // only the parts price is supplied
            var dto = new UpdateProblemDTO { ProblemId = original.ProblemId, Price = 99.99m };

            await _problemService.UpdateProblemAsync(dto);

            Assert.Equal(99.99m, original.Price);
            Assert.Equal(originalName, original.ProblemName);
            Assert.Equal(originalLabour, original.LabourPrice);
            Assert.Equal(originalRisk, original.RiskCost);
            Assert.Equal(originalDeviceId, original.DeviceId);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Once);
        }

        // bulk update applies the same partial-overwrite rule to each problem independently
        [Fact]
        public async Task UpdateProblems_Bulk_PartialDtos_OnlyOverwriteProvidedFields()
        {
            var first = _fixture.Create<Problem>();
            var second = _fixture.Create<Problem>();
            var firstOriginalPrice = first.Price;
            var secondOriginalName = second.ProblemName;
            _repo.Setup(r => r.GetProblemByIdAsync(first.ProblemId)).ReturnsAsync(first);
            _repo.Setup(r => r.GetProblemByIdAsync(second.ProblemId)).ReturnsAsync(second);

            var dtos = new List<UpdateProblemDTO>
            {
                new() { ProblemId = first.ProblemId, ProblemName = "renamed" },
                new() { ProblemId = second.ProblemId, LabourPrice = 50m },
            };

            await _problemService.UpdateProblemsAsync(dtos);

            // first: only the name changed
            Assert.Equal("renamed", first.ProblemName);
            Assert.Equal(firstOriginalPrice, first.Price);
            // second: only the labour price changed
            Assert.Equal(50m, second.LabourPrice);
            Assert.Equal(secondOriginalName, second.ProblemName);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.AtLeastOnce);
        }

        // syncing a device's problem catalogue diffs the incoming set against the stored set:
        // a DTO carrying an existing id is an update, a DTO without an id is an add,
        // and a stored problem not referenced by any DTO is a delete
        [Fact]
        public async Task UpdateDeviceProblems_DiffsIntoAddUpdateDelete()
        {
            const int deviceId = 5;
            var existing1 = _fixture.Build<Problem>().With(p => p.ProblemId, 1).With(p => p.DeviceId, deviceId).Create();
            var existing2 = _fixture.Build<Problem>().With(p => p.ProblemId, 2).With(p => p.DeviceId, deviceId).Create();
            _repo.Setup(r => r.GetProblemsOfDeviceAsync(deviceId)).ReturnsAsync([existing1, existing2]);

            var dtos = new List<InProblemDTO>
            {
                // references existing problem 1 -> update
                _fixture.Build<InProblemDTO>().With(p => p.ProblemId, (int?)1).With(p => p.DeviceId, deviceId).Create(),
                // no id -> add
                _fixture.Build<InProblemDTO>().With(p => p.ProblemId, (int?)null).With(p => p.DeviceId, deviceId).Create(),
                // existing problem 2 is absent -> delete
            };

            await _problemService.UpdateDeviceProblemsAsync(deviceId, dtos);

            _repo.Verify(r => r.UpdateDeviceProblemsAsync(
                It.Is<ICollection<Problem>>(toDelete => toDelete.Count == 1 && toDelete.Any(p => p.ProblemId == 2)),
                It.Is<ICollection<Problem>>(toUpdate => toUpdate.Count == 1 && toUpdate.Any(p => p.ProblemId == 1)),
                It.Is<ICollection<Problem>>(toAdd => toAdd.Count == 1)),
                Times.Once);
        }
    }
}
