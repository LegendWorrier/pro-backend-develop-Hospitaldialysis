using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmartLoginController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly ISmartLoginService smartLogin;

        public SmartLoginController(IMapper mapper, ISmartLoginService smartLogin)
        {
            this.mapper = mapper;
            this.smartLogin = smartLogin;
        }

        [HttpPut("one-time-token")]
        public async Task<IActionResult> GenerateOneTimeToken()
        {
            var token = await smartLogin.GenerateOneTimeToken(User.GetUserIdAsGuid());

            return Ok(new { Token = token });
        }

        [HttpPost("generate-new")]
        public async Task<IActionResult> Generate([FromBody] LoginViewModel info)
        {
            var token = await smartLogin.GenerateTokenForUser(User.GetUserIdAsGuid(), info.Password);

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("The password is wrong.");
            }

            return Ok(new { Token = token });
        }

        [HttpDelete("revoke")]
        public async Task<IActionResult> RevokeTokenAsync()
        {
            var response = await smartLogin.RevokeAllTokenForUser(User.GetUserIdAsGuid());

            if (!response)
                return Ok(new { message = "No Token Found" });

            return Ok(new { message = "Token revoked" });
        }

    }
}
