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

        // POST: api/Jobs/create
        [Theory]
        [MemberData(nameof(CreateJobData))]
        public async Task TestCreateJob(int customerId, int deviceId, DateTime receiveTime, ICollection<Problem> problems)
        {
            // arrange
            var newJob = _fixture.Build<Job>()
                                .Without(j => j.JobId)
                                .With(j => j.CustomerId, customerId)
                                .With(j => j.DeviceId, deviceId)
                                .With(j => j.ReceiveTime, receiveTime)
                                .With(j => j.Problems, problems)
                                .Create();
            _repo.Setup(r => r.AddJobAsync(newJob))
                .ReturnsAsync((Job j) =>
                {
                    j.JobId = 1;
                    return j;
                });

            // act
            var result = await _controller.CreateJobAsync(newJob);

            // assert
            // valid job
            if (customerId == 1 && deviceId == 1 && problems.Count >= 1)
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdJob = Assert.IsAssignableFrom<Job>(createdResult.Value);     // return type Job

                // returned job should match the sent job
                Assert.Equal(newJob.CustomerId, createdJob.CustomerId);
                Assert.Equal(newJob.DeviceId, deviceId);
                Assert.Equal(newJob.ReceiveTime, createdJob.ReceiveTime);
                Assert.Equal(newJob.Problems, createdJob.Problems);
            }
            // invalid job
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        public static IEnumerable<object[]> CreateJobData =>
            [
                [1, 1, new DateTime(2024, 1, 1, 12, 0, 0), new List<Problem> { new() }],    // valid (datetime can be whatever)
                [-1, 1, new DateTime(2024, 1, 1, 12, 0, 0), new List<Problem> { new() }],   // invalid customer
                [1, -1, new DateTime(2024, 1, 1, 12, 0, 0), new List<Problem> { new() }],   // invalid device
                [1, 1, new DateTime(2024, 1, 1, 12, 0, 0), new List<Problem>()],    // invalid problems
                [-1, -1, new DateTime(2024, 1, 1, 12, 0, 0), new List<Problem>()],  // invalid combined
            ];
    }
}