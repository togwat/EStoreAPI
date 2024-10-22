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
        [Theory]
        [InlineData(1)]     // valid id
        [InlineData(-1)]    // invalid id
        [InlineData(2)]     // invalid id
        public async Task TestGetCustomerById(int id)
        {
            // arrange
            Customer customer = _fixture.Build<Customer>()
                                        .With(c => c.CustomerId, 1)
                                        .Create();
            _repo.Setup(r => r.GetCustomerByIdAsync(1)).ReturnsAsync(customer);

            // act
            var result = await _controller.GetCustomerAsync(id);

            // assert
            // valid id
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
                var customerResult = Assert.IsAssignableFrom<Customer>(okResult.Value); // return type Customer
                Assert.Equal(customer.CustomerId, customerResult.CustomerId);   // matching id        
            }
            // invalid id
            else
            {
                Assert.IsType<NotFoundObjectResult>(result.Result); // returns 404 Not found
            }
        }

        // GET: api/Customers/search?query=
        [Theory]
        [InlineData("name")]
        [InlineData("na")]
        [InlineData("notname")]
        public async Task TestSearchCustomersName(string name)
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
        public async Task TestSearchCustomersPhone(string phone)
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

        // POST: api/Customers/create
        [Theory]
        [InlineData("a", new string[] { "123" })]   // valid customer
        [InlineData(null, new string[] { "123" })]  // invalid name, valid phone
        [InlineData("b", new string[] { })]     // valid name, invalid phone
        [InlineData(null, new string[] { })]    // invalid name, invalid phone
        [InlineData(null, null)]                // invalid name, invalid phone
        public async Task TestCreateCustomer(string name, string[] phones)
        {
            // arrange
            var newCustomer = _fixture.Build<Customer>()
                                    .Without(c => c.CustomerId)
                                    .With(c => c.CustomerName, name)
                                    .With(c => c.PhoneNumbers, phones)
                                    .Create();
            _repo.Setup(r => r.AddCustomerAsync(newCustomer))
                .ReturnsAsync((Customer c) =>
                {
                    c.CustomerId = 1; // EF auto-increments id
                    return c;
                });

            // act
            var reuslt = await _controller.CreateCustomerAsync(newCustomer);

            // assert
            // valid customer
            if (name == "a" && Enumerable.SequenceEqual(phones, ["123"]))
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(reuslt.Result);
                var createdCustomer = Assert.IsAssignableFrom<Customer>(createdResult.Value);

                Assert.Equal(newCustomer.CustomerName, createdCustomer.CustomerName);
                Assert.Equal(newCustomer.PhoneNumbers, createdCustomer.PhoneNumbers);
                Assert.Equal(newCustomer.Email, createdCustomer.Email);
            }
            // invalid customer
            else
            {
                Assert.IsType<BadRequestResult>(reuslt.Result);
            }
        }
    }
}