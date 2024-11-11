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
            Assert.Equal(5, devicesResult.Count);   // returns 5 devices
        }

        [Fact]
        public async Task TestGetEmptyDevices()
        {
            // arrange
            _repo.Setup(r => r.GetDevicesAsync()).ReturnsAsync(new List<Device>());

            // act
            var result = await _controller.GetAllDevicesAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);  // return type ICollection<Device>
            Assert.Empty(devicesResult);    // returns empty list
        }

        // GET: api/Devices/{id}
        [Theory]
        [InlineData(1)]     // valid id
        [InlineData(-1)]    // invalid id
        [InlineData(2)]     // invalid id
        public async Task TestGetDeviceById(int id)
        {
            // arrange
            Device device = _fixture.Build<Device>()
                                    .With(d => d.DeviceId, 1)
                                    .Create();
            _repo.Setup(r => r.GetDeviceByIdAsync(1)).ReturnsAsync(device);

            // act
            var result = await _controller.GetDeviceAsync(id);

            // assert
            // valid id
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var deviceResult = Assert.IsAssignableFrom<Device>(okResult.Value); // return type device
                Assert.Equal(device.DeviceId, deviceResult.DeviceId);
            }
            // invalid id
            else
            {
                Assert.IsType<NotFoundObjectResult>(result.Result); // returns 404 not found
            }
        }

        // GET: api/Devices/searchName?name=
        [Theory]
        [InlineData("phone")]   // valid
        [InlineData("pho")]     // partial, valid
        [InlineData("abc")]     // invalid
        [InlineData("phone1")]  // invalid
        public async Task TestSearchDevicesName(string name)
        {
            // arrange
            var device = _fixture.Build<Device>()
                                    .With(d => d.deviceName, "phone")
                                    .Create();
            _repo.Setup(r => r.GetDevicesByNameAsync(It.Is<string>(s => "phone".Contains(s)))).ReturnsAsync([device]);

            // act
            var result = await _controller.SearchDevicesNameAsync(name);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);   // return type ICollection<Device>

            // valid name
            if (name == "phone" || name == "pho")
            {
                Assert.Single(devicesResult);   // returns 1 device
                Assert.Contains(devicesResult, d => d.deviceName.Contains("phone"));
            }
            // invalid name
            else
            {
                Assert.Empty(devicesResult); // returns no device
            }
        }
    }
}
