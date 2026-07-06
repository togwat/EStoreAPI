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
    public class CustomerServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IEStoreRepo> _repo;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new NoCircularReferencesCustomization())
                .Customize(new IgnoreVirtualMembersCustomization());

            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _customerService = new CustomerService(_repo.Object);
        }

        // returns every customer the repo holds
        [Fact]
        public async Task GetAllCustomers_ReturnsAllFromRepo()
        {
            var customers = _fixture.CreateMany<Customer>(3).ToList();
            _repo.Setup(r => r.GetCustomersAsync()).ReturnsAsync(customers);

            var result = await _customerService.GetAllCustomersAsync();

            Assert.Equal(3, result.Count);
        }

        // an existing id resolves to that customer
        [Fact]
        public async Task GetCustomer_Exists_ReturnsCustomer()
        {
            var customer = _fixture.Create<Customer>();
            _repo.Setup(r => r.GetCustomerByIdAsync(customer.CustomerId)).ReturnsAsync(customer);

            var result = await _customerService.GetCustomerAsync(customer.CustomerId);

            Assert.Equal(customer.CustomerId, result?.CustomerId);
        }

        // an unknown id returns null rather than throwing
        [Fact]
        public async Task GetCustomer_NotFound_ReturnsNull()
        {
            _repo.Setup(r => r.GetCustomerByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);

            var result = await _customerService.GetCustomerAsync(404);

            Assert.Null(result);
        }

        // an existing primary contact resolves to that customer (used by the form flow to avoid duplicates)
        [Fact]
        public async Task GetCustomerByPrimaryContact_Exists_ReturnsCustomer()
        {
            var customer = _fixture.Create<Customer>();
            _repo.Setup(r => r.GetCustomerByPrimaryContactAsync(customer.PrimaryContact)).ReturnsAsync(customer);

            var result = await _customerService.GetCustomerByPrimaryContactAsync(customer.PrimaryContact);

            Assert.Equal(customer.PrimaryContact, result?.PrimaryContact);
        }

        // an unknown primary contact returns null
        [Fact]
        public async Task GetCustomerByPrimaryContact_NotFound_ReturnsNull()
        {
            _repo.Setup(r => r.GetCustomerByPrimaryContactAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);

            var result = await _customerService.GetCustomerByPrimaryContactAsync("0000");

            Assert.Null(result);
        }

        // a null query returns all customers without filtering
        [Fact]
        public async Task SearchCustomers_NullQuery_ReturnsAll()
        {
            var customers = _fixture.CreateMany<Customer>(3).ToList();
            _repo.Setup(r => r.GetCustomersAsync()).ReturnsAsync(customers);

            var result = await _customerService.SearchCustomersAsync(null);

            Assert.Equal(3, result.Count);
            _repo.Verify(r => r.GetCustomersAsync(), Times.Once);
        }

        // a non-null query is forwarded to the repo's query search
        [Fact]
        public async Task SearchCustomers_Query_ReturnsMatches()
        {
            var matches = _fixture.CreateMany<Customer>(2).ToList();
            _repo.Setup(r => r.GetCustomersByQueryAsync("alice")).ReturnsAsync(matches);

            var result = await _customerService.SearchCustomersAsync("alice");

            Assert.Equal(2, result.Count);
            _repo.Verify(r => r.GetCustomersByQueryAsync("alice"), Times.Once);
        }

        // creating a customer persists it and returns the stored entity
        [Fact]
        public async Task CreateCustomer_PersistsAndReturns()
        {
            var dto = _fixture.Create<InCustomerDTO>();
            var created = _fixture.Create<Customer>();
            _repo.Setup(r => r.AddCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(created);

            var result = await _customerService.CreateCustomerAsync(dto);

            Assert.Equal(created.CustomerId, result.CustomerId);
            _repo.Verify(r => r.AddCustomerAsync(It.IsAny<Customer>()), Times.Once);
        }

        // the primary contact is stored verbatim (it may be an email, telegram id, etc.),
        // while the optional phone number is still normalised to digits only
        [Fact]
        public async Task CreateCustomer_PrimaryContactVerbatim_PhoneNormalised()
        {
            var dto = new InCustomerDTO
            {
                CustomerName = "Alice",
                PrimaryContact = "alice@example.com",
                PhoneNumber = "021 123-4567",
            };
            _repo.Setup(r => r.AddCustomerAsync(It.IsAny<Customer>()))
                .ReturnsAsync((Customer c) => c);

            await _customerService.CreateCustomerAsync(dto);

            _repo.Verify(r => r.AddCustomerAsync(It.Is<Customer>(c =>
                c.PrimaryContact == "alice@example.com" &&
                c.PhoneNumber == "0211234567")), Times.Once);
        }

        // the numbers-only rule no longer applies to the primary contact,
        // but still applies to the optional phone number
        [Fact]
        public async Task CreateCustomer_NonNumericPhone_ThrowsValidation()
        {
            var dto = new InCustomerDTO
            {
                CustomerName = "Alice",
                PrimaryContact = "alice@example.com",
                PhoneNumber = "not-a-phone",
            };

            await Assert.ThrowsAsync<ValidationException>(() => _customerService.CreateCustomerAsync(dto));
            _repo.Verify(r => r.AddCustomerAsync(It.IsAny<Customer>()), Times.Never);
        }

        // bulk create persists every customer
        [Fact]
        public async Task CreateCustomers_Bulk_PersistsAll()
        {
            var dtos = _fixture.CreateMany<InCustomerDTO>(3).ToList();
            var created = _fixture.CreateMany<Customer>(3).ToList();
            _repo.Setup(r => r.AddCustomersAsync(It.IsAny<ICollection<Customer>>())).ReturnsAsync(created);

            var result = await _customerService.CreateCustomersAsync(dtos);

            Assert.Equal(3, result.Count);
            _repo.Verify(r => r.AddCustomersAsync(It.IsAny<ICollection<Customer>>()), Times.Once);
        }

        // a partially-filled update DTO overwrites only the provided fields, leaving the rest untouched
        [Fact]
        public async Task UpdateCustomer_PartialDto_OnlyOverwritesProvidedFields()
        {
            var original = _fixture.Create<Customer>();
            var originalName = original.CustomerName;
            var originalPrimaryContact = original.PrimaryContact;
            var originalPhone = original.PhoneNumber;
            var originalAddress = original.Address;
            _repo.Setup(r => r.GetCustomerByIdAsync(original.CustomerId)).ReturnsAsync(original);

            // only the email is supplied; everything else is left null
            var dto = new UpdateCustomerDTO { CustomerId = original.CustomerId, Email = "new@example.com" };

            await _customerService.UpdateCustomerAsync(dto);

            Assert.Equal("new@example.com", original.Email);
            Assert.Equal(originalName, original.CustomerName);
            Assert.Equal(originalPrimaryContact, original.PrimaryContact);
            Assert.Equal(originalPhone, original.PhoneNumber);
            Assert.Equal(originalAddress, original.Address);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Once);
        }

        // the update merge writes each DTO field into the matching entity field:
        // primary contact stays verbatim, phone number is normalised
        [Fact]
        public async Task UpdateCustomer_ContactFields_MapToCorrectColumns()
        {
            var original = _fixture.Create<Customer>();
            _repo.Setup(r => r.GetCustomerByIdAsync(original.CustomerId)).ReturnsAsync(original);

            var dto = new UpdateCustomerDTO
            {
                CustomerId = original.CustomerId,
                PrimaryContact = "tg:@alice",
                PhoneNumber = "09 555-1234",
            };

            await _customerService.UpdateCustomerAsync(dto);

            Assert.Equal("tg:@alice", original.PrimaryContact);
            Assert.Equal("095551234", original.PhoneNumber);
        }

        // bulk update applies the same partial-overwrite rule to each customer independently
        [Fact]
        public async Task UpdateCustomers_Bulk_PartialDtos_OnlyOverwriteProvidedFields()
        {
            var first = _fixture.Create<Customer>();
            var second = _fixture.Create<Customer>();
            var firstOriginalEmail = first.Email;
            var secondOriginalName = second.CustomerName;
            _repo.Setup(r => r.GetCustomerByIdAsync(first.CustomerId)).ReturnsAsync(first);
            _repo.Setup(r => r.GetCustomerByIdAsync(second.CustomerId)).ReturnsAsync(second);

            var dtos = new List<UpdateCustomerDTO>
            {
                new() { CustomerId = first.CustomerId, CustomerName = "Renamed" },
                new() { CustomerId = second.CustomerId, Email = "second@example.com" },
            };

            await _customerService.UpdateCustomersAsync(dtos);

            // first: only the name changed
            Assert.Equal("Renamed", first.CustomerName);
            Assert.Equal(firstOriginalEmail, first.Email);
            // second: only the email changed
            Assert.Equal("second@example.com", second.Email);
            Assert.Equal(secondOriginalName, second.CustomerName);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.AtLeastOnce);
        }
    }
}
