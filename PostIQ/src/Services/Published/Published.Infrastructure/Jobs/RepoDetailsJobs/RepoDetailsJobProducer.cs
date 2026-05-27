using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.Database;
using PostIQ.Core.Shared.Enums;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Infrastructure.Jobs.RepoDetailsJobs
{
    public class RepoDetailsJobProducer : IJobItemsProducer<Repo>
    {
        private IUnitOfWork<PublishDbContext> _uow;
        private IRepositoryReadOnlyAsync<Repo> _repoAsync;
        private readonly ILogger<RepoDetailsJobProducer> _logger;

        public RepoDetailsJobProducer(ILogger<RepoDetailsJobProducer> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            var scope = scopeFactory.CreateScope();
            _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<PublishDbContext>>();
            _repoAsync = _uow.GetReadOnlyRepositoryAsync<Repo>();

        }
        public async Task<IReadOnlyList<Repo>> GetItemsToProcessAsync(int maxItems, CancellationToken cancellationToken = default)
        {
            try
            {
                var reposToProcess = await _repoAsync.GetFilterListAsync(predicate: r => r.Status == Convert.ToInt16(StatusEnum.Pending), 
                    state: new PostIQ.Core.Database.Entities.FilterState { Take = maxItems });

                return reposToProcess.Data.Take(maxItems).ToList().AsReadOnly();
            }
            catch (Exception ex)
            {
                // Log the error - implement proper logging based on your logging infrastructure
                throw new InvalidOperationException("Failed to process pending jobs", ex);
            }
        }
    }
}
