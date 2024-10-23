using AutoFixture;
using Moq;
using AutoFixture.AutoMoq;
using EStoreAPI.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace EStoreAPI.Tests.APITests
{
    public abstract class APITests<T> where T : ControllerBase
    {
        protected readonly T _controller;
        protected readonly Mock<IEStoreRepo> _repo;
        protected readonly IFixture _fixture;

        public APITests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _controller = (T)Activator.CreateInstance(typeof(T), _repo.Object);
        }
    }
}
