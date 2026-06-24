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
    public class JobAPITests : APITests<JobsController, IJobService>
    {
        // GET: api/Jobs
        [Fact]
        public async Task TestGetJobs()
        {
            // arrange
            var jobs = _fixture.CreateMany<Job>(5).ToList();
            _service.Setup(s => s.GetAllJobsAsync()).ReturnsAsync(jobs);

            // act
            var result = await _controller.GetAllJobsAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var jobsResult = Assert.IsAssignableFrom<ICollection<OutJobDTO>>(okResult.Value);
            Assert.Equal(5, jobsResult.Count);  // returns 5 jobs
        }

        [Fact]
        public async Task TestGetEmptyJobs()
        {
            // arrange
            _service.Setup(s => s.GetAllJobsAsync()).ReturnsAsync(new List<Job>());

            // act
            var result = await _controller.GetAllJobsAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var jobResult = Assert.IsAssignableFrom<ICollection<OutJobDTO>>(okResult.Value);
            Assert.Empty(jobResult);    // returns empty list
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
            _service.Setup(s => s.GetJobAsync(1)).ReturnsAsync(job);
            _service.Setup(s => s.GetJobAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Job);

            // act
            var result = await _controller.GetJobAsync(id);

            // assert
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var jobResult = Assert.IsAssignableFrom<OutJobDTO>(okResult.Value);
                Assert.Equal(job.JobId, jobResult.JobId);   // matching id
            }
            else
            {
                Assert.IsType<NotFoundResult>(result.Result); // returns 404 not found
            }
        }

        // POST: api/Jobs/create
        [Theory]
        [MemberData(nameof(CreateJobData))]
        public async Task TestCreateJob(bool valid, bool invalidProblems)
        {
            // arrange
            var dto = _fixture.Build<InJobDTO>()
                                .With(j => j.CustomerId, 1)
                                .With(j => j.DeviceId, 1)
                                .With(j => j.ProblemIds, new List<int> { 1 })
                                .Create();
            Job newJob = _fixture.Build<Job>()
                                .With(j => j.JobId, 1)
                                .Create();

            if (valid)
            {
                _service.Setup(s => s.CreateJobAsync(dto)).ReturnsAsync(newJob);
            }
            else if (invalidProblems)
            {
                _service.Setup(s => s.CreateJobAsync(dto)).ThrowsAsync(new KeyNotFoundException("One or more problem IDs are invalid."));
            }
            else
            {
                _service.Setup(s => s.CreateJobAsync(dto)).ThrowsAsync(new ValidationException());
            }

            // act
            var result = await _controller.CreateJobAsync(dto);

            // assert
            if (valid)
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdJob = Assert.IsAssignableFrom<OutJobDTO>(createdResult.Value);

                Assert.Equal(newJob.CustomerId, createdJob.CustomerId);
                Assert.Equal(newJob.DeviceId, createdJob.DeviceId);
                Assert.Equal(newJob.JobId, createdJob.JobId);
            }
            else if (invalidProblems)
            {
                Assert.IsType<BadRequestObjectResult>(result.Result); // returns 400 bad request with message
            }
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        public static IEnumerable<object[]> CreateJobData =>
            [
                [true, false],      // valid
                [false, true],      // invalid problem IDs
                [false, false],     // invalid data (ValidationException)
            ];

        // POST: api/Jobs/create-bulk
        [Theory]
        [InlineData(-1)]    // all valid
        [InlineData(0)]     // job at index 0 has invalid problem IDs
        [InlineData(1)]     // job at index 1 has invalid problem IDs
        public async Task TestCreateJobs(int invalidIndex)
        {
            // arrange
            var dtos = _fixture.Build<InJobDTO>()
                                .With(j => j.CustomerId, 1)
                                .With(j => j.DeviceId, 1)
                                .With(j => j.ProblemIds, new List<int> { 1 })
                                .CreateMany(3).ToList();

            if (invalidIndex >= 0)
            {
                _service.Setup(s => s.CreateJobsAsync(It.IsAny<ICollection<InJobDTO>>()))
                        .ThrowsAsync(new KeyNotFoundException($"One or more problem IDs are invalid for job at index {invalidIndex}."));
            }
            else
            {
                var newJobs = dtos.Select((d, i) => _fixture.Build<Job>()
                    .With(j => j.JobId, i + 1)
                    .Create()).ToList();

                _service.Setup(s => s.CreateJobsAsync(It.IsAny<ICollection<InJobDTO>>()))
                        .ReturnsAsync(newJobs);
            }

            // act
            var result = await _controller.CreateJobsAsync(dtos);

            // assert
            if (invalidIndex < 0)
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdJobs = Assert.IsAssignableFrom<ICollection<OutJobDTO>>(createdResult.Value);
                Assert.Equal(3, createdJobs.Count);
                var createdList = createdJobs.ToList();
                for (int i = 0; i < createdList.Count; i++)
                    Assert.Equal(i + 1, createdList[i].JobId);
            }
            else
            {
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);    // returns 400 with message
                Assert.Contains($"index {invalidIndex}", badRequestResult.Value!.ToString());
            }
        }

        // PUT: api/Jobs/update/{id}
        [Theory]
        [MemberData(nameof(UpdateJobData))]
        public async Task TestUpdateJob(int id, bool invalidProblems)
        {
            // arrange
            var dto = _fixture.Build<UpdateJobDTO>()
                                .With(j => j.ProblemIds, invalidProblems ? new List<int>() : new List<int> { 1 })
                                .Create();

            if (id == 1 && !invalidProblems)
            {
                _service.Setup(s => s.UpdateJobAsync(dto)).Returns(Task.CompletedTask);
            }
            else if (id != 1)
            {
                _service.Setup(s => s.UpdateJobAsync(dto)).ThrowsAsync(new KeyNotFoundException("Job not found."));
            }
            else
            {
                _service.Setup(s => s.UpdateJobAsync(dto)).ThrowsAsync(new ValidationException());
            }

            // act
            var result = await _controller.UpdateJobAsync(id, dto);

            // assert
            if (id == 1 && !invalidProblems)
            {
                Assert.IsType<NoContentResult>(result); // returns 204 no content
            }
            else if (id != 1)
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                Assert.Equal("Job not found.", notFoundResult.Value);
            }
            else
            {
                Assert.IsType<BadRequestResult>(result);    // returns 400 bad request
            }
        }

        public static IEnumerable<object[]> UpdateJobData =>
            [
                [1, false],     // valid id, valid data
                [2, false],     // invalid id, valid data
                [1, true],      // valid id, invalid data (no problems)
            ];

        // GET: api/Jobs/customer/{customerId}
        [Theory]
        [InlineData(1, true)]   // valid customer, has jobs
        [InlineData(1, false)]  // valid customer, no jobs
        [InlineData(2, false)]  // invalid customer
        public async Task TestGetCustomerJobs(int customerId, bool hasJobs)
        {
            // arrange
            var jobs = _fixture.CreateMany<Job>(3).ToList();

            if (customerId == 1 && hasJobs)
                _service.Setup(s => s.GetCustomerJobsAsync(1)).ReturnsAsync(jobs);
            else if (customerId == 1)
                _service.Setup(s => s.GetCustomerJobsAsync(1)).ReturnsAsync(new List<Job>());
            else
                _service.Setup(s => s.GetCustomerJobsAsync(2)).ThrowsAsync(new KeyNotFoundException("Customer not found."));

            // act
            var result = await _controller.GetCustomerJobsAsync(customerId);

            // assert
            if (customerId == 1 && hasJobs)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var jobsResult = Assert.IsAssignableFrom<ICollection<OutJobDTO>>(okResult.Value);
                Assert.Equal(3, jobsResult.Count);  // returns 3 jobs
            }
            else if (customerId == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var jobsResult = Assert.IsAssignableFrom<ICollection<OutJobDTO>>(okResult.Value);
                Assert.Empty(jobsResult);   // returns empty list
            }
            else
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);    // returns 404 not found
                Assert.Equal("Customer not found.", notFoundResult.Value);
            }
        }

        // POST: api/Jobs/create — a warranty parent not found surfaces as 400 with the service message
        [Fact]
        public async Task TestCreateJob_WarrantyParentNotFound_ReturnsBadRequest()
        {
            var dto = _fixture.Build<InJobDTO>()
                                .With(j => j.CustomerId, 1)
                                .With(j => j.DeviceId, 1)
                                .With(j => j.ProblemIds, new List<int> { 1 })
                                .With(j => j.WarrantyOfJobId, (int?)999)
                                .Create();
            _service.Setup(s => s.CreateJobAsync(dto))
                    .ThrowsAsync(new KeyNotFoundException("Job 999 not found when linking for warranty."));

            var result = await _controller.CreateJobAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);   // 400 with message
            Assert.Contains("warranty", badRequest.Value!.ToString());
        }

        // PUT: api/Jobs/update/{id} — a warranty parent not found surfaces as 404 with the service message
        [Fact]
        public async Task TestUpdateJob_WarrantyParentNotFound_ReturnsNotFound()
        {
            var dto = _fixture.Build<UpdateJobDTO>()
                                .Without(j => j.ProblemIds)
                                .With(j => j.WarrantyOfJobId, (int?)999)
                                .Create();
            _service.Setup(s => s.UpdateJobAsync(It.IsAny<UpdateJobDTO>()))
                    .ThrowsAsync(new KeyNotFoundException("Job 999 not found when linking for warranty."));

            var result = await _controller.UpdateJobAsync(1, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);  // 404 with message
            Assert.Contains("warranty", notFound.Value!.ToString());
        }

        // PUT: api/Jobs/update/{id} — a self-link rejected by the service surfaces as 400
        [Fact]
        public async Task TestUpdateJob_SelfLink_ReturnsBadRequest()
        {
            var dto = _fixture.Build<UpdateJobDTO>()
                                .Without(j => j.ProblemIds)
                                .With(j => j.WarrantyOfJobId, (int?)1)
                                .Create();
            _service.Setup(s => s.UpdateJobAsync(It.IsAny<UpdateJobDTO>()))
                    .ThrowsAsync(new ValidationException("A job cannot link itself as warranty."));

            var result = await _controller.UpdateJobAsync(1, dto);

            Assert.IsType<BadRequestResult>(result);    // 400 (ValidationException -> BadRequest, no body)
        }
    }
}
