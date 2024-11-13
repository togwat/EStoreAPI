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
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(-1)]
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
    }
}
