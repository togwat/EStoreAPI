using EStoreAPI.Server.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace EStoreAPI.Tests.APITests
{
    public class AuthAPITests
    {
        private readonly AuthController _controller;

        public AuthAPITests()
        {
            _controller = new AuthController();
        }

        // GET: api/auth/login
        [Fact]
        public void TestLoginChallengesGoogle()
        {
            // act
            var result = _controller.Login();

            // assert
            var challenge = Assert.IsType<ChallengeResult>(result);     // triggers an auth challenge
            Assert.Contains("Google", challenge.AuthenticationSchemes); // OAuth flow starts at Google
            Assert.Equal("/", challenge.Properties?.RedirectUri);       // lands on the SPA after sign-in
        }

        [Fact]
        public void TestLoginAllowsAnonymous()
        {
            // login must be reachable without a session, unlike every other endpoint
            var method = typeof(AuthController).GetMethod(nameof(AuthController.Login))!;
            Assert.NotEmpty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));
        }

        // POST: api/auth/logout
        [Fact]
        public async Task TestLogoutSignsOutOfCookieSession()
        {
            // arrange
            // HttpContext.SignOutAsync resolves IAuthenticationService from request services
            var authService = new Mock<IAuthenticationService>();
            authService
                .Setup(s => s.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<AuthenticationProperties?>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton(authService.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() },
            };

            // act
            var result = await _controller.Logout();

            // assert
            Assert.IsType<NoContentResult>(result);     // returns 204 NoContent
            authService.Verify(
                s => s.SignOutAsync(It.IsAny<HttpContext>(), "Cookies", It.IsAny<AuthenticationProperties?>()),
                Times.Once);                            // session cookie scheme was signed out
        }

        // GET: api/auth/me
        [Fact]
        public void TestGetMeReturnsEmailAndName()
        {
            // arrange
            // simulate the principal the cookie middleware builds from a session
            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Email, "staff@example.com"),
                    new Claim(ClaimTypes.Name, "Staff Member"),
                ],
                authenticationType: "Cookies");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
            };

            // act
            var result = _controller.GetMe();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result);   // returns 200 Ok
            var value = okResult.Value!;                            // anonymous object, read via reflection
            Assert.Equal("staff@example.com", value.GetType().GetProperty("Email")!.GetValue(value));
            Assert.Equal("Staff Member", value.GetType().GetProperty("Name")!.GetValue(value));
        }

        // GET: api/auth/verify
        [Fact]
        public void TestVerifyReturnsOk()
        {
            // act
            var result = _controller.Verify();

            // assert
            // reaching the action at all means the session was valid (the global
            // fallback policy rejects with 401 first otherwise), so it just says 200
            Assert.IsType<OkResult>(result);
        }
    }
}