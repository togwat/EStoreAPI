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
            Device device = _fixture.Build<Device>()
                                    .With(d => d.DeviceName, "phone")
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
                Assert.Contains(devicesResult, d => d.DeviceName.Contains("phone"));
            }
            // invalid name
            else
            {
                Assert.Empty(devicesResult); // returns no device
            }
        }

        // GET: api/Devices/searchType?type=
        [Theory]
        [InlineData("phone")]   // valid
        [InlineData("pho")]     // partial, invalid
        [InlineData("tablet")]  // invalid
        public async Task TestSearchDeviceType(string type)
        {
            // arrange
            var devices = _fixture.Build<Device>()
                                    .With(d => d.DeviceType, "phone")
                                    .CreateMany(5).ToList();
            _repo.Setup(r => r.GetDevicesByTypeAsync("phone")).ReturnsAsync(devices);

            // act
            var result = await _controller.SearchDevicesTypeAsync(type);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);   // return type ICollection<Device>
            
            // valid type
            if (type == "phone")
            {
                Assert.Equal(devices.Count, devicesResult.Count);   // returns 5 devices
                Assert.All(devicesResult, d => Assert.Equal("phone", d.DeviceType));    // all devices have type phone
            }
            // invalid type
            else
            {
                Assert.Empty(devicesResult);    //returns no device
            }
        }

        // POST: api/Devices/create
        [Theory]
        [InlineData("name", "type")]    // valid device
        [InlineData("", "type")]    // invalid
        [InlineData("name", "")]    // invalid
        [InlineData("", "")]        // invalid
        public async Task TestCreateDevice(string name, string type)
        {
            // arrange
            Device newDevice = _fixture.Build<Device>()
                                    .Without(d => d.DeviceId)
                                    .With(d => d.DeviceName, name)
                                    .With(d => d.DeviceType, type)
                                    .Create();
            _repo.Setup(r => r.AddDeviceAsync(newDevice))
                .ReturnsAsync((Device d) =>
                {
                    d.DeviceId = 1; // EF auto-increments id
                    return d;
                });

            // act
            var result = await _controller.CreateDeviceAsync(newDevice);

            // assert
            // valid device
            if (name == "name" && type == "type")
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdDevice = Assert.IsAssignableFrom<Device>(createdResult.Value);   // return type device

                Assert.Equal(newDevice.DeviceName, createdDevice.DeviceName);
                Assert.Equal(newDevice.DeviceType, createdDevice.DeviceType);
            }
            // invalid
            else
            {   
                Assert.IsType<BadRequestResult>(result.Result);
            }
        }

        // PUT: api/Devices/update/{id}
        [Theory]
        [InlineData(1, "newname", "newtype")]   // valid id, valid data
        [InlineData(1, "", "")]     // valid id, invalid data
        [InlineData(1, "newname", "")]     // valid id, invalid data
        [InlineData(1, "", "newtype")]     // valid id, invalid data
        [InlineData(2, "newname", "newtype")]   // invalid id, valid data
        [InlineData(2, "", "")]     // invalid id, invalid data
        [InlineData(2, "newname", "")]     // invalid id, invalid data
        [InlineData(2, "", "newtype")]     // invalid id, invalid data
        public async Task TestUpdateDevice(int id, string name, string type)
        {
            // arrange
            Device oldDevice = _fixture.Build<Device>()
                                        .With(d => d.DeviceId, 1)
                                        .Create();
            Device newDevice = _fixture.Build<Device>()
                                        .With(d => d.DeviceId, 1)
                                        .With(d => d.DeviceName, name)
                                        .With(d => d.DeviceType, type)
                                        .Create();
            // valid id
            if (id == 1)
            {
                _repo.Setup(r => r.UpdateDeviceAsync(newDevice)).Returns(Task.CompletedTask);
            }
            // invalid id
            else
            {
                _repo.Setup(r => r.UpdateDeviceAsync(newDevice)).ThrowsAsync(new KeyNotFoundException("Device not found."));
            }

            // act
            var result = await _controller.UpdateDeviceByIdAsync(id, newDevice);

            // assert
            // valid id
            if (id == 1)
            {
                // valid data
                if (name == "newname" && type == "newtype")
                {
                    Assert.IsType<NoContentResult>(result); // returns 204 no content
                }
                // invalid data
                else
                {
                    var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                    Assert.Equal("Device not found.", notFoundResult.Value);    // matching error message
                }
            }
            // invalid id
            else
            {
                Assert.IsType<BadRequestResult>(result);    // returns 400 bad request
            }
        }
    }
}
