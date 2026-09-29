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

        // a receive time given on the dto is kept on the saved job
        [Fact]
        public async Task CreateJob_ReceiveTimeGiven_UsesGivenTime()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var receiveTime = new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.ReceiveTime, receiveTime)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);

            Job? saved = null;
            _repo.Setup(r => r.AddJobAsync(It.IsAny<Job>()))
                .Callback<Job>(j => saved = j)
                .ReturnsAsync(_fixture.Create<Job>());

            await _jobService.CreateJobAsync(dto);

            Assert.Equal(receiveTime, saved!.ReceiveTime);
        }

        // an omitted receive time defaults to the time of creation
        [Fact]
        public async Task CreateJob_ReceiveTimeOmitted_DefaultsToNow()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.ReceiveTime, (DateTime?)null)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);

            Job? saved = null;
            _repo.Setup(r => r.AddJobAsync(It.IsAny<Job>()))
                .Callback<Job>(j => saved = j)
                .ReturnsAsync(_fixture.Create<Job>());

            await _jobService.CreateJobAsync(dto);

            Assert.True((DateTime.UtcNow - saved!.ReceiveTime).TotalMinutes < 1);
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
            var originalReceive = original.ReceiveTime;
            var originalPickup = original.PickupTime;
            var originalEstimatedPrice = original.EstimatedPrice;
            var originalStatus = original.Status;
            _repo.Setup(r => r.GetJobByIdAsync(original.JobId)).ReturnsAsync(original);

            // only the note is supplied; problem ids left null so the problem set is untouched
            var dto = new UpdateJobDTO { JobId = original.JobId, Note = "updated note" };

            await _jobService.UpdateJobAsync(dto);

            Assert.Equal("updated note", original.Note);
            Assert.Equal(originalReceive, original.ReceiveTime);
            Assert.Equal(originalPickup, original.PickupTime);
            Assert.Equal(originalEstimatedPrice, original.EstimatedPrice);
            Assert.Equal(originalStatus, original.Status);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Once);
        }

        // a supplied receive time replaces the job's existing one
        [Fact]
        public async Task UpdateJob_ReceiveTimeGiven_OverwritesReceiveTime()
        {
            var original = _fixture.Create<Job>();
            var receiveTime = new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);
            _repo.Setup(r => r.GetJobByIdAsync(original.JobId)).ReturnsAsync(original);

            var dto = new UpdateJobDTO { JobId = original.JobId, ReceiveTime = receiveTime };

            await _jobService.UpdateJobAsync(dto);

            Assert.Equal(receiveTime, original.ReceiveTime);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Once);
        }

        // bulk update applies the same partial-overwrite rule to each job independently
        [Fact]
        public async Task UpdateJobs_Bulk_PartialDtos_OnlyOverwriteProvidedFields()
        {
            var first = _fixture.Create<Job>();
            var second = _fixture.Create<Job>();
            var firstOriginalNote = first.Note;
            var secondOriginalStatus = second.Status;
            _repo.Setup(r => r.GetJobByIdAsync(first.JobId)).ReturnsAsync(first);
            _repo.Setup(r => r.GetJobByIdAsync(second.JobId)).ReturnsAsync(second);

            var dtos = new List<UpdateJobDTO>
            {
                new() { JobId = first.JobId, Status = JobStatus.Finished },
                new() { JobId = second.JobId, Note = "second note" },
            };

            await _jobService.UpdateJobsAsync(dtos);

            // first: only the status changed
            Assert.Equal(JobStatus.Finished, first.Status);
            Assert.Equal(firstOriginalNote, first.Note);
            // second: only the note changed
            Assert.Equal("second note", second.Note);
            Assert.Equal(secondOriginalStatus, second.Status);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.AtLeastOnce);
        }

        // Giving a status value not part of the enum should fail
        [Fact]
        public async Task CreateJob_StatusNotInEnum_ThrowsAndPersistsNothing()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.Status, (JobStatus)99)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);

            await Assert.ThrowsAsync<ValidationException>(() => _jobService.CreateJobAsync(dto));

            _repo.Verify(r => r.AddJobAsync(It.IsAny<Job>()), Times.Never);
        }

        [Fact]
        public async Task UpdateJob_StatusNotInEnum_ThrowsAndPersistsNothing()
        {
            var existing = _fixture.Build<Job>().With(j => j.JobId, 5).Create();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);
            var dto = new UpdateJobDTO { JobId = 5, Status = (JobStatus)99 };

            await Assert.ThrowsAsync<ValidationException>(() => _jobService.UpdateJobAsync(dto));

            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Never);
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

        // Job transaction logging

        // a created job carries exactly one log mirroring the status and note it was created with
        [Fact]
        public async Task CreateJob_AttachesLogMirroringJob()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.Status, JobStatus.Finished)
                .With(d => d.Note, "screen cracked")
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);

            // capture the job handed to the repo so we can inspect the log built onto its graph
            Job? saved = null;
            _repo.Setup(r => r.AddJobAsync(It.IsAny<Job>()))
                .Callback<Job>(j => saved = j)
                .ReturnsAsync(_fixture.Create<Job>());

            await _jobService.CreateJobAsync(dto);

            JobLog log = Assert.Single(saved!.Logs!);
            Assert.Equal(JobStatus.Finished, log.Status);
            Assert.Equal("screen cracked", log.Note);
            // booking in records no money movement
            Assert.Null(log.MoneyChange);
            // stamped at creation time
            Assert.True((DateTime.UtcNow - log.Timestamp).TotalMinutes < 1);
        }

        // a field left null on the job (note) stay null on the log
        [Fact]
        public async Task CreateJob_NullNote_LeavesLogNoteNull()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var dto = _fixture.Build<InJobDTO>()
                .With(d => d.ProblemIds, problems.Select(p => p.ProblemId).ToList())
                .With(d => d.Note, (string?)null)
                .Create();
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);

            Job? saved = null;
            _repo.Setup(r => r.AddJobAsync(It.IsAny<Job>()))
                .Callback<Job>(j => saved = j)
                .ReturnsAsync(_fixture.Create<Job>());

            await _jobService.CreateJobAsync(dto);

            JobLog log = Assert.Single(saved!.Logs!);
            Assert.Null(log.Note);
            Assert.Null(log.MoneyChange);
        }

        // bulk create attaches a log to every job, each mirroring its own job's fields
        [Fact]
        public async Task CreateJobs_Bulk_AttachesLogToEachJob()
        {
            var problems = _fixture.CreateMany<Problem>(2).ToList();
            var problemIds = problems.Select(p => p.ProblemId).ToList();
            var dtos = new List<InJobDTO>
            {
                _fixture.Build<InJobDTO>()
                    .With(d => d.ProblemIds, problemIds.ToList())
                    .With(d => d.Status, JobStatus.InProgress)
                    .With(d => d.Note, "first")
                    .Create(),
                // second job has a null note to prove per-job null preservation survives the batch
                _fixture.Build<InJobDTO>()
                    .With(d => d.ProblemIds, problemIds.ToList())
                    .With(d => d.Status, JobStatus.Finished)
                    .With(d => d.Note, (string?)null)
                    .Create(),
            };
            _repo.Setup(r => r.GetProblemsByIdsAsync(It.IsAny<ICollection<int>>())).ReturnsAsync(problems);

            ICollection<Job>? saved = null;
            _repo.Setup(r => r.AddJobsAsync(It.IsAny<ICollection<Job>>()))
                .Callback<ICollection<Job>>(j => saved = j)
                .ReturnsAsync(_fixture.CreateMany<Job>(2).ToList());

            await _jobService.CreateJobsAsync(dtos);

            var savedList = saved!.ToList();
            // every job gets exactly one log
            Assert.All(savedList, j => Assert.Single(j.Logs!));

            JobLog firstLog = savedList[0].Logs!.Single();
            Assert.Equal(JobStatus.InProgress, firstLog.Status);
            Assert.Equal("first", firstLog.Note);

            JobLog secondLog = savedList[1].Logs!.Single();
            Assert.Equal(JobStatus.Finished, secondLog.Status);
            Assert.Null(secondLog.Note);
        }

        // an update that changes the status records one log carrying the resulting status
        [Fact]
        public async Task UpdateJob_StatusChange_WritesLogWithResultingState()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.Status, JobStatus.InProgress)
                .Create();
            // the loaded job carries its (empty) log collection; the update should append to it
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            var dto = new UpdateJobDTO { JobId = 5, Status = JobStatus.Finished };

            await _jobService.UpdateJobAsync(dto);

            JobLog log = Assert.Single(existing.Logs);
            Assert.Equal(JobStatus.Finished, log.Status);
            Assert.True((DateTime.UtcNow - log.Timestamp).TotalMinutes < 1);
        }

        // taking a payment raises CollectedPrice, and the log records the positive delta
        [Fact]
        public async Task UpdateJob_CollectedPriceIncrease_LogsPositiveMoneyDelta()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.CollectedPrice, 100m)
                .Create();
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            // 100 -> 250 is a 150 payment
            var dto = new UpdateJobDTO { JobId = 5, CollectedPrice = 250m };

            await _jobService.UpdateJobAsync(dto);

            JobLog log = Assert.Single(existing.Logs);
            Assert.Equal(150m, log.MoneyChange);
        }

        // a refund lowers CollectedPrice, and the log records the money going back out as a negative delta
        [Fact]
        public async Task UpdateJob_Refund_LogsNegativeMoneyDelta()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.CollectedPrice, 150m)
                .Create();
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            // 150 -> 0 is a full refund
            var dto = new UpdateJobDTO { JobId = 5, CollectedPrice = 0m };

            await _jobService.UpdateJobAsync(dto);

            JobLog log = Assert.Single(existing.Logs);
            Assert.Equal(-150m, log.MoneyChange);
        }

        // a null note on the job stays null on the update log, same as on create
        [Fact]
        public async Task UpdateJob_NullNote_LeavesLogNoteNull()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.Note, (string?)null)
                .With(j => j.Status, JobStatus.InProgress)
                .Create();
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            // change the status so an update genuinely happens; the note is left untouched (null)
            var dto = new UpdateJobDTO { JobId = 5, Status = JobStatus.Finished };

            await _jobService.UpdateJobAsync(dto);

            JobLog log = Assert.Single(existing.Logs);
            Assert.Null(log.Note);
        }

        // bulk update logs each changed job independently, one log apiece
        [Fact]
        public async Task UpdateJobs_Bulk_EachChangedJobGetsOwnLog()
        {
            var first = _fixture.Build<Job>().With(j => j.JobId, 1).With(j => j.Status, JobStatus.InProgress).Create();
            var second = _fixture.Build<Job>().With(j => j.JobId, 2).With(j => j.Status, JobStatus.InProgress).Create();
            first.Logs = new List<JobLog>();
            second.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(1)).ReturnsAsync(first);
            _repo.Setup(r => r.GetJobByIdAsync(2)).ReturnsAsync(second);

            var dtos = new List<UpdateJobDTO>
            {
                new() { JobId = 1, Status = JobStatus.Finished },
                new() { JobId = 2, Status = JobStatus.Cancelled },
            };

            await _jobService.UpdateJobsAsync(dtos);

            JobLog firstLog = Assert.Single(first.Logs);
            Assert.Equal(JobStatus.Finished, firstLog.Status);
            JobLog secondLog = Assert.Single(second.Logs);
            Assert.Equal(JobStatus.Cancelled, secondLog.Status);
        }

        // Update logging: full-object PATCH
        
        [Fact]
        public async Task UpdateJob_FullDto_OnlyNoteChanged_LogsNoteOnly()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.Status, JobStatus.InProgress)
                .With(j => j.Note, "old note")
                .With(j => j.CollectedPrice, 100m)
                .Create();
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            // whole job resent; status and price unchanged, only the note differs
            var dto = new UpdateJobDTO
            {
                JobId = 5,
                Status = JobStatus.InProgress,
                Note = "new note",
                CollectedPrice = 100m,
            };

            await _jobService.UpdateJobAsync(dto);

            JobLog log = Assert.Single(existing.Logs);
            Assert.Equal("new note", log.Note);
            Assert.Null(log.Status);        // status resent unchanged: must not be logged
            Assert.Null(log.MoneyChange);   // price unchanged: no money entry, not 0
        }

        // resending every field but changing only the status must log the status alone
        [Fact]
        public async Task UpdateJob_FullDto_OnlyStatusChanged_LogsStatusOnly()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.Status, JobStatus.InProgress)
                .With(j => j.Note, "keep me")
                .With(j => j.CollectedPrice, 100m)
                .Create();
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            var dto = new UpdateJobDTO
            {
                JobId = 5,
                Status = JobStatus.Finished,
                Note = "keep me",
                CollectedPrice = 100m,
            };

            await _jobService.UpdateJobAsync(dto);

            JobLog log = Assert.Single(existing.Logs);
            Assert.Equal(JobStatus.Finished, log.Status);
            Assert.Null(log.Note);          // note resent unchanged: must not be logged
            Assert.Null(log.MoneyChange);
        }

        // first payment: CollectedPrice null -> value logs the full amount as a positive delta
        [Fact]
        public async Task UpdateJob_CollectedPriceFromNull_LogsPositiveDelta()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.Status, JobStatus.InProgress)
                .With(j => j.Note, "keep me")
                .With(j => j.CollectedPrice, (decimal?)null)
                .Create();
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            var dto = new UpdateJobDTO
            {
                JobId = 5,
                Status = JobStatus.InProgress,
                Note = "keep me",
                CollectedPrice = 150m,
            };

            await _jobService.UpdateJobAsync(dto);

            JobLog log = Assert.Single(existing.Logs);
            Assert.Equal(150m, log.MoneyChange);    // null baseline treated as 0
            Assert.Null(log.Status);
            Assert.Null(log.Note);
        }

        // a full-object update that changes nothing writes no log entry at all
        [Fact]
        public async Task UpdateJob_FullDto_NoActualChange_WritesNoLog()
        {
            var existing = _fixture.Build<Job>()
                .With(j => j.JobId, 5)
                .With(j => j.Status, JobStatus.InProgress)
                .With(j => j.Note, "unchanged")
                .With(j => j.CollectedPrice, 100m)
                .Create();
            existing.Logs = new List<JobLog>();
            _repo.Setup(r => r.GetJobByIdAsync(5)).ReturnsAsync(existing);

            var dto = new UpdateJobDTO
            {
                JobId = 5,
                Status = JobStatus.InProgress,
                Note = "unchanged",
                CollectedPrice = 100m,
            };

            await _jobService.UpdateJobAsync(dto);

            Assert.Empty(existing.Logs);
        }
    }
}
