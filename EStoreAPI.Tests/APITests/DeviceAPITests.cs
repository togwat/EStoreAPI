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
    public class DeviceAPITests : APITests<DevicesController, IDeviceService>
    {
        // GET: api/Devices
        [Fact]
        public async Task TestGetDevices()
        {
            // arrange
            var devices = _fixture.CreateMany<Device>(5).ToList();
            _service.Setup(s => s.GetAllDevicesAsync()).ReturnsAsync(devices);

            // act
            var result = await _controller.GetAllDevicesAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);
            Assert.Equal(5, devicesResult.Count);   // returns 5 devices
        }

        [Fact]
        public async Task TestGetEmptyDevices()
        {
            // arrange
            _service.Setup(s => s.GetAllDevicesAsync()).ReturnsAsync(new List<Device>());

            // act
            var result = await _controller.GetAllDevicesAsync();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);
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
            _service.Setup(s => s.GetDeviceAsync(1)).ReturnsAsync(device);
            _service.Setup(s => s.GetDeviceAsync(It.Is<int>(i => i != 1))).ReturnsAsync(null as Device);

            // act
            var result = await _controller.GetDeviceAsync(id);

            // assert
            if (id == 1)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var deviceResult = Assert.IsAssignableFrom<Device>(okResult.Value);
                Assert.Equal(device.DeviceId, deviceResult.DeviceId);
            }
            else
            {
                Assert.IsType<NotFoundResult>(result.Result); // returns 404 not found
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
            _service.Setup(s => s.SearchDevicesByNameAsync(It.Is<string>(s => "phone".Contains(s)))).ReturnsAsync(new List<Device> { device });
            _service.Setup(s => s.SearchDevicesByNameAsync(It.Is<string>(s => !"phone".Contains(s)))).ReturnsAsync(new List<Device>());

            // act
            var result = await _controller.SearchDevicesNameAsync(name);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);

            if (name == "phone" || name == "pho")
            {
                Assert.Single(devicesResult);   // returns 1 device
                Assert.Contains(devicesResult, d => d.DeviceName.Contains("phone"));
            }
            else
            {
                Assert.Empty(devicesResult); // returns no device
            }
        }

        // GET: api/Devices/searchType?type=
        [Theory]
        [InlineData("phone")]   // valid
        [InlineData("pho")]     // partial, invalid (type search is exact)
        [InlineData("tablet")]  // invalid
        public async Task TestSearchDeviceType(string type)
        {
            // arrange
            var devices = _fixture.Build<Device>()
                                    .With(d => d.DeviceType, "phone")
                                    .CreateMany(5).ToList();
            _service.Setup(s => s.SearchDevicesByTypeAsync("phone")).ReturnsAsync(devices);
            _service.Setup(s => s.SearchDevicesByTypeAsync(It.Is<string>(s => s != "phone"))).ReturnsAsync(new List<Device>());

            // act
            var result = await _controller.SearchDevicesTypeAsync(type);

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
            var devicesResult = Assert.IsAssignableFrom<ICollection<Device>>(okResult.Value);

            if (type == "phone")
            {
                Assert.Equal(devices.Count, devicesResult.Count);   // returns 5 devices
                Assert.All(devicesResult, d => Assert.Equal("phone", d.DeviceType));
            }
            else
            {
                Assert.Empty(devicesResult);    // returns no device
            }
        }

        // POST: api/Devices/create
        [Theory]
        [InlineData("name", "type")]    // valid
        [InlineData("", "type")]        // invalid name
        [InlineData("name", "")]        // invalid type
        [InlineData("", "")]            // invalid name and type
        public async Task TestCreateDevice(string name, string type)
        {
            // arrange
            var dto = new DeviceDTO { DeviceName = name, DeviceType = type };
            Device newDevice = _fixture.Build<Device>()
                                    .With(d => d.DeviceId, 1)
                                    .With(d => d.DeviceName, name)
                                    .With(d => d.DeviceType, type)
                                    .Create();

            if (name == "name" && type == "type")
            {
                _service.Setup(s => s.CreateDeviceAsync(dto)).ReturnsAsync(newDevice);
            }
            else
            {
                _service.Setup(s => s.CreateDeviceAsync(dto)).ThrowsAsync(new ValidationException());
            }

            // act
            var result = await _controller.CreateDeviceAsync(dto);

            // assert
            if (name == "name" && type == "type")
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdDevice = Assert.IsAssignableFrom<Device>(createdResult.Value);

                Assert.Equal(newDevice.DeviceName, createdDevice.DeviceName);
                Assert.Equal(newDevice.DeviceType, createdDevice.DeviceType);
            }
            else
            {
                Assert.IsType<BadRequestResult>(result.Result);
            }
        }

        // POST: api/Devices/create-bulk
        [Theory]
        [InlineData(-1)]    // all valid
        [InlineData(0)]     // device at index 0 is invalid
        [InlineData(1)]     // device at index 1 is invalid
        public async Task TestCreateDevices(int invalidIndex)
        {
            // arrange
            var dtos = _fixture.Build<DeviceDTO>()
                                .With(d => d.DeviceName, "name")
                                .With(d => d.DeviceType, "type")
                                .CreateMany(3).ToList();

            if (invalidIndex >= 0)
            {
                _service.Setup(s => s.CreateDevicesAsync(It.IsAny<ICollection<DeviceDTO>>()))
                        .ThrowsAsync(new ValidationException($"Device at index {invalidIndex} is missing required fields."));
            }
            else
            {
                var newDevices = dtos.Select((d, i) => _fixture.Build<Device>()
                    .With(dev => dev.DeviceId, i + 1)
                    .With(dev => dev.DeviceName, d.DeviceName)
                    .With(dev => dev.DeviceType, d.DeviceType)
                    .Create()).ToList();

                _service.Setup(s => s.CreateDevicesAsync(It.IsAny<ICollection<DeviceDTO>>()))
                        .ReturnsAsync(newDevices);
            }

            // act
            var result = await _controller.CreateDevicesAsync(dtos);

            // assert
            if (invalidIndex < 0)
            {
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);    // returns 201 created
                var createdDevices = Assert.IsAssignableFrom<ICollection<Device>>(createdResult.Value);
                Assert.Equal(3, createdDevices.Count);
                var createdList = createdDevices.ToList();
                for (int i = 0; i < createdList.Count; i++)
                    Assert.Equal(i + 1, createdList[i].DeviceId);
            }
            else
            {
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);    // returns 400 with message
                Assert.Contains($"index {invalidIndex}", badRequestResult.Value!.ToString());
            }
        }

        // PUT: api/Devices/update/{id}
        [Theory]
        [InlineData(1, "newname", "newtype")]    // valid id, valid data
        [InlineData(1, "", "")]                  // valid id, invalid data
        [InlineData(1, "newname", "")]           // valid id, invalid data
        [InlineData(1, "", "newtype")]           // valid id, invalid data
        [InlineData(2, "newname", "newtype")]    // invalid id, valid data
        [InlineData(2, "", "")]                  // invalid id, invalid data
        [InlineData(2, "newname", "")]           // invalid id, invalid data
        [InlineData(2, "", "newtype")]           // invalid id, invalid data
        public async Task TestUpdateDevice(int id, string name, string type)
        {
            // arrange
            var dto = new DeviceDTO { DeviceName = name, DeviceType = type };

            if (id == 1)
            {
                if (name == "newname" && type == "newtype")
                    _service.Setup(s => s.UpdateDeviceAsync(id, dto)).Returns(Task.CompletedTask);
                else
                    _service.Setup(s => s.UpdateDeviceAsync(id, dto)).ThrowsAsync(new ValidationException());
            }
            else
            {
                _service.Setup(s => s.UpdateDeviceAsync(id, dto)).ThrowsAsync(new KeyNotFoundException("Device not found."));
            }

            // act
            var result = await _controller.UpdateDeviceAsync(id, dto);

            // assert
            if (id == 1)
            {
                if (name == "newname" && type == "newtype")
                    Assert.IsType<NoContentResult>(result); // returns 204 no content
                else
                    Assert.IsType<BadRequestResult>(result);    // returns 400 bad request
            }
            else
            {
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);   // returns 404 not found
                Assert.Equal("Device not found.", notFoundResult.Value);
            }
        }
    }
}
