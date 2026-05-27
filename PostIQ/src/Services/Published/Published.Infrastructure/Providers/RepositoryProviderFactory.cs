using Microsoft.Extensions.DependencyInjection;

namespace Published.Infrastructure.Providers;

public interface IRepositoryProviderFactory
{
    IRepositoryProvider GetProvider(string source);
}

public class RepositoryProviderFactory : IRepositoryProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRepositoryProvider GetProvider(string source)
    {
        return source?.ToLowerInvariant() switch
        {
            "medium" => _serviceProvider.GetRequiredService<MediumRepositoryProvider>(),
            _ => throw new NotSupportedException($"Repository provider for source '{source}' is not supported")
        };
    }
}
