using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;
using User.Application.Commands;
using User.Core.IServices;
using User.Core.Entities;
using PostIQ.Core.Database;
using User.Core.Persistence;

namespace User.Application.BackgroundServices
{
    public class SyncWorker : BackgroundService
    {
        private readonly Channel<SyncContentCommand> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SyncWorker> _logger;

        public SyncWorker(
            Channel<SyncContentCommand> channel,
            IServiceProvider serviceProvider,
            ILogger<SyncWorker> logger)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var command in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var syncServices = scope.ServiceProvider.GetServices<ISyncService>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<UserDBContext>>();
                    var postRepo = uow.GetRepositoryAsync<Post>();

                    var service = syncServices.FirstOrDefault(s => s.SourceName.Equals(command.Source, StringComparison.OrdinalIgnoreCase));
                    
                    if (service == null)
                    {
                        _logger.LogWarning($"No sync service found for source: {command.Source}");
                        continue;
                    }

                    _logger.LogInformation($"Starting sync for user {command.UserId} from {command.Source} at {command.BaseUrl}");

                    // 1. Fetch Posts
                    var posts = await service.FetchPostsAsync(command.BaseUrl);

                    // 2. Save Posts
                    foreach (var post in posts)
                    {
                        post.UserId = command.UserId;
                        // Ideally check existence by ExternalId + UserId
                        // For simplicity, we just add. A real impl should upsert.
                        await postRepo.InsertAsync(post);
                    }

                    // 3. Commit changes (IMPORTANT for UoW)
                    await uow.CommitAsync();

                    _logger.LogInformation($"Synced {posts.Count} posts for user {command.UserId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error syncing content for user {command.UserId}");
                }
            }
        }
    }
}
