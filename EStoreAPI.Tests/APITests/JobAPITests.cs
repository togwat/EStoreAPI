using AutoFixture;
using Moq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace EStoreAPI.Tests.APITests
{
    public class JobAPITests : APITests<JobsController>
    {
        // GET: api/Jobs
        [Fact]
        public async Task TestGetJobs()
        {
            // arrange
            var jobs = _fixture.CreateMany<Job>(5).ToList();
            _repo.Setup(r => r.GetJobsAsync()).ReturnsAsync(jobs);

            // act
            var result = await _controller.GetAllJobsAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var jobsResult = Assert.IsAssignableFrom<ICollection<Job>>(okResult.Value); // return type ICollection<Job>
            Assert.Equal(5, jobsResult.Count);  // returns 5 jobs
        }

        // GET: api/Jobs/{id}
        [Theory]
        [InlineData(1)]     // valid id
        [InlineData(-1)]    // invalid id
        [InlineData(2)]     // invalid id
        public async Task TestGetJobsById(int id)
        {
            // arrange
            Job job = _fixture.Build<Job>()
                                .With(j => j.JobId, 1)
                                .Create();
            _repo.Setup(r => r.GetJobByIdAsync(1)).ReturnsAsync(job);

            // act
            var result = await _controller.GetJobAsync(id);

            // assert
            // valid id
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var jobResult = Assert.IsAssignableFrom<Job>(okResult.Value);   // return type job
                Assert.Equal(job.JobId, jobResult.JobId);   // matching id
            }
            // invalid id
            else
            {
                Assert.IsType<NotFoundObjectResult>(result.Result); // returns 404 not found
            }
        }
    }
}