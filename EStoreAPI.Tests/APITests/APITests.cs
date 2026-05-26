using AutoFixture;
using Moq;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Mvc;
using AutoFixture.Kernel;
using System.Reflection;

namespace EStoreAPI.Tests.APITests
{
    public abstract class APITests<TController, TService>
        where TController : ControllerBase
        where TService : class
    {
        protected readonly TController _controller;
        protected readonly Mock<TService> _service;
        protected readonly IFixture _fixture;

        public APITests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new NoCircularReferencesCustomization())
                .Customize(new IgnoreVirtualMembersCustomization());

            _service = _fixture.Freeze<Mock<TService>>();
            _controller = (TController)Activator.CreateInstance(typeof(TController), _service.Object)!;
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
