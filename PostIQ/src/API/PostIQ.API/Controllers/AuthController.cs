using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostIQ.Identity.Contracts;
using PostIQ.Identity.Services;
using System.Security.Claims;

namespace PostIQ.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        [NonAction]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            var r = await _auth.RegisterAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Created("/api/auth/new", new { userId = r.Value });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var r = await _auth.LoginAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpPost("login/totp")]
        public async Task<IActionResult> LoginTotp([FromBody] LoginTotpRequest req, CancellationToken ct)
        {
            var r = await _auth.CompleteTotpLoginAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        {
            var r = await _auth.RefreshAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest req, CancellationToken ct)
        {
            var r = await _auth.RevokeAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll(CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.RevokeAllAsync(id.Value, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.GetProfileAsync(id.Value, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpPost("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.ChangePasswordAsync(id.Value, req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, CancellationToken ct)
        {
            var r = await _auth.ForgotPasswordAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req, CancellationToken ct)
        {
            var r = await _auth.ResetPasswordAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest req, CancellationToken ct)
        {
            var r = await _auth.ConfirmEmailAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest req, CancellationToken ct)
        {
            var r = await _auth.ResendConfirmationAsync(req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpPost("two-factor/setup")]
        public async Task<IActionResult> SetupTwoFactor(CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.SetupTwoFactorAsync(id.Value, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpPost("two-factor/enable")]
        public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest req, CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.EnableTotpAsync(id.Value, req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpPost("two-factor/disable")]
        public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest req, CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.DisableTotpAsync(id.Value, req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpPost("phone/request")]
        public async Task<IActionResult> RequestPhone([FromBody] RequestPhoneRequest req, CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.RequestPhoneVerificationAsync(id.Value, req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [Authorize]
        [HttpPost("phone/confirm")]
        public async Task<IActionResult> ConfirmPhone([FromBody] ConfirmPhoneRequest req, CancellationToken ct)
        {
            var id = RequireUserId();
            if (id is null) return Unauthorized();
            var r = await _auth.ConfirmPhoneAsync(id.Value, req, ct);
            if (!r.Ok) return Problem(detail: r.Error, statusCode: r.Status);
            return Ok(r.Value);
        }

        [HttpGet("external/providers")]
        public ActionResult<ExternalProvidersResponse> ExternalProviders()
        {
            return Ok(new ExternalProvidersResponse { Providers = Array.Empty<string>() });
        }

        private Guid? RequireUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return sub != null && Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
