using AutoFixture;
using Moq;
using AutoFixture.AutoMoq;
using EStoreAPI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using AutoFixture.Kernel;
using System.Reflection;

namespace EStoreAPI.Tests.APITests
{
    public abstract class APITests<T> where T : ControllerBase
    {
        protected readonly T _controller;
        protected readonly Mock<IEStoreRepo> _repo;
        protected readonly IFixture _fixture;

        public APITests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new NoCircularReferencesCustomization())
                .Customize(new IgnoreVirtualMembersCustomization());

            _repo = _fixture.Freeze<Mock<IEStoreRepo>>();
            _controller = (T)Activator.CreateInstance(typeof(T), _repo.Object);
        }
    }

    // resolves the issue with fixture circular references
    public class NoCircularReferencesCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(behavior => fixture.Behaviors.Remove(behavior));
        }
    }

    public class IgnoreVirtualMembersCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new IgnoreVirtualMembers());
        }
    }

    public class IgnoreVirtualMembers : ISpecimenBuilder
    {
        public object? Create(object request, ISpecimenContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var propertyInfo = request as PropertyInfo;
            if (propertyInfo == null)
            {
                return new NoSpecimen();
            }

            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsVirtual)
            {
                return null;
            }

            return new NoSpecimen();
        }
    }
}
