using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using PostIQ.Identity.Contracts;
using PostIQ.Identity.Services;
using User.Application.Commands;
using User.Application.Queries;

namespace PostIQ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly AuthService _auth;
        private readonly ILogger<UserController> _logger;
        public UserController(AuthService auth, ILogger<UserController> logger)
        {
            _auth = auth;
            _logger = logger;
        }
        /// <summary>
        /// Registers a new user. Creates the auth user in the Identity service,
        /// stores the user profile and records the referral.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            if (command is null)
                return BadRequest(new { Error = "Request body is required." });

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var referralCode = command.ReferralCode?.Trim();
            if (string.IsNullOrWhiteSpace(referralCode))
            {
                return BadRequest(new { Error = "Referral code is required." });
            }

            try
            {
                var validReferralCodeCommand = new ValidateReferralQuery(referralCode);
                var isReferralValid = await Mediator.Send(validReferralCodeCommand, cancellationToken);

                if (!isReferralValid.Data)
                {
                    _logger.LogInformation("Invalid or already used referral code provided: {ReferralCode}", referralCode);
                    return BadRequest(new { Error = "Invalid or already used referral code." });
                }

                var req = new RegisterRequest
                {
                    Email = command.Email,
                    Password = command.Password,
                };

                var auth = await _auth.RegisterAsync(req, cancellationToken);

                if (!auth.Ok)
                {
                    var detail = auth.Error ?? "Registration failed.";
                    var statusCode = auth.Status != 0 ? auth.Status : StatusCodes.Status400BadRequest;
                    _logger.LogWarning("Auth.RegisterAsync failed for {Email}: {Detail}", command.Email, detail);
                    return Problem(detail: detail, statusCode: statusCode);
                }

                // Set the AuthId on the command
                command.AuthId = auth.Value;

                var result = await Mediator.Send(command, cancellationToken);

                if (result == null)
                {
                    _logger.LogError("Mediator returned null result for RegisterUserCommand for {Email}", command.Email);
                    return Problem(detail: "Registration failed.", statusCode: StatusCodes.Status500InternalServerError);
                }

                if (!result.IsValid)
                {
                    var error = result.Errors?.FirstOrDefault();
                    var statusCode = StatusCodes.Status400BadRequest;
                    _logger.LogInformation("User registration validation failed");
                    return Problem(detail: "User registration validation failed", statusCode: statusCode);
                }

                // Prefer CreatedAtAction when an Id is available
                var data = result.Data;
                return Ok(data);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Register operation was cancelled by the client.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "Request cancelled." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", command?.Email);
                return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
