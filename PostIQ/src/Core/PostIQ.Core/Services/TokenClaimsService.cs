#nullable enable
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PostIQ.Core.Services;

public sealed class TokenClaimsService(IHttpContextAccessor httpContextAccessor) : ITokenClaimsService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public IReadOnlyCollection<Claim> GetClaims() => User?.Claims.ToArray() ?? Array.Empty<Claim>();

    public string? GetClaimValue(string claimType) => User?.FindFirst(claimType)?.Value;

    public Guid? GetUserGuid()
    {
        var subject = GetClaimValue(ClaimTypes.NameIdentifier)
            ?? GetClaimValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(subject, out var userGuid) ? userGuid : null;
    }

    public string? GetEmail() => GetClaimValue(ClaimTypes.Email)
        ?? GetClaimValue(JwtRegisteredClaimNames.Email);
}