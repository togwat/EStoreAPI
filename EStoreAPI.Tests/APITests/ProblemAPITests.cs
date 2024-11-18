using AutoFixture;
using Moq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Mvc;

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
                Assert.IsType<NotFoundObjectResult>(result.Result); // returns 404 not found
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
            var device = _fixture.Build<Device>()
                                .With(d => d.DeviceId, 1)
                                .Create();
            var problems = _fixture.Build<Problem>()
                                .With(p => p.Device, device)
                                .CreateMany(5).ToList();
            _repo.Setup(r => r.GetProblemsOfDeviceAsync(device)).ReturnsAsync(problems);

            // act
            var result = await _controller.GetDeviceProblemsAsync(device.DeviceId);

            // assert
            // valid device
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var problemsResult = Assert.IsAssignableFrom<ICollection<Problem>>(okResult.Value); // return type ICollection<Problem>

                Assert.Equal(problems.Count, problemsResult.Count); // returns 5 problems
                Assert.All(problemsResult, p => Assert.Equal(device, p.Device));    // all returned problems should contain device
            }
            // invalid device
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        // POST: api/Problems/create
        [Theory]
        [InlineData("name", 1, 100.00)] // valid
        [InlineData(null, 1, 100.00)]   // invalid name
        [InlineData("name", 2, 100.00)] // invalid device
        [InlineData("name", 1, null)]   // invalid price
        [InlineData(null, -1, null)]    // invalid everything
        public async Task TestCreateProblem(string name, int deviceId, decimal? price)
        {
            // arrange
            var validDevice = _fixture.Build<Device>()
                                    .With(d => d.DeviceId, 1)
                                    .Create();
            var givenDevice = _fixture.Build<Device>()
                                    .With(d => d.DeviceId, deviceId)
                                    .Create();

            var newProblem = _fixture.Build<Problem>()
                                    .Without(p => p.ProblemId)
                                    .With(p => p.ProblemName, name)
                                    .With(p => p.Price, price)
                                    .With(p => p.Device, givenDevice)
                                    .Create();
            _repo.Setup(r => r.AddProblemAsync(newProblem))
                .ReturnsAsync((Problem p) =>
                {
                    p.ProblemId = 1;    // auto incremented id
                    return p;
                });

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
    }
}
