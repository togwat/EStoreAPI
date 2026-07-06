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
    public class CustomerAPITests : APITests<CustomersController, ICustomerService>
    {
        // GET: api/Customers
        [Fact]
        public async Task TestGetCustomers()
        {
            // arrange
            var customers = _fixture.CreateMany<Customer>(5).ToList();
            _service.Setup(s => s.GetAllCustomersAsync()).ReturnsAsync(customers);

            // act
            var result = await _controller.GetAllCustomersAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<OutCustomerDTO>>(okResult.Value);   // return type ICollection<OutCustomerDTO>
            Assert.Equal(5, customersResult.Count);   // returns 5 customers
        }

        [Fact]
        public async Task TestGetEmptyCustomers()
        {
            // arrange
            _service.Setup(s => s.GetAllCustomersAsync()).ReturnsAsync(new List<Customer>());

            // act
            var result = await _controller.GetAllCustomersAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<OutCustomerDTO>>(okResult.Value);   // return type ICollection<OutCustomerDTO>
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
            _service.Setup(s => s.GetCustomerAsync(1)).ReturnsAsync(customer);
            _service.Setup(s => s.GetCustomerAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Customer);

            // act
            var result = await _controller.GetCustomerAsync(id);

            // assert
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
                var customerResult = Assert.IsAssignableFrom<OutCustomerDTO>(okResult.Value); // return type OutCustomerDTO
                Assert.Equal(customer.CustomerId, customerResult.CustomerId);   // matching id
            }
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
            _service.Setup(s => s.SearchCustomersAsync(It.Is<string>(s => "name".Contains(s)))).ReturnsAsync(customers);
            _service.Setup(s => s.SearchCustomersAsync("notname")).ReturnsAsync(new List<Customer>());

            // act
            var result = await _controller.SearchCustomersAsync(name);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<OutCustomerDTO>>(okResult.Value);   // return type ICollection<OutCustomerDTO>

            if (name == "name")
            {
                Assert.Equal(customers.Count, customersResult.Count);   // returns 2 customers
                Assert.All(customersResult, c => Assert.Contains("name", c.CustomerName));
            }
            else if (name == "na")
            {
                Assert.Equal(customers.Count, customersResult.Count);   // returns 2 customers
                Assert.All(customersResult, c => Assert.Contains("na", c.CustomerName));
            }
            else
            {
                Assert.Empty(customersResult);  // returns no customers
            }
        }

        [Theory]
        [InlineData("12345")]   // valid
        [InlineData("45678")]   // invalid
        [InlineData("123")]     // invalid
        public async Task TestSearchCustomersPrimaryContact(string primaryContact)
        {
            // arrange
            Customer customer = _fixture.Build<Customer>()
                                    .With(c => c.PrimaryContact, "12345")
                                    .Create();
            _service.Setup(s => s.SearchCustomersAsync("12345")).ReturnsAsync(new List<Customer> { customer });
            _service.Setup(s => s.SearchCustomersAsync(It.Is<string>(s => s != "12345"))).ReturnsAsync(new List<Customer>());

            // act
            var result = await _controller.SearchCustomersAsync(primaryContact);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<OutCustomerDTO>>(okResult.Value);

            if (primaryContact == "12345")
            {
                Assert.Single(customersResult);   // returns 1 customer
                Assert.All(customersResult, c => Assert.Equal("12345", c.PrimaryContact));
            }
            else
            {
                Assert.Empty(customersResult);   // returns no customer
            }
        }

        [Theory]
        [InlineData("test@email")]  // valid
        [InlineData("no@email")]    // invalid
        [InlineData("test")]        // partial, valid
        public async Task TestSearchCustomersEmail(string email)
        {
            // arrange
            Customer customer = _fixture.Build<Customer>()
                                    .With(c => c.Email, "test@email")
                                    .Create();
            _service.Setup(s => s.SearchCustomersAsync(It.Is<string>(s => "test@email".Contains(s)))).ReturnsAsync(new List<Customer> { customer });
            _service.Setup(s => s.SearchCustomersAsync("no@email")).ReturnsAsync(new List<Customer>());

            // act
            var result = await _controller.SearchCustomersAsync(email);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<OutCustomerDTO>>(okResult.Value);

            if (email == "test@email" || email == "test")
            {
                Assert.Single(customersResult);
                Assert.All(customersResult, c => Assert.Contains("test@email", c.Email));
            }
            else
            {
                Assert.Empty(customersResult);
            }
        }

        [Fact]
        public async Task TestSearchCustomersEmpty()
        {
            // arrange
            var customers = _fixture.CreateMany<Customer>(5).ToList();
            _service.Setup(s => s.SearchCustomersAsync(null)).ReturnsAsync(customers);

            // act
            var result = await _controller.SearchCustomersAsync(null);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 Ok
            var customersResult = Assert.IsAssignableFrom<ICollection<OutCustomerDTO>>(okResult.Value);
            Assert.Equal(5, customersResult.Count);   // returns all customers
        }

        // POST: api/Customers/create
        [Theory]
        [InlineData("a", "123")]            // valid customer, numeric contact
        [InlineData("a", "test@email")]     // valid customer, non-numeric contact
        [InlineData(null, "123")]           // invalid name
        [InlineData("b", null)]             // invalid contact: null
        [InlineData(null, null)]            // invalid name and contact
        public async Task TestCreateCustomer(string name, string primaryContact)
        {
            // arrange
            bool valid = name != null && primaryContact != null;
            var dto = new InCustomerDTO { CustomerName = name, PrimaryContact = primaryContact };
            Customer newCustomer = _fixture.Build<Customer>()
                                    .With(c => c.CustomerId, 1)
                                    .With(c => c.CustomerName, name)
                                    .With(c => c.PrimaryContact, primaryContact)
                                    .Create();

            if (valid)
            {
                _service.Setup(s => s.CreateCustomerAsync(dto)).ReturnsAsync(newCustomer);
            }
            else
            {
                _service.Setup(s => s.CreateCustomerAsync(dto)).ThrowsAsync(new ValidationException());
            }

            // act
            var result = await _controller.CreateCustomerAsync(dto);

            // assert
            if (valid)
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdCustomer = Assert.IsAssignableFrom<OutCustomerDTO>(createdResult.Value);

                Assert.Equal(newCustomer.CustomerName, createdCustomer.CustomerName);
                Assert.Equal(newCustomer.PrimaryContact, createdCustomer.PrimaryContact);
                Assert.Equal(newCustomer.Email, createdCustomer.Email);
            }
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        // POST: api/Customers/create-bulk
        [Theory]
        [InlineData(-1, "123")]     // all valid
        [InlineData(0, "123")]      // customer at index 0 is invalid
        [InlineData(1, "123")]      // customer at index 1 is invalid
        [InlineData(-1, "abc")]     // invalid phone: no digits
        public async Task TestCreateCustomers(int invalidIndex, string phone)
        {
            // arrange
            var dtos = _fixture.Build<InCustomerDTO>()
                                .With(d => d.CustomerName, "name")
                                .With(d => d.PhoneNumber, phone)
                                .CreateMany(3).ToList();

            if (invalidIndex >= 0)
            {
                _service.Setup(s => s.CreateCustomersAsync(It.IsAny<ICollection<InCustomerDTO>>()))
                        .ThrowsAsync(new ValidationException($"Customer at index {invalidIndex} is missing required fields."));
            }
            else if (phone != "123")
            {
                _service.Setup(s => s.CreateCustomersAsync(It.IsAny<ICollection<InCustomerDTO>>()))
                        .ThrowsAsync(new ValidationException("Phone number must contain only numbers."));
            }
            else
            {
                var newCustomers = dtos.Select((d, i) => _fixture.Build<Customer>()
                    .With(c => c.CustomerId, i + 1)
                    .With(c => c.CustomerName, d.CustomerName)
                    .With(c => c.PrimaryContact, d.PrimaryContact)
                    .With(c => c.PhoneNumber, d.PhoneNumber)
                    .Create()).ToList();

                _service.Setup(s => s.CreateCustomersAsync(It.IsAny<ICollection<InCustomerDTO>>()))
                        .ReturnsAsync(newCustomers);
            }

            // act
            var result = await _controller.CreateCustomersAsync(dtos);

            // assert
            if (invalidIndex < 0 && phone == "123")
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdCustomers = Assert.IsAssignableFrom<ICollection<OutCustomerDTO>>(createdResult.Value);
                Assert.Equal(3, createdCustomers.Count);
                var createdList = createdCustomers.ToList();
                for (int i = 0; i < createdList.Count; i++)
                    Assert.Equal(i + 1, createdList[i].CustomerId);
            }
            else if (invalidIndex >= 0)
            {
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);    // returns 400 with message
                Assert.Contains($"index {invalidIndex}", badRequestResult.Value!.ToString());
            }
            else
            {
                Assert.IsType<BadRequestObjectResult>(result.Result);   // returns 400 bad request
            }
        }

        // PUT: api/Customers/update/{id}
        [Theory]
        [InlineData(1, "newname", "123")]    // valid id, valid data
        [InlineData(1, null, null)]          // valid id, invalid data
        [InlineData(1, "newname", "abc")]    // valid id, invalid data: non-digit phone
        [InlineData(2, "newname", "123")]    // invalid id, valid data
        [InlineData(2, null, null)]          // invalid id, invalid data
        public async Task TestUpdateCustomer(int id, string name, string phone)
        {
            // arrange
            var dto = new UpdateCustomerDTO { CustomerId = id, CustomerName = name, PhoneNumber = phone };

            if (id == 1)
            {
                if (name == "newname" && phone == "123")
                    _service.Setup(s => s.UpdateCustomerAsync(dto)).Returns(Task.CompletedTask);
                else
                    _service.Setup(s => s.UpdateCustomerAsync(dto)).ThrowsAsync(new ValidationException());
            }
            else
            {
                _service.Setup(s => s.UpdateCustomerAsync(dto)).ThrowsAsync(new KeyNotFoundException("Customer not found."));
            }

            // act
            var result = await _controller.UpdateCustomerAsync(id, dto);

            // assert
            if (id == 1)
            {
                if (name == "newname" && phone == "123")
                    Assert.IsType<NoContentResult>(result); // returns 204 no content
                else
                    Assert.IsType<BadRequestResult>(result);    // returns 400 bad request
            }
            else
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                Assert.Equal("Customer not found.", notFoundResult.Value);
            }
        }
    }
}
