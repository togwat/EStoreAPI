using AutoFixture;
using Moq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Tests.APITests
{
    public class ProblemAPITests : APITests<ProblemsController, IProblemService>
    {
        // GET: api/Problems/{id}
        [Theory]
        [InlineData(1)]     // valid
        [InlineData(2)]     // invalid
        [InlineData(-1)]    // invalid
        public async Task TestGetProblemById(int id)
        {
            // arrange
            Problem problem = _fixture.Build<Problem>()
                                    .With(p => p.ProblemId, 1)
                                    .Create();
            _service.Setup(s => s.GetProblemAsync(1)).ReturnsAsync(problem);
            _service.Setup(s => s.GetProblemAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Problem);

            // act
            var result = await _controller.GetProblemAsync(id);

            // assert
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var problemResult = Assert.IsAssignableFrom<OutProblemDTO>(okResult.Value);
                Assert.Equal(problem.ProblemId, problemResult.ProblemId);   // matching id
            }
            else
            {
                Assert.IsType<NotFoundResult>(result.Result); // returns 404 not found
            }
        }

        // GET: api/Problems?deviceId=
        [Theory]
        [InlineData(1)]     // valid device
        [InlineData(2)]     // invalid device
        [InlineData(-1)]    // invalid device
        public async Task TestGetProblemByDeviceId(int id)
        {
            // arrange
            var problems = _fixture.Build<Problem>()
                                .With(p => p.DeviceId, 1)
                                .CreateMany(5).ToList();
            _service.Setup(s => s.GetDeviceProblemsAsync(1)).ReturnsAsync(problems);
            _service.Setup(s => s.GetDeviceProblemsAsync(It.Is<int>(i => i != 1)))
                    .ThrowsAsync(new KeyNotFoundException($"Device not found."));

            // act
            var result = await _controller.GetDeviceProblemsAsync(id);

            // assert
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var problemsResult = Assert.IsAssignableFrom<ICollection<OutProblemDTO>>(okResult.Value);

                Assert.Equal(problems.Count, problemsResult.Count); // returns 5 problems
                Assert.All(problemsResult, p => Assert.Equal(1, p.DeviceId));
            }
            else
            {
                Assert.IsType<NotFoundObjectResult>(result.Result); // returns 404 not found
            }
        }

        // POST: api/Problems/create
        [Theory]
        [MemberData(nameof(CreateProblemData))]
        public async Task TestCreateProblem(string name, int deviceId, decimal? price, decimal? labourPrice, decimal? riskCost)
        {
            // arrange
            var dto = new InProblemDTO { ProblemName = name, DeviceId = deviceId, Price = price ?? 0, LabourPrice = labourPrice ?? 0, RiskCost = riskCost ?? 0 };
            Problem newProblem = _fixture.Build<Problem>()
                                    .With(p => p.ProblemId, 1)
                                    .With(p => p.ProblemName, name)
                                    .With(p => p.DeviceId, deviceId)
                                    .With(p => p.Price, price ?? 0)
                                    .With(p => p.LabourPrice, labourPrice ?? 0)
                                    .With(p => p.RiskCost, riskCost ?? 0)
                                    .Create();

            if (name == "name" && deviceId == 1 && price == 100.00m && labourPrice == 50.00m && riskCost == 25.00m)
            {
                _service.Setup(s => s.CreateProblemAsync(dto)).ReturnsAsync(newProblem);
            }
            else
            {
                _service.Setup(s => s.CreateProblemAsync(dto)).ThrowsAsync(new ValidationException());
            }

            // act
            var result = await _controller.CreateProblemAsync(dto);

            // assert
            if (name == "name" && deviceId == 1 && price == 100.00m && labourPrice == 50.00m && riskCost == 25.00m)
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdProblem = Assert.IsAssignableFrom<OutProblemDTO>(createdResult.Value);

                Assert.Equal(newProblem.ProblemName, createdProblem.ProblemName);
                Assert.Equal(newProblem.Price, createdProblem.Price);
                Assert.Equal(newProblem.LabourPrice, createdProblem.LabourPrice);
                Assert.Equal(newProblem.RiskCost, createdProblem.RiskCost);
                Assert.Equal(newProblem.DeviceId, createdProblem.DeviceId);
            }
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        public static IEnumerable<object[]> CreateProblemData =>
            [
                ["name", 1, 100.00m, 50.00m, 25.00m],   // valid
                [null, 1, 100.00m, 50.00m, 25.00m],     // invalid name
                ["name", 2, 100.00m, 50.00m, 25.00m],   // invalid device
                ["name", 1, null, 50.00m, 25.00m],      // invalid price
                [null, -1, null, null, null]          // invalid everything
            ];

        // POST: api/Problems/create-bulk
        [Theory]
        [InlineData(-1)]    // all valid
        [InlineData(0)]     // problem at index 0 is invalid
        [InlineData(1)]     // problem at index 1 is invalid
        public async Task TestCreateProblems(int invalidIndex)
        {
            // arrange
            var dtos = _fixture.Build<InProblemDTO>()
                                .With(p => p.ProblemName, "name")
                                .With(p => p.DeviceId, 1)
                                .With(p => p.Price, 100.00m)
                                .With(p => p.LabourPrice, 50.00m)
                                .With(p => p.RiskCost, 25.00m)
                                .CreateMany(3).ToList();

            if (invalidIndex >= 0)
            {
                _service.Setup(s => s.CreateProblemsAsync(It.IsAny<ICollection<InProblemDTO>>()))
                        .ThrowsAsync(new ValidationException($"Problem at index {invalidIndex} is missing required fields."));
            }
            else
            {
                var newProblems = dtos.Select((d, i) => _fixture.Build<Problem>()
                    .With(p => p.ProblemId, i + 1)
                    .With(p => p.ProblemName, d.ProblemName)
                    .With(p => p.DeviceId, d.DeviceId)
                    .With(p => p.Price, d.Price)
                    .With(p => p.LabourPrice, d.LabourPrice)
                    .With(p => p.RiskCost, d.RiskCost)
                    .Create()).ToList();

                _service.Setup(s => s.CreateProblemsAsync(It.IsAny<ICollection<InProblemDTO>>()))
                        .ReturnsAsync(newProblems);
            }

            // act
            var result = await _controller.CreateProblemsAsync(dtos);

            // assert
            if (invalidIndex < 0)
            {
                var createdResult = Assert.IsType<ObjectResult>(result.Result);    // returns 201 created
                Assert.Equal(201, createdResult.StatusCode);
                var createdProblems = Assert.IsAssignableFrom<ICollection<OutProblemDTO>>(createdResult.Value);
                Assert.Equal(3, createdProblems.Count);
                var createdList = createdProblems.ToList();
                for (int i = 0; i < createdList.Count; i++)
                    Assert.Equal(i + 1, createdList[i].ProblemId);
            }
            else
            {
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);    // returns 400 with message
                Assert.Contains($"index {invalidIndex}", badRequestResult.Value!.ToString());
            }
        }

        // PUT: api/Problems/device/{deviceId}
        [Theory]
        [InlineData(1, false)]   // valid device, no conflict → 204
        [InlineData(2, false)]   // device not found → 404
        [InlineData(1, true)]    // valid device, problem in use by a job → 409
        public async Task TestUpdateDeviceProblems(int deviceId, bool hasConflict)
        {
            // arrange
            var dtos = _fixture.Build<InProblemDTO>()
                                .With(p => p.ProblemName, "name")
                                .With(p => p.Price, 100.00m)
                                .With(p => p.LabourPrice, 50.00m)
                                .With(p => p.RiskCost, 25.00m)
                                .CreateMany(3).ToList();

            if (deviceId != 1)
            {
                _service.Setup(s => s.UpdateProblemsAsync(deviceId, It.IsAny<ICollection<InProblemDTO>>()))
                        .ThrowsAsync(new KeyNotFoundException($"Device {deviceId} not found."));
            }
            else if (hasConflict)
            {
                _service.Setup(s => s.UpdateProblemsAsync(deviceId, It.IsAny<ICollection<InProblemDTO>>()))
                        .ThrowsAsync(new InvalidOperationException("One or more problems are in use by a job and cannot be deleted."));
            }
            else
            {
                _service.Setup(s => s.UpdateProblemsAsync(deviceId, It.IsAny<ICollection<InProblemDTO>>()))
                        .Returns(Task.CompletedTask);
            }

            // act
            var result = await _controller.UpdateDeviceProblemsAsync(deviceId, dtos);

            // assert
            if (deviceId != 1)
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                Assert.Equal($"Device {deviceId} not found.", notFoundResult.Value);
            }
            else if (hasConflict)
            {
                Assert.IsType<ConflictObjectResult>(result);    // returns 409 conflict
            }
            else
            {
                Assert.IsType<NoContentResult>(result); // returns 204 no content
            }
        }

        // PUT: api/Problems/update/{id}
        [Theory]
        [MemberData(nameof(UpdateProblemData))]
        public async Task TestUpdateProblem(int id, string name, int deviceId, decimal price, decimal labourPrice, decimal riskCost)
        {
            // arrange
            var dto = new InProblemDTO { ProblemName = name, DeviceId = deviceId, Price = price, LabourPrice = labourPrice, RiskCost = riskCost };

            if (id == 1)
            {
                if (name == "newname" && deviceId == 1 && price == 99.99m && labourPrice == 49.99m && riskCost == 24.99m)
                    _service.Setup(s => s.UpdateProblemAsync(id, dto)).Returns(Task.CompletedTask);
                else
                    _service.Setup(s => s.UpdateProblemAsync(id, dto)).ThrowsAsync(new ValidationException());
            }
            else
            {
                _service.Setup(s => s.UpdateProblemAsync(id, dto)).ThrowsAsync(new KeyNotFoundException("Problem not found."));
            }

            // act
            var result = await _controller.UpdateProblemWithIdAsync(id, dto);

            // assert
            if (id == 1)
            {
                if (name == "newname" && deviceId == 1 && price == 99.99m && labourPrice == 49.99m && riskCost == 24.99m)
                    Assert.IsType<NoContentResult>(result); // returns 204 no content
                else
                    Assert.IsType<BadRequestResult>(result);    // returns 400 bad request
            }
            else
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                Assert.Equal("Problem not found.", notFoundResult.Value);
            }
        }

        public static IEnumerable<object[]> UpdateProblemData =>
            [
                [1, "newname", 1, 99.99m, 49.99m, 24.99m],   // valid
                [2, "newname", 1, 99.99m, 49.99m, 24.99m],   // invalid id, valid data
                [1, null, 1, 99.99m, 49.99m, 24.99m],        // valid id, invalid name
                [1, "newname", 2, 99.99m, 49.99m, 24.99m],   // valid id, invalid device
                [1, null, 2, 0m, 0m, 0m],                // valid id, invalid data
                [2, null, 1, 99.99m, 49.99m, 24.99m],        // invalid id, invalid data
                [2, "newname", 2, 99.99m, 49.99m, 24.99m],   // invalid id, invalid data
                [2, null, 2, 0m, 0m, 0m]                 // invalid id, invalid data
            ];
    }
}
