using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.repositories.Contract;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserAccount accountInterface) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> CreateAsync(Register User)
        {
            if (User is null) return BadRequest("Model is empty");
            var result = await accountInterface.CreateAsync(User);
            return Ok(result);
        }
    }
}
