using AutoFixture;
using Moq;
using EStoreAPI.Server.Controllers;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Tests.APITests
{
    public class FormAPITests : APITests<FormController, IFormService>
    {
        // POST: api/Form/submit
        [Theory]
        [MemberData(nameof(SubmitFormData))]
        public async Task TestSubmitForm(bool valid, bool invalidReference)
        {
            // arrange
            var dto = _fixture.Build<InFormDTO>()
                                .With(f => f.PhoneNumber, "12345")
                                .With(f => f.DeviceName, "phone")
                                .With(f => f.Problems, ["broken screen"])
                                .Create();
            var outJob = _fixture.Create<OutJobDTO>();

            if (valid)
            {
                _service.Setup(s => s.SubmitFormAsync(dto)).ReturnsAsync(outJob);
            }
            else if (invalidReference)
            {
                _service.Setup(s => s.SubmitFormAsync(dto)).ThrowsAsync(new KeyNotFoundException("Device not found."));
            }
            else
            {
                _service.Setup(s => s.SubmitFormAsync(dto)).ThrowsAsync(new ValidationException());
            }

            // act
            var result = await _controller.SubmitFormAsync(dto);

            // assert
            if (valid)
            {
                var okResult = Assert.IsType<OkObjectResult>(result.Result);    // returns 200 ok
                var jobResult = Assert.IsAssignableFrom<OutJobDTO>(okResult.Value);
                Assert.Equal(outJob.JobId, jobResult.JobId);
            }
            else if (invalidReference)
            {
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);    // returns 400 bad request with message
                Assert.Equal("Device not found.", badRequestResult.Value);
            }
            else
            {
                Assert.IsType<BadRequestResult>(result.Result); // returns 400 bad request
            }
        }

        public static IEnumerable<object[]> SubmitFormData =>
            [
                [true, false],      // valid form
                [false, true],      // invalid reference (KeyNotFoundException)
                [false, false],     // invalid data (ValidationException)
            ];
    }
}