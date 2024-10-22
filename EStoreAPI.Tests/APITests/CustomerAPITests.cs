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
    }
}