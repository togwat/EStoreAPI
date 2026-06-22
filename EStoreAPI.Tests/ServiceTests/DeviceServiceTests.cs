using AutoFixture;
using AutoFixture.AutoMoq;
using EStoreAPI.Server.Data;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.Services;
using EStoreAPI.Tests.APITests;
using Moq;

namespace EStoreAPI.Tests.ServiceTests
{
    public class DeviceServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IEStoreRepo> _repo;
        private readonly DeviceService _deviceService;

        public DeviceServiceTests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new NoCircularReferencesCustomization())
                .Customize(new IgnoreVirtualMembersCustomization());

            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _deviceService = new DeviceService(_repo.Object);
        }

        // returns every device the repo holds
        [Fact]
        public async Task GetAllDevices_ReturnsAllFromRepo()
        {
            var devices = _fixture.CreateMany<Device>(3).ToList();
            _repo.Setup(r => r.GetDevicesAsync()).ReturnsAsync(devices);

            var result = await _deviceService.GetAllDevicesAsync();

            Assert.Equal(3, result.Count);
        }

        // an existing id resolves to that device
        [Fact]
        public async Task GetDevice_Exists_ReturnsDevice()
        {
            var device = _fixture.Create<Device>();
            _repo.Setup(r => r.GetDeviceByIdAsync(device.DeviceId)).ReturnsAsync(device);

            var result = await _deviceService.GetDeviceAsync(device.DeviceId);

            Assert.Equal(device.DeviceId, result?.DeviceId);
        }

        // an unknown id returns null
        [Fact]
        public async Task GetDevice_NotFound_ReturnsNull()
        {
            _repo.Setup(r => r.GetDeviceByIdAsync(It.IsAny<int>())).ReturnsAsync((Device?)null);

            var result = await _deviceService.GetDeviceAsync(404);

            Assert.Null(result);
        }

        // the distinct list of device types is forwarded from the repo
        [Fact]
        public async Task GetDeviceTypes_ReturnsTypes()
        {
            var types = new List<string> { "phone", "tablet", "laptop" };
            _repo.Setup(r => r.GetDeviceTypesAsync()).ReturnsAsync(types);

            var result = await _deviceService.GetDeviceTypesAsync();

            Assert.Equal(3, result.Count);
        }

        // search by name forwards to the repo's name lookup
        [Fact]
        public async Task SearchDevicesByName_ReturnsMatches()
        {
            var matches = _fixture.CreateMany<Device>(2).ToList();
            _repo.Setup(r => r.GetDevicesByNameAsync("iphone")).ReturnsAsync(matches);

            var result = await _deviceService.SearchDevicesByNameAsync("iphone");

            Assert.Equal(2, result.Count);
            _repo.Verify(r => r.GetDevicesByNameAsync("iphone"), Times.Once);
        }

        // search by type forwards to the repo's type lookup
        [Fact]
        public async Task SearchDevicesByType_ReturnsMatches()
        {
            var matches = _fixture.CreateMany<Device>(2).ToList();
            _repo.Setup(r => r.GetDevicesByTypeAsync("phone")).ReturnsAsync(matches);

            var result = await _deviceService.SearchDevicesByTypeAsync("phone");

            Assert.Equal(2, result.Count);
            _repo.Verify(r => r.GetDevicesByTypeAsync("phone"), Times.Once);
        }

        // the broad search fans out across the repo's lookups and a device matching
        // more than one of them is returned only once
        [Fact]
        public async Task SearchDevices_OverlappingSources_Deduplicates()
        {
            var device = _fixture.Create<Device>();
            // the same device surfaces from every lookup the broad search might consult
            _repo.Setup(r => r.GetDevicesByNameAsync(It.IsAny<string>())).ReturnsAsync([device]);
            _repo.Setup(r => r.GetDevicesByModelNumberAsync(It.IsAny<string>())).ReturnsAsync([device]);
            _repo.Setup(r => r.GetDevicesByTypeAsync(It.IsAny<string>())).ReturnsAsync([device]);

            var result = await _deviceService.SearchDevicesAsync("query");

            Assert.Single(result);
        }

        // creating a device persists it and returns the stored entity
        [Fact]
        public async Task CreateDevice_PersistsAndReturns()
        {
            var dto = _fixture.Create<InDeviceDTO>();
            var created = _fixture.Create<Device>();
            _repo.Setup(r => r.AddDeviceAsync(It.IsAny<Device>())).ReturnsAsync(created);

            var result = await _deviceService.CreateDeviceAsync(dto);

            Assert.Equal(created.DeviceId, result.DeviceId);
            _repo.Verify(r => r.AddDeviceAsync(It.IsAny<Device>()), Times.Once);
        }

        // bulk create persists every device
        [Fact]
        public async Task CreateDevices_Bulk_PersistsAll()
        {
            var dtos = _fixture.CreateMany<InDeviceDTO>(3).ToList();
            var created = _fixture.CreateMany<Device>(3).ToList();
            _repo.Setup(r => r.AddDevicesAsync(It.IsAny<ICollection<Device>>())).ReturnsAsync(created);

            var result = await _deviceService.CreateDevicesAsync(dtos);

            Assert.Equal(3, result.Count);
            _repo.Verify(r => r.AddDevicesAsync(It.IsAny<ICollection<Device>>()), Times.Once);
        }

        // a partially-filled update DTO overwrites only the provided fields, leaving the rest untouched
        [Fact]
        public async Task UpdateDevice_PartialDto_OnlyOverwritesProvidedFields()
        {
            var original = _fixture.Create<Device>();
            var originalName = original.DeviceName;
            var originalType = original.DeviceType;
            _repo.Setup(r => r.GetDeviceByIdAsync(original.DeviceId)).ReturnsAsync(original);

            // only the model number is supplied
            var dto = new UpdateDeviceDTO { DeviceId = original.DeviceId, ModelNumber = "A1234" };

            await _deviceService.UpdateDeviceAsync(dto);

            Assert.Equal("A1234", original.ModelNumber);
            Assert.Equal(originalName, original.DeviceName);
            Assert.Equal(originalType, original.DeviceType);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.Once);
        }

        // bulk update applies the same partial-overwrite rule to each device independently
        [Fact]
        public async Task UpdateDevices_Bulk_PartialDtos_OnlyOverwriteProvidedFields()
        {
            var first = _fixture.Create<Device>();
            var second = _fixture.Create<Device>();
            var firstOriginalType = first.DeviceType;
            var secondOriginalName = second.DeviceName;
            _repo.Setup(r => r.GetDeviceByIdAsync(first.DeviceId)).ReturnsAsync(first);
            _repo.Setup(r => r.GetDeviceByIdAsync(second.DeviceId)).ReturnsAsync(second);

            var dtos = new List<UpdateDeviceDTO>
            {
                new() { DeviceId = first.DeviceId, DeviceName = "Renamed" },
                new() { DeviceId = second.DeviceId, ModelNumber = "B5678" },
            };

            await _deviceService.UpdateDevicesAsync(dtos);

            // first: only the name changed
            Assert.Equal("Renamed", first.DeviceName);
            Assert.Equal(firstOriginalType, first.DeviceType);
            // second: only the model number changed
            Assert.Equal("B5678", second.ModelNumber);
            Assert.Equal(secondOriginalName, second.DeviceName);
            _repo.Verify(r => r.ApplyUpdateAsync(), Times.AtLeastOnce);
        }
    }
}
