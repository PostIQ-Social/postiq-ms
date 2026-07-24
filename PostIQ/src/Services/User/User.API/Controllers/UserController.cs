using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using User.Application.Commands;

namespace User.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        /// <summary>
        /// Registers a new user. Creates the auth user in the Identity service,
        /// stores the user profile and records the referral.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.IsValid)
            {
                var error = result.Errors.FirstOrDefault();
                var statusCode = int.TryParse(error.Key, out var parsed) ? parsed : 400;
                var detail = error.Value?.FirstOrDefault() ?? "Registration failed.";
                return Problem(detail: detail, statusCode: statusCode);
            }

            return Ok(result.Data);
        }
    }
}
