using AutoFixture;
using AutoFixture.AutoMoq;
using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.Services;
using EStoreAPI.Tests.APITests;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Tests.ServiceTests
{
    public class JobServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IEStoreRepo> _repo;
        private readonly JobService _jobService;

        public JobServiceTests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new NoCircularReferencesCustomization())
                .Customize(new IgnoreVirtualMembersCustomization());

            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _jobService = new JobService(_repo.Object);
        }

        // null query returns all jobs without filtering
        [Fact]
        public async Task SearchJobs_NullQuery_ReturnsAllJobs()
        {
            var allJobs = _fixture.CreateMany<Job>(3).ToList();
            _repo.Setup(r => r.GetJobsAsync()).ReturnsAsync(allJobs);

            var result = await _jobService.SearchJobsAsync(null);

            Assert.Equal(3, result.Count);
            _repo.Verify(r => r.GetJobsAsync(), Times.Once);
        }

        // query matching customers returns jobs belonging to those customers
        [Fact]
        public async Task SearchJobs_MatchingCustomers_ReturnsCustomerJobs()
        {
            var customer = _fixture.Create<Customer>();
            var customerJobs = _fixture.CreateMany<Job>(2).ToList();

            _repo.Setup(r => r.GetCustomersByQueryAsync("alice")).ReturnsAsync([customer]);
            _repo.Setup(r => r.GetDevicesByNameAsync("alice")).ReturnsAsync([]);
            _repo.Setup(r => r.GetJobsOfCustomerAsync(customer.CustomerId)).ReturnsAsync(customerJobs);

            var result = await _jobService.SearchJobsAsync("alice");

            Assert.Equal(2, result.Count);
        }

        // query matching devices returns jobs belonging to those devices
        [Fact]
        public async Task SearchJobs_MatchingDevices_ReturnsDeviceJobs()
        {
            var device = _fixture.Create<Device>();
            var deviceJobs = _fixture.CreateMany<Job>(2).ToList();

            _repo.Setup(r => r.GetCustomersByQueryAsync("iphone")).ReturnsAsync([]);
            _repo.Setup(r => r.GetDevicesByNameAsync("iphone")).ReturnsAsync([device]);
            _repo.Setup(r => r.GetJobsOfDeviceAsync(device.DeviceId)).ReturnsAsync(deviceJobs);

            var result = await _jobService.SearchJobsAsync("iphone");

            Assert.Equal(2, result.Count);
        }

        // a job that matches on both customer and device should appear only once
        [Fact]
        public async Task SearchJobs_JobMatchesBothCustomerAndDevice_Deduplicates()
        {
            var customer = _fixture.Create<Customer>();
            var device = _fixture.Create<Device>();
            var sharedJob = _fixture.Create<Job>();

            _repo.Setup(r => r.GetCustomersByQueryAsync("query")).ReturnsAsync([customer]);
            _repo.Setup(r => r.GetDevicesByNameAsync("query")).ReturnsAsync([device]);
            _repo.Setup(r => r.GetJobsOfCustomerAsync(customer.CustomerId)).ReturnsAsync([sharedJob]);
            _repo.Setup(r => r.GetJobsOfDeviceAsync(device.DeviceId)).ReturnsAsync([sharedJob]);

            var result = await _jobService.SearchJobsAsync("query");

            Assert.Single(result);
        }

        // partial query matching multiple customers returns jobs from all of them combined
        [Fact]
        public async Task SearchJobs_MultipleMatchingCustomers_ReturnsAllCustomerJobs()
        {
            var customers = _fixture.CreateMany<Customer>(3).ToList();
            var jobsPerCustomer = customers.Select(c => _fixture.CreateMany<Job>(2).ToList()).ToList();

            _repo.Setup(r => r.GetCustomersByQueryAsync("ali")).ReturnsAsync(customers);
            _repo.Setup(r => r.GetDevicesByNameAsync("ali")).ReturnsAsync([]);
            for (int i = 0; i < customers.Count; i++)
                _repo.Setup(r => r.GetJobsOfCustomerAsync(customers[i].CustomerId)).ReturnsAsync(jobsPerCustomer[i]);

            var result = await _jobService.SearchJobsAsync("ali");

            Assert.Equal(6, result.Count);
        }

        // partial query matching multiple devices returns jobs from all of them combined
        [Fact]
        public async Task SearchJobs_MultipleMatchingDevices_ReturnsAllDeviceJobs()
        {
            var devices = _fixture.CreateMany<Device>(3).ToList();
            var jobsPerDevice = devices.Select(d => _fixture.CreateMany<Job>(2).ToList()).ToList();

            _repo.Setup(r => r.GetCustomersByQueryAsync("iph")).ReturnsAsync([]);
            _repo.Setup(r => r.GetDevicesByNameAsync("iph")).ReturnsAsync(devices);
            for (int i = 0; i < devices.Count; i++)
                _repo.Setup(r => r.GetJobsOfDeviceAsync(devices[i].DeviceId)).ReturnsAsync(jobsPerDevice[i]);

            var result = await _jobService.SearchJobsAsync("iph");

            Assert.Equal(6, result.Count);
        }

        // partial query matching multiple customers and devices deduplicates shared jobs
        [Fact]
        public async Task SearchJobs_MultipleMatchesBothSides_DeduplicatesSharedJobs()
        {
            var customers = _fixture.CreateMany<Customer>(2).ToList();
            var devices = _fixture.CreateMany<Device>(2).ToList();
            var sharedJob = _fixture.Create<Job>();
            var uniqueCustomerJob = _fixture.Create<Job>();
            var uniqueDeviceJob = _fixture.Create<Job>();

            _repo.Setup(r => r.GetCustomersByQueryAsync("sam")).ReturnsAsync(customers);
            _repo.Setup(r => r.GetDevicesByNameAsync("sam")).ReturnsAsync(devices);
            _repo.Setup(r => r.GetJobsOfCustomerAsync(customers[0].CustomerId)).ReturnsAsync([sharedJob, uniqueCustomerJob]);
            _repo.Setup(r => r.GetJobsOfCustomerAsync(customers[1].CustomerId)).ReturnsAsync([sharedJob]);
            _repo.Setup(r => r.GetJobsOfDeviceAsync(devices[0].DeviceId)).ReturnsAsync([sharedJob, uniqueDeviceJob]);
            _repo.Setup(r => r.GetJobsOfDeviceAsync(devices[1].DeviceId)).ReturnsAsync([sharedJob]);

            var result = await _jobService.SearchJobsAsync("sam");

            // sharedJob appears 4 times across sources but should be counted once
            Assert.Equal(3, result.Count);
        }

        // query with no matching customers or devices returns empty list
        [Fact]
        public async Task SearchJobs_NoMatches_ReturnsEmpty()
        {
            _repo.Setup(r => r.GetCustomersByQueryAsync("xyz")).ReturnsAsync([]);
            _repo.Setup(r => r.GetDevicesByNameAsync("xyz")).ReturnsAsync([]);

            var result = await _jobService.SearchJobsAsync("xyz");

            Assert.Empty(result);
        }

        // returns every job the repo holds
        [Fact]
        public async Task GetAllJobs_ReturnsAllFromRepo()
        {
            var jobs = _fixture.CreateMany<Job>(3).ToList();
            _repo.Setup(r => r.GetJobsAsync()).ReturnsAsync(jobs);

            var result = await _jobService.GetAllJobsAsync();

            Assert.Equal(3, result.Count);
        }

        // an existing id resolves to that job
        [Fact]
        public async Task GetJob_Exists_ReturnsJob()
        {
            var job = _fixture.Create<Job>();
            _repo.Setup(r => r.GetJobByIdAsync(job.JobId)).ReturnsAsync(job);

            var result = await _jobService.GetJobAsync(job.JobId);

            Assert.Equal(job.JobId, result?.JobId);
        }

        // an unknown id returns null rather than throwing
        [Fact]
        public async Task GetJob_NotFound_ReturnsNull()
        {
            _repo.Setup(r => r.GetJobByIdAsync(It.IsAny<int>())).ReturnsAsync((Job?)null);

            var result = await _jobService.GetJobAsync(404);

            Assert.Null(result);
        }

        // a customer's jobs are forwarded from the repo
        [Fact]
        public async Task GetCustomerJobs_ReturnsCustomerJobs()
        {
            var jobs = _fixture.CreateMany<Job>(2).ToList();
            _repo.Setup(r => r.GetJobsOfCustomerAsync(9)).ReturnsAsync(jobs);

            var result = await _jobService.GetCustomerJobsAsync(9);

            Assert.Equal(2, result.Count);
        }

        // creating a job persists it and returns the stored entity
        [Fact]
        public async Task CreateJob_PersistsAndReturns()
        {
            var created = _fixture.Create<Job>();
            // the dto's problem ids are resolved against the repo before the job is built,
            // so the dto requests exactly the ids of the problems the repo will return
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);
            _repo.Setup(r => r.AddJobAsync(It.IsAny<Job>())).ReturnsAsync(created);

            var result = await _jobService.CreateJobAsync(dto);

            Assert.Equal(created.JobId, result.JobId);
            _repo.Verify(r => r.AddJobAsync(It.IsAny<Job>()), Times.Once);
        }

        // bulk create persists every job
        [Fact]
        public async Task CreateJobs_Bulk_PersistsAll()
        {
            var created = _fixture.CreateMany<Job>(3).ToList();
            // every dto requests the same prepared problem set so id-resolution succeeds for each
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var problemIds = problems.Select(p => p.ProblemId).ToList();
            var dtos = Enumerable.Range(0, 3)
                .Select(_ => _fixture.Build<InJobDTO>().With(d => d.ProblemIds, problemIds.ToList()).Create())
                .ToList();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);
            _repo.Setup(r => r.AddJobsAsync(It.IsAny<ICollection<Job>>())).ReturnsAsync(created);

            var result = await _jobService.CreateJobsAsync(dtos);

            Assert.Equal(3, result.Count);
            _repo.Verify(r => r.AddJobsAsync(It.IsAny<ICollection<Job>>()), Times.Once);
        }

        // a partially-filled update DTO overwrites only the provided fields, leaving the rest untouched
        [Fact]
        public async Task UpdateJob_PartialDto_OnlyOverwritesProvidedFields()
        {
            var original = _fixture.Create<Job>();
            var originalPickup = original.PickupTime;
            var originalEstimatedPrice = original.EstimatedPrice;
            var originalIsFinished = original.IsFinished;
            _repo.Setup(r => r.GetJobByIdAsync(original.JobId)).ReturnsAsync(original);

            // only the note is supplied; problem ids left null so the problem set is untouched
            var dto = new UpdateJobDTO { JobId = original.JobId, Note = "updated note" };

            await _jobService.UpdateJobAsync(dto);

            Assert.Equal("updated note", original.Note);
            Assert.Equal(originalPickup, original.PickupTime);
            Assert.Equal(originalEstimatedPrice, original.EstimatedPrice);
            Assert.Equal(originalIsFinished, original.IsFinished);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Once);
        }

        // bulk update applies the same partial-overwrite rule to each job independently
        [Fact]
        public async Task UpdateJobs_Bulk_PartialDtos_OnlyOverwriteProvidedFields()
        {
            var first = _fixture.Create<Job>();
            var second = _fixture.Create<Job>();
            var firstOriginalNote = first.Note;
            var secondOriginalFinished = second.IsFinished;
            _repo.Setup(r => r.GetJobByIdAsync(first.JobId)).ReturnsAsync(first);
            _repo.Setup(r => r.GetJobByIdAsync(second.JobId)).ReturnsAsync(second);

            var dtos = new List<UpdateJobDTO>
            {
                new() { JobId = first.JobId, IsFinished = true },
                new() { JobId = second.JobId, Note = "second note" },
            };

            await _jobService.UpdateJobsAsync(dtos);

            // first: only the finished flag changed
            Assert.True(first.IsFinished);
            Assert.Equal(firstOriginalNote, first.Note);
            // second: only the note changed
            Assert.Equal("second note", second.Note);
            Assert.Equal(secondOriginalFinished, second.IsFinished);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.AtLeastOnce);
        }

        // --- Warranty linking ---

        // creating a warranty job validates the parent exists and carries the link onto the saved job
        [Fact]
        public async Task CreateJob_WithExistingWarrantyParent_PersistsLink()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var parent = _fixture.Build<Job>().With(j => j.JobId, 50).Create();
            var created = _fixture.Create<Job>();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.WarrantyOfJobId, (int?)50)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);
            _repo.Setup(r => r.GetJobByIdAsync(50)).ReturnsAsync(parent);

            // capture the job handed to the repo so we can assert the link survived ToModel
            Job? saved = null;
            _repo.Setup(r => r.AddJobAsync(It.IsAny<Job>()))
                .Callback<Job>(j => saved = j)
                .ReturnsAsync(created);

            await _jobService.CreateJobAsync(dto);

            Assert.Equal(50, saved!.WarrantyOfJobId);
            _repo.Verify(r => r.GetJobByIdAsync(50), Times.Once);
        }

        // a warranty link to a non-existent parent is rejected and nothing is persisted
        [Fact]
        public async Task CreateJob_WarrantyParentNotFound_ThrowsAndPersistsNothing()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.WarrantyOfJobId, (int?)999)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);
            _repo.Setup(r => r.GetJobByIdAsync(999)).ReturnsAsync((Job?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _jobService.CreateJobAsync(dto));
            _repo.Verify(r => r.AddJobAsync(It.IsAny<Job>()), Times.Never);
        }

        // omitting the warranty id creates an ordinary job with no link and no parent lookup
        [Fact]
        public async Task CreateJob_NoWarrantyId_DoesNotLinkOrValidate()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var created = _fixture.Create<Job>();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.WarrantyOfJobId, (int?)null)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);

            Job? saved = null;
            _repo.Setup(r => r.AddJobAsync(It.IsAny<Job>()))
                .Callback<Job>(j => saved = j)
                .ReturnsAsync(created);

            await _jobService.CreateJobAsync(dto);

            Assert.Null(saved!.WarrantyOfJobId);
            // no warranty id means the parent existence check is skipped entirely
            _repo.Verify(r => r.GetJobByIdAsync(It.IsAny<int>()), Times.Never);
        }

        // in a bulk create, one bad warranty link rejects the whole batch before anything is saved
        [Fact]
        public async Task CreateJobs_OneWarrantyParentNotFound_ThrowsAndPersistsNothing()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var problemIds = problems.Select(p => p.ProblemId).ToList();
            var goodDto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problemIds.ToList())
                .With(d => d.WarrantyOfJobId, (int?)null)
                .Create();
            var badDto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problemIds.ToList())
                .With(d => d.WarrantyOfJobId, (int?)999)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);
            _repo.Setup(r => r.GetJobByIdAsync(999)).ReturnsAsync((Job?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _jobService.CreateJobsAsync(new List<InJobDTO> { goodDto, badDto }));
            _repo.Verify(r => r.AddJobsAsync(It.IsAny<ICollection<Job>>()), Times.Never);
        }

        // linking an existing job to a valid parent (without touching problems) sets the link
        [Fact]
        public async Task UpdateJob_LinkWarrantyToExistingParent_SetsLink()
        {
            var existing = _fixture.Build<Job>().With(j => j.JobId, 5).With(j => j.WarrantyOfJobId, (int?)null).Create();
            var parent = _fixture.Build<Job>().With(j => j.JobId, 50).Create();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);
            _repo.Setup(r => r.GetJobByIdAsync(50)).ReturnsAsync(parent);

            var dto = new UpdateJobDTO { JobId = 5, WarrantyOfJobId = 50 };

            await _jobService.UpdateJobAsync(dto);

            Assert.Equal(50, existing.WarrantyOfJobId);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Once);
        }

        // linking to a non-existent parent must be rejected even when no problems are being changed
        [Fact]
        public async Task UpdateJob_WarrantyParentNotFound_Throws()
        {
            var existing = _fixture.Build<Job>().With(j => j.JobId, 5).Create();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);
            _repo.Setup(r => r.GetJobByIdAsync(999)).ReturnsAsync((Job?)null);

            // no ProblemIds supplied: the warranty check must still run
            var dto = new UpdateJobDTO { JobId = 5, WarrantyOfJobId = 999 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _jobService.UpdateJobAsync(dto));
        }

        // a job cannot be linked as a warranty of itself, even when no problems are being changed
        [Fact]
        public async Task UpdateJob_SelfLink_Throws()
        {
            var existing = _fixture.Build<Job>().With(j => j.JobId, 5).Create();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            var dto = new UpdateJobDTO { JobId = 5, WarrantyOfJobId = 5 };

            await Assert.ThrowsAsync<ValidationException>(() => _jobService.UpdateJobAsync(dto));
        }

        // omitting the warranty id on update leaves an existing link untouched and looks up no parent
        [Fact]
        public async Task UpdateJob_OmitWarrantyId_LeavesExistingLinkUnchanged()
        {
            var existing = _fixture.Build<Job>().With(j => j.JobId, 5).With(j => j.WarrantyOfJobId, (int?)7).Create();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            // dto with no warranty id supplied, only an unrelated change
            var dto = new UpdateJobDTO { JobId = 5, Note = "unrelated change" };

            await _jobService.UpdateJobAsync(dto);

            Assert.Equal(7, existing.WarrantyOfJobId);
            // nothing should look up a warranty parent when none was supplied
            _repo.Verify(r => r.GetJobByIdAsync(7), Times.Never);
        }
    }
}
