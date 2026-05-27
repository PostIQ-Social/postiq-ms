using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.Database;
using PostIQ.Core.Database.Entities;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Infrastructure.Jobs.RepoJobs
{
    public class RepoJobProducer : IJobItemsProducer<Job>
    {
        private IUnitOfWork<PublishDbContext> _uow;
        private IRepositoryReadOnlyAsync<Job> _jobAsync;
        private readonly ILogger<RepoJobProducer> _logger;

        public RepoJobProducer(ILogger<RepoJobProducer> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            var scope = scopeFactory.CreateScope();
            _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<PublishDbContext>>();
            _jobAsync = _uow.GetReadOnlyRepositoryAsync<Job>();

        }
        public async Task<IReadOnlyList<Job>> GetItemsToProcessAsync(int maxItems, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentDateTime = DateTime.UtcNow;
                var jobsToProcess = await _jobAsync.GetFilterListAsync(
                    predicate: j => j.NextExecutionTime.HasValue && j.NextExecutionTime.Value <= currentDateTime && j.IsActive,
                    state: new FilterState { Take = maxItems});

                 return jobsToProcess.Data.AsReadOnly();
            }
            catch (Exception ex)
            {
                // Log the error - implement proper logging based on your logging infrastructure
                throw new InvalidOperationException("Failed to process pending jobs", ex);
            }
        }
    }
}
