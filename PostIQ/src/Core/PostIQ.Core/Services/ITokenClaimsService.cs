#nullable enable
using System.Security.Claims;

namespace PostIQ.Core.Services;

public interface ITokenClaimsService
{
    IReadOnlyCollection<Claim> GetClaims();

    string? GetClaimValue(string claimType);

    Guid? GetUserGuid();

    string? GetEmail();
}