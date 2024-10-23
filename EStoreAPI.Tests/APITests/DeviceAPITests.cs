using AutoFixture;
using Moq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace EStoreAPI.Tests.APITests
{
    public class DeviceAPITests : APITests<DevicesController>
    {
        // GET: api/Devices
        [Fact]
        public async Task TestGetDevices()
        {
            // arrange
            var devices = _fixture.CreateMany<Device>(5).ToList();
            _repo.Setup(r => r.GetDevicesAsync()).ReturnsAsync(devices);

            // act
            var result = await _controller.GetAllDevicesAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);  // return type ICollection<Device>
            Assert.Equal(5, devicesResult.Count);
        }
    }
}
