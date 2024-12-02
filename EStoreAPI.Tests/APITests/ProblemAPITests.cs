using AutoFixture;
using Moq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Tests.APITests
{
    public class ProblemAPITests : APITests<ProblemsController>
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
            _repo.Setup(r => r.GetProblemByIdAsync(1)).ReturnsAsync(problem);
            _repo.Setup(r => r.GetProblemByIdAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Problem); // return null for invalid ids

            // act
            var result = await _controller.GetProblemAsync(id);

            // assert
            // valid id
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);  // returns 200 ok
                var problemResult = Assert.IsAssignableFrom<Problem>(okResult.Value);   // return type Problem
                Assert.Equal(problem.ProblemId, problemResult.ProblemId);   // matching id
            }
            // invalid id
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
            Device device = _fixture.Build<Device>()
                                .With(d => d.DeviceId, 1)
                                .Create();
            var problems = _fixture.Build<Problem>()
                                .With(p => p.DeviceId, id)
                                .CreateMany(5).ToList();
            _repo.Setup(r => r.GetProblemsOfDeviceAsync(1)).ReturnsAsync(problems);
            _repo.Setup(r => r.GetDeviceByIdAsync(1)).ReturnsAsync(device);
            _repo.Setup(r => r.GetDeviceByIdAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Device);

            // act
            var result = await _controller.GetDeviceProblemsAsync(id);

            // assert
            // valid device
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var problemsResult = Assert.IsAssignableFrom<ICollection<Problem>>(okResult.Value); // return type ICollection<Problem>

                Assert.Equal(problems.Count, problemsResult.Count); // returns 5 problems
                Assert.All(problemsResult, p => Assert.Equal(id, p.DeviceId));    // all returned problems should contain device
            }
            // invalid device
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        // POST: api/Problems/create
        [Theory]
        [MemberData(nameof(CreateProblemData))]
        public async Task TestCreateProblem(string name, int deviceId, decimal? price)
        {
            // arrange
            Device validDevice = _fixture.Build<Device>()
                                    .With(d => d.DeviceId, 1)
                                    .Create();
            Device givenDevice = _fixture.Build<Device>()
                                    .With(d => d.DeviceId, deviceId)
                                    .Create();

            Problem newProblem = _fixture.Build<Problem>()
                                    .Without(p => p.ProblemId)
                                    .With(p => p.ProblemName, name)
                                    .With(p => p.Price, price)
                                    .With(p => p.Device, givenDevice)
                                    .Create();
            // valid data
            if (name == "name" && deviceId == 1 && price == 100.00m)
            {
                _repo.Setup(r => r.AddProblemAsync(newProblem))
                .ReturnsAsync((Problem p) =>
                {
                    p.ProblemId = 1;    // auto incremented id
                    return p;
                });
            }
            else
            {
                _repo.Setup(r => r.AddProblemAsync(newProblem)).ThrowsAsync(new ValidationException());
            }
            
            // act
            var result = await _controller.CreateProblemAsync(newProblem);

            // assert
            // valid problem
            if (name == "name" && deviceId == 1 && price == 100.00m)
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdProblem = Assert.IsAssignableFrom<Problem>(createdResult.Value);     // return type problem

                // returned problem should match the sent problem
                Assert.Equal(newProblem.ProblemName, createdProblem.ProblemName);
                Assert.Equal(newProblem.Price, createdProblem.Price);
                Assert.Equal(newProblem.Device.DeviceId, createdProblem.Device.DeviceId);
            }
            // invalid problem
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        public static IEnumerable<object[]> CreateProblemData =>
            [
                ["name", 1, 100.00m],   // valid
                [null, 1, 100.00m],     // invalid name
                ["name", 2, 100.00m],   // invalid device
                ["name", 1, null],      // invalid price
                [null, -1, null]        // invalid everything 
            ];

        // PUT: api/Problems/update/{id}
        [Theory]
        [MemberData(nameof(UpdateProblemData))]
        public async Task TestUpdateProblem(int id, string name, int deviceId, decimal price)
        {
            // arrange
            Problem oldProblem = _fixture.Build<Problem>()
                                        .With(p => p.ProblemId, 1)
                                        .Create();
            Problem newProblem = _fixture.Build<Problem>()
                                        .With(p => p.ProblemId, 1)
                                        .With(p => p.ProblemName, name)
                                        .With(p => p.DeviceId, deviceId)
                                        .With(p => p.Price, price)
                                        .Create();
            Device device = _fixture.Build<Device>()
                                        .With(d => d.DeviceId, 1)
                                        .Create();
            _repo.Setup(r => r.GetDeviceByIdAsync(1)).ReturnsAsync(device);
            _repo.Setup(r => r.GetDeviceByIdAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Device);
            // valid id
            if (id == 1)
            {
                // valid data
                if (name == "newname" && deviceId == 1 && price == 99.99m)
                {
                    _repo.Setup(r => r.UpdateProblemAsync(newProblem)).Returns(Task.CompletedTask);
                }
                else
                {
                    _repo.Setup(r => r.UpdateProblemAsync(newProblem)).ThrowsAsync(new ValidationException());
                }
            }
            // not found id
            else
            {
                _repo.Setup(r => r.UpdateProblemAsync(newProblem)).ThrowsAsync(new KeyNotFoundException("Problem not found."));
            }
            // act
            var result = await _controller.UpdateProblemWithIdAsync(id, newProblem);

            // assert
            // valid id
            if (id == 1)
            {
                // valid data
                if (name == "newname" && deviceId == 1 && price == 99.99m)
                {
                    Assert.IsType<NoContentResult>(result); // returns 204 no content
                }
                // invalid data
                else
                {
                    Assert.IsType<BadRequestResult>(result);   // returns 400 bad request
                }
            }
            // invalid id
            else
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                Assert.Equal("Problem not found.", notFoundResult.Value);   // matching error message
            }
        }

        public static IEnumerable<object[]> UpdateProblemData =>
            [
                [1, "newname", 1, 99.99m],   // valid
                [2, "newname", 1, 99.99m],   // invalid id, valid data
                [1, null, 1, 99.99m],        // valid id, invalid data
                [1, "newname", 2, 99.99m],   // valid id, invalid data
                [1, "newname", 1, null],     // valid id, invalid data
                [1, null, 2, null],          // valid id, invalid data
                [2, null, 1, 99.99m],        // invalid id, invalid data
                [2, "newname", 2, 99.99m],   // invalid id, invalid data
                [2, "newname", 1, null],     // invalid id, invalid data
                [2, null, 2, null]           // invalid id, invalid data
            ];
    }
}
