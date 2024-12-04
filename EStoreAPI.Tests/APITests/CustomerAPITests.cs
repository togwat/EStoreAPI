using AutoFixture;
using Moq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Tests.APITests
{
    public class CustomerAPITests : APITests<CustomersController>
    {
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
            _repo.Setup(r => r.GetCustomerByIdAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Customer);

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
                Assert.IsType<NotFoundResult>(result.Result); // returns 404 Not found
            }
        }

        // GET: api/Customers/search?query=
        [Theory]
        [InlineData("name")]    // valid name
        [InlineData("na")]      // partial name, valid
        [InlineData("notname")] // invalid name
        public async Task TestSearchCustomersName(string name)
        {
            // arrange
            var customers = _fixture.Build<Customer>()
                                    .With(c => c.CustomerName, "name")
                                    .CreateMany(2).ToList();
            _repo.Setup(r => r.GetCustomersByQueryAsync(It.Is<string>(s => "name".Contains(s)))).ReturnsAsync(customers);

            // act
            var result = await _controller.SearchCustomersAsync(name);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>

            // valid name
            if (name == "name")
            {
                Assert.Equal(customers.Count, customersResult.Count);   // returns 2 customers
                Assert.All(customersResult, c => Assert.Contains("name", c.CustomerName)); // all returned customer contain name "name"
            }
            // partial name
            else if (name == "na")
            {
                Assert.Equal(customers.Count, customersResult.Count);   // returns 2 customers
                Assert.All(customersResult, c => Assert.Contains("na", c.CustomerName)); // all returned customer contain name "na"
            }
            // invalid name
            else
            {
                Assert.Empty(customersResult);  // returns no customers
            }
        }

        [Theory]
        [InlineData("12345")]   // valid
        [InlineData("45678")]   // invalid
        [InlineData("123")]     // invalid
        public async Task TestSearchCustomersPhone(string phone)
        {
            // arrange
            Customer customer = _fixture.Build<Customer>()
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
                Assert.All(customersResult, c => Assert.Contains("12345", c.PhoneNumbers)); // customer should have phone number
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
            Customer customer = _fixture.Build<Customer>()
                                    .With(c => c.Email, "test@email")
                                    .Create();
            _repo.Setup(r => r.GetCustomersByQueryAsync(It.Is<string>(s => "test@email".Contains(s)))).ReturnsAsync([customer]);

            // act
            var result = await _controller.SearchCustomersAsync(email);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<Customer>>(okResult.Value);   // return type ICollection<Customer>

            // valid email
            if (email == "test@email" || email == "test")
            {
                Assert.Single(customersResult);   // returns 1 customer
                Assert.All(customersResult, c => Assert.Contains("test@email", c.Email)); // customer should have email
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
            Customer newCustomer = _fixture.Build<Customer>()
                                    .Without(c => c.CustomerId)
                                    .With(c => c.CustomerName, name)
                                    .With(c => c.PhoneNumbers, phones)
                                    .Create();
            // valid data
            if (name == "a" && Enumerable.SequenceEqual(phones, ["123"]))
            {
                _repo.Setup(r => r.AddCustomerAsync(newCustomer))
                .ReturnsAsync((Customer c) =>
                {
                    c.CustomerId = 1; // EF auto-increments id
                    return c;
                });
            }
            else
            {
                _repo.Setup(r => r.AddCustomerAsync(newCustomer)).ThrowsAsync(new ValidationException());
            }
            // act
            var result = await _controller.CreateCustomerAsync(newCustomer);

            // assert
            // valid customer
            if (name == "a" && Enumerable.SequenceEqual(phones, ["123"]))
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdCustomer = Assert.IsAssignableFrom<Customer>(createdResult.Value);   // return type Customer

                // returned customer should match the sent customer
                Assert.Equal(newCustomer.CustomerName, createdCustomer.CustomerName);
                Assert.Equal(newCustomer.PhoneNumbers, createdCustomer.PhoneNumbers);
                Assert.Equal(newCustomer.Email, createdCustomer.Email);
            }
            // invalid customer
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        // PUT: api/Customers/update/{id}
        [Theory]
        [InlineData(1, "newname", new string[] { "123" })]     // valid id, valid data
        [InlineData(1, null, null)]    // valid id, invalid data
        [InlineData(2, "newname", new string[] { "123" })]     // invalid id, valid data
        [InlineData(2, null, null)]    // invalid id, invalid data
        public async Task TestUpdateCustomer(int id, string name, string[] phones)
        {
            // arrange
            Customer oldCustomer = _fixture.Build<Customer>()
                                        .With(c => c.CustomerId, 1)
                                        .Create();
            Customer newCustomer = _fixture.Build<Customer>()
                                        .With(c => c.CustomerId, 1)     // supplied customer will have same id as oldCustomer
                                        .With(c => c.CustomerName, name)
                                        .With(c => c.PhoneNumbers, phones)
                                        .Create();
            // valid id
            if (id == 1)
            {
                // valid data
                if (name == "newname" && Enumerable.SequenceEqual(phones, ["123"]))
                {
                    _repo.Setup(r => r.UpdateCustomerAsync(newCustomer)).Returns(Task.CompletedTask);
                }
                else
                {
                    _repo.Setup(r => r.UpdateCustomerAsync(newCustomer)).ThrowsAsync(new ValidationException());
                }
                    
            }
            // not found id
            else
            {
                _repo.Setup(r => r.UpdateCustomerAsync(newCustomer)).ThrowsAsync(new KeyNotFoundException("Customer not found."));
            }

            // act
            var result = await _controller.UpdateCustomerWithIdAsync(id, newCustomer);

            // assert
            // valid id
            if (id == 1)
            {
                // valid data
                if (name == "newname" && Enumerable.SequenceEqual(phones, ["123"]))
                {
                    Assert.IsType<NoContentResult>(result); // returns 204 no content
                }
                // invalid data
                else
                {
                    Assert.IsType<BadRequestResult>(result);    // returns 400 bad request
                }
            }
            // invalid id
            else
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                Assert.Equal("Customer not found.", notFoundResult.Value);  // matching error message
            }
        }
    }
}