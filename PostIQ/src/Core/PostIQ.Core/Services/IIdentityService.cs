namespace PostIQ.Core.Services
{
    public interface IIdentityService
    {
        Task<IdentityDto?> GetIdentityAsync(CancellationToken cancellationToken = default);
    }
}
