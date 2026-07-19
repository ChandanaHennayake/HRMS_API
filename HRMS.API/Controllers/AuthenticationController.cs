using HRMS.API.DTOs.Authentication;
using HRMS.API.Service.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(
            IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result =
                await _authenticationService.LoginAsync(request);

            return Ok(new
            {
                Success = true,
                Message = "Login successful.",
                Data = result
            });
        }
    }
}