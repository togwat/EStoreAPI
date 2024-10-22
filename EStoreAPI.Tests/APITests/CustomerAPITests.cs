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
        private readonly CustomersController _controller;
        private readonly Mock<IEStoreRepo> _repo;
        private readonly IFixture _fixture;

        public CustomerAPITests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _controller = new CustomersController(_repo.Object);
        }

        // GET: api/Customers
        [Fact]
        public async Task TestGetCustomers()
        {
            // arrange
            var customers = _fixture.CreateMany<Customer>(5).ToList();
            _repo.Setup(r => r.GetCustomersAsync()).ReturnsAsync(customers);

            // act
            var result = await _controller.GetAllCustomersAsync();

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
            var result = await _controller.GetAllCustomersAsync();

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
                var result = await _controller.GetCustomerAsync(customer.CustomerId);

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
                var invalidResult = await _controller.GetCustomerAsync(id);

                // assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(invalidResult.Result); // returns 404 Not found
            }
        }

        // GET: api/Customers/search?query=
        [Theory]
        [InlineData("name")]
        [InlineData("na")]
        [InlineData("notname")]
        public async Task TestSearchCustomersWithValidName(string name)
        {
            // arrange
            var customers = _fixture.Build<Customer>()
                                    .With(c => c.CustomerName, "name")
                                    .CreateMany(2).ToList();
            _repo.Setup(r => r.GetCustomersByQueryAsync("name")).ReturnsAsync(customers);

            // act
            var result = await _controller.SearchCustomersAsync(name);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>

            // valid name
            if (name == "name")
            {
                Assert.Equal(customers.Count, customersResult.Count);   // returns 2 customers
                Assert.Contains(customersResult, c => c.CustomerName.Contains("name")); // all returned customer contain name "name"
            }
            // partial name
            else if (name == "na")
            {
                Assert.Equal(customers.Count, customersResult.Count);   // returns 2 customers
                Assert.Contains(customersResult, c => c.CustomerName.Contains("na")); // all returned customer contain name "na"
            }
            // invalid name
            else
            {
                Assert.Empty(customersResult);  // returns no customers
            }
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("45678")]
        [InlineData("123")]
        public async Task TestSearchCustomersWithValidPhone(string phone)
        {
            // arrange
            var customer = _fixture.Build<Customer>()
                                    .With(c => c.PhoneNumbers, ["12345", "67890"])
                                    .Create();
            _repo.Setup(r => r.GetCustomersByQueryAsync("12345")).ReturnsAsync([customer]);

            // act
            var result = await _controller.SearchCustomersAsync(phone);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>

            // valid phone
            if (phone == "12345")
            {
                Assert.Single(customersResult);   // returns 1 customer
                Assert.Contains(customersResult, c => c.PhoneNumbers.Contains("12345")); // customer should have phone number
            }
            // invalid phone
            else
            {
                Assert.Empty(customersResult);   // returns no customer
            }
        }

        [Theory]
        [InlineData("test@email")]  // valid
        [InlineData("no@email")]    // invalid
        [InlineData("test")]        // incomplete but valid
        public async Task TestSearchCustomersEmail(string email)
        {
            // arrange
            var customer = _fixture.Build<Customer>()
                                    .With(c => c.Email, "test@email")
                                    .Create();
            _repo.Setup(r => r.GetCustomersByQueryAsync("test@email")).ReturnsAsync([customer]);

            // act
            var result = await _controller.SearchCustomersAsync(email);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>

            // valid email
            if (email == "test@email" || email == "test")
            {
                Assert.Single(customersResult);   // returns 1 customer
                Assert.Contains(customersResult, c => c.Email.Contains("test@email")); // customer should have email
            }
            // invalid email
            else
            {
                Assert.Empty(customersResult);   // returns no customer
            } 
        }

        [Fact]
        public async Task TestSearchCustomersEmpty()
        {
            // arrange
            var customers = _fixture.CreateMany<Customer>(5).ToList();
            _repo.Setup(r => r.GetCustomersAsync()).ReturnsAsync(customers);

            // act
            var result = await _controller.SearchCustomersAsync(null);

            // assert
            // should return all customers
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>
            Assert.Equal(5, customersResult.Count);   // returns 5 customers
        }
    }
}