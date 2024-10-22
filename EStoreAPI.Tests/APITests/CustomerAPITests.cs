using AutoFixture;
using Moq;
using AutoFixture.AutoMoq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.Data;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace EStoreAPI.Tests.APITests
{
    public class CustomerAPITests
    {
        private readonly CustomersController _Controller;
        private readonly Mock<IEStoreRepo> _repo;
        private readonly IFixture _fixture;

        public CustomerAPITests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _Controller = new CustomersController(_repo.Object);
        }

        // GET: api/Customers
        [Fact]
        public async Task TestGetCustomers()
        {
            // arrange
            var customers = _fixture.CreateMany<Customer>(5).ToList();
            _repo.Setup(r => r.GetCustomersAsync()).ReturnsAsync(customers);

            // act
            var result = await _Controller.GetAllCustomersAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>
            Assert.Equal(5, customersResult.Count);   // returns 5 customers
        }

        [Fact]
        public async Task TestGetEmptyCustomers()
        {
            // arrange
            _repo.Setup(r => r.GetCustomersAsync()).ReturnsAsync(new List<Customer>());

            // act
            var result = await _Controller.GetAllCustomersAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>
            Assert.Empty(customersResult);   // returns empty list
        }

        // GET: api/Customers/{id}
        [Fact]
        public async Task TestGetCustomersByValidId()
        {
            var customers = _fixture.CreateMany<Customer>(5);
            foreach(var customer in customers)
            {
                _repo.Setup(r => r.GetCustomerByIdAsync(customer.CustomerId)).ReturnsAsync(customer);
            }

            // act
            foreach (var customer in customers)
            {
                var result = await _Controller.GetCustomerAsync(customer.CustomerId);

                // assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
                var customerResult = Assert.IsAssignableFrom<Customer>(okResult.Value); // return type Customer
                Assert.Equal(customer.CustomerId, customerResult.CustomerId);   // matching id        
            }
        }

        [Fact]
        public async Task TestGetCustomersByInvalidId()
        {
            // arrange
            var customers = _fixture.CreateMany<Customer>(5);
            foreach (var customer in customers)
            {
                _repo.Setup(r => r.GetCustomerByIdAsync(customer.CustomerId)).ReturnsAsync(customer);
            }

            int validId = customers.Max(x => x.CustomerId);

            IEnumerable<int> invalidIds = new List<int> { -1, validId + 1 };

            foreach (int id in invalidIds)
            {
                // act
                var invalidResult = await _Controller.GetCustomerAsync(id);

                // assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(invalidResult.Result); // returns 404 Not found
            }
        }
    }
}