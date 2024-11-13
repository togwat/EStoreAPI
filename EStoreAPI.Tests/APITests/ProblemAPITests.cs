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
    }
}
