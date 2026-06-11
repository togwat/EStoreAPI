using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace EStoreAPI.Server.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [AllowAnonymous]
        // GET: api/auth/login
        [HttpGet("login")]
        public ActionResult Login()
        {
            // redirect to google login, if success go to /
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Google");
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return NoContent();
        }

        // GET: api/auth/me
        [HttpGet("me")]
        public ActionResult GetMe()
        {
            // for React/axios to get google account email and name
            return Ok(new
            {
               Email = User.FindFirstValue(ClaimTypes.Email),
               Name = User.FindFirstValue(ClaimTypes.Name)  // account name from google
            });
        }

        // GET: api/auth/verify
        [HttpGet("verify")]
        public ActionResult Verify()
        {
            // for nginx auth_request, can trigger 401
            return Ok();
        }
    }
}