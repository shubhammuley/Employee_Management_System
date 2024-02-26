using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.repositories.Contract;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController(IUserAccount accountInterface) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> CreateAsync(Register User)
        {
            if (User is null) return BadRequest("Model is empty");
            var result = await accountInterface.CreateAsync(User);
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> SignInAsync(Login user)
        {
            if (User is null) return BadRequest("Model is empty");
            var result = await accountInterface.SignInAsync(user);
            return Ok(result);  
        }

        [HttpPost("refresh-token")]

        public async Task<IActionResult> RefreshTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken is null) return BadRequest("Model is empty");
            var result = await accountInterface.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }
    }
}
