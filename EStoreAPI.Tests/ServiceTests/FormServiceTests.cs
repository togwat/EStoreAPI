using AutoFixture;
using AutoFixture.AutoMoq;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.Services;
using EStoreAPI.Tests.APITests;
using Moq;

namespace EStoreAPI.Tests.ServiceTests
{
    public class FormServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<ICustomerService> _customerService;
        private readonly Mock<IDeviceService> _deviceService;
        private readonly Mock<IJobService> _jobService;
        private readonly Mock<IProblemService> _problemService;
        private readonly FormService _formService;

        public FormServiceTests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new NoCircularReferencesCustomization())
                .Customize(new IgnoreVirtualMembersCustomization());

            _customerService = _fixture.Freeze<Mock<ICustomerService>>();
            _deviceService   = _fixture.Freeze<Mock<IDeviceService>>();
            _jobService      = _fixture.Freeze<Mock<IJobService>>();
            _problemService  = _fixture.Freeze<Mock<IProblemService>>();

            _formService = new FormService(_jobService.Object, _customerService.Object, _deviceService.Object, _problemService.Object);
        }

        // submitting a form with a primary contact that already belongs to a customer reuses that customer
        [Fact]
        public async Task SubmitForm_ExistingPrimaryContact_DoesNotCreateDuplicate()
        {
            // arrange
            var existingCustomer = _fixture.Create<Customer>();
            var device = _fixture.Create<Device>();
            var problem = _fixture.Create<Problem>();
            var job = _fixture.Create<Job>();

            var dto = _fixture.Build<InFormDTO>()
                .With(f => f.PrimaryContact, existingCustomer.PrimaryContact)
                .With(f => f.DeviceName, device.DeviceName)
                .With(f => f.Problems, [problem.ProblemName])
                .Create();

            _deviceService.Setup(s => s.SearchDevicesByNameAsync(device.DeviceName))
                .ReturnsAsync([device]);
            _problemService.Setup(s => s.GetDeviceProblemsAsync(device.DeviceId))
                .ReturnsAsync([problem]);
            _customerService.Setup(s => s.GetCustomerByPrimaryContactAsync(existingCustomer.PrimaryContact))
                .ReturnsAsync(existingCustomer);
            _jobService.Setup(s => s.CreateJobAsync(It.IsAny<InJobDTO>()))
                .ReturnsAsync(job);

            // act
            await _formService.SubmitFormAsync(dto);

            // assert: existing customer was used, no new customer created
            _customerService.Verify(s => s.CreateCustomerAsync(It.IsAny<InCustomerDTO>()), Times.Never);
            _jobService.Verify(s => s.CreateJobAsync(It.Is<InJobDTO>(j => j.CustomerId == existingCustomer.CustomerId)), Times.Once);
        }

        // submitting a form with an unknown primary contact creates a new customer
        [Fact]
        public async Task SubmitForm_NewPrimaryContact_CreatesCustomer()
        {
            // arrange
            var newCustomer = _fixture.Create<Customer>();
            var device = _fixture.Create<Device>();
            var problem = _fixture.Create<Problem>();
            var job = _fixture.Create<Job>();

            var dto = _fixture.Build<InFormDTO>()
                .With(f => f.DeviceName, device.DeviceName)
                .With(f => f.Problems, [problem.ProblemName])
                .Create();

            _deviceService.Setup(s => s.SearchDevicesByNameAsync(device.DeviceName))
                .ReturnsAsync([device]);
            _problemService.Setup(s => s.GetDeviceProblemsAsync(device.DeviceId))
                .ReturnsAsync([problem]);
            _customerService.Setup(s => s.GetCustomerByPrimaryContactAsync(dto.PrimaryContact))
                .ReturnsAsync((Customer?)null);
            _customerService.Setup(s => s.CreateCustomerAsync(It.IsAny<InCustomerDTO>()))
                .ReturnsAsync(newCustomer);
            _jobService.Setup(s => s.CreateJobAsync(It.IsAny<InJobDTO>()))
                .ReturnsAsync(job);

            // act
            await _formService.SubmitFormAsync(dto);

            // assert: no existing customer found, so a new one was created
            // with each form field mapped to the matching customer field
            _customerService.Verify(s => s.CreateCustomerAsync(It.Is<InCustomerDTO>(c =>
                c.CustomerName == dto.Name &&
                c.PrimaryContact == dto.PrimaryContact &&
                c.PhoneNumber == dto.PhoneNumber &&
                c.Email == dto.Email &&
                c.Address == dto.Address)), Times.Once);
        }

        // a successful submission returns the created job mapped to its output DTO
        [Fact]
        public async Task SubmitForm_ReturnsCreatedJobAsDto()
        {
            var customer = _fixture.Create<Customer>();
            var device = _fixture.Create<Device>();
            var problem = _fixture.Create<Problem>();
            var job = _fixture.Create<Job>();

            var dto = _fixture.Build<InFormDTO>()
                .With(f => f.PrimaryContact, customer.PrimaryContact)
                .With(f => f.DeviceName, device.DeviceName)
                .With(f => f.Problems, [problem.ProblemName])
                .Create();

            _deviceService.Setup(s => s.SearchDevicesByNameAsync(device.DeviceName))
                .ReturnsAsync([device]);
            _problemService.Setup(s => s.GetDeviceProblemsAsync(device.DeviceId))
                .ReturnsAsync([problem]);
            _customerService.Setup(s => s.GetCustomerByPrimaryContactAsync(customer.PrimaryContact))
                .ReturnsAsync(customer);
            _jobService.Setup(s => s.CreateJobAsync(It.IsAny<InJobDTO>()))
                .ReturnsAsync(job);

            var result = await _formService.SubmitFormAsync(dto);

            Assert.Equal(job.JobId, result.JobId);
        }

        // every problem named on the form is resolved and attached to the created job
        [Fact]
        public async Task SubmitForm_MultipleProblems_AllAttachedToJob()
        {
            var customer = _fixture.Create<Customer>();
            var device = _fixture.Create<Device>();
            var problem1 = _fixture.Create<Problem>();
            var problem2 = _fixture.Create<Problem>();
            var job = _fixture.Create<Job>();

            var dto = _fixture.Build<InFormDTO>()
                .With(f => f.PrimaryContact, customer.PrimaryContact)
                .With(f => f.DeviceName, device.DeviceName)
                .With(f => f.Problems, [problem1.ProblemName, problem2.ProblemName])
                .Create();

            _deviceService.Setup(s => s.SearchDevicesByNameAsync(device.DeviceName))
                .ReturnsAsync([device]);
            _problemService.Setup(s => s.GetDeviceProblemsAsync(device.DeviceId))
                .ReturnsAsync([problem1, problem2]);
            _customerService.Setup(s => s.GetCustomerByPrimaryContactAsync(customer.PrimaryContact))
                .ReturnsAsync(customer);
            _jobService.Setup(s => s.CreateJobAsync(It.IsAny<InJobDTO>()))
                .ReturnsAsync(job);

            await _formService.SubmitFormAsync(dto);

            // both problems are resolved to their ids and passed to the job creation
            _jobService.Verify(s => s.CreateJobAsync(It.Is<InJobDTO>(j =>
                j.ProblemIds.Count == 2 &&
                j.ProblemIds.Contains(problem1.ProblemId) &&
                j.ProblemIds.Contains(problem2.ProblemId))), Times.Once);
        }

        // the device name must match exactly; a near-miss without an exact match is rejected
        [Fact]
        public async Task SubmitForm_DeviceNameNoExactMatch_ThrowsKeyNotFound()
        {
            var dto = _fixture.Build<InFormDTO>()
                .With(f => f.DeviceName, "iPhone 15")
                .Create();

            // the search returns a similarly-named device, but none matches exactly
            var nearMiss = _fixture.Build<Device>().With(d => d.DeviceName, "iPhone 15 Pro").Create();
            _deviceService.Setup(s => s.SearchDevicesByNameAsync("iPhone 15"))
                .ReturnsAsync([nearMiss]);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _formService.SubmitFormAsync(dto));
            _jobService.Verify(s => s.CreateJobAsync(It.IsAny<InJobDTO>()), Times.Never);
        }

        // a problem named on the form that is not in the device's catalogue is rejected
        [Fact]
        public async Task SubmitForm_ProblemNotFound_ThrowsKeyNotFound()
        {
            var customer = _fixture.Create<Customer>();
            var device = _fixture.Create<Device>();
            var catalogueProblem = _fixture.Create<Problem>();

            var dto = _fixture.Build<InFormDTO>()
                .With(f => f.PrimaryContact, customer.PrimaryContact)
                .With(f => f.DeviceName, device.DeviceName)
                .With(f => f.Problems, ["not in catalogue"])
                .Create();

            _deviceService.Setup(s => s.SearchDevicesByNameAsync(device.DeviceName))
                .ReturnsAsync([device]);
            // the device's catalogue does not contain the named problem
            _problemService.Setup(s => s.GetDeviceProblemsAsync(device.DeviceId))
                .ReturnsAsync([catalogueProblem]);
            _customerService.Setup(s => s.GetCustomerByPrimaryContactAsync(customer.PrimaryContact))
                .ReturnsAsync(customer);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _formService.SubmitFormAsync(dto));
            _jobService.Verify(s => s.CreateJobAsync(It.IsAny<InJobDTO>()), Times.Never);
        }
    }
}
