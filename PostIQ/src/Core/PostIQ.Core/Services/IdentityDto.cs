namespace PostIQ.Core.Services;

public sealed record IdentityDto(long UserId, Guid AuthId, string? Email);