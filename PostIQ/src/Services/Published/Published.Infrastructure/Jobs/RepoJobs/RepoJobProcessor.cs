using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PostIQ.Core.BackgroundProcess.Configuration;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.Database;
using PostIQ.Core.Shared.Enums;
using Published.Core.Entities;
using Published.Core.Persistence;
using Published.Infrastructure.Providers;

namespace Published.Infrastructure.Jobs.RepoJobs
{
    public class RepoJobProcessor : IJobItemProcessor<Job>
    {
        private readonly IUnitOfWork<PublishDbContext> _uow;
        private readonly IRepositoryAsync<Repo> _repoAsync;
        private readonly IRepositoryAsync<Job> _jobAsync;
        private readonly IRepositoryProviderFactory _providerFactory;
        private readonly IOptions<BackgroundJobsConfiguration> _jobsConfiguration;

        public RepoJobProcessor(IServiceScopeFactory scopeFactory, IOptions<BackgroundJobsConfiguration> jobsConfiguration)
        {
            var scope = scopeFactory.CreateScope();
            _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<PublishDbContext>>();
            _repoAsync = _uow.GetRepositoryAsync<Repo>();
            _jobAsync = _uow.GetRepositoryAsync<Job>();
            _providerFactory = scope.ServiceProvider.GetRequiredService<IRepositoryProviderFactory>();
            _jobsConfiguration = jobsConfiguration;
        }

        public async Task ProcessItemAsync(Job item, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(item.Source) || string.IsNullOrEmpty(item.BaseUrl))
                {
                    return;
                }

                // Get the appropriate provider for the source
                var provider = _providerFactory.GetProvider(item.Source);

                // Fetch repositories/blogs from the source
                var repositories = await provider.FetchRepositoriesAsync(item.BaseUrl, cancellationToken);

                // Process each repository and add to database
                foreach (var repoInfo in repositories)
                {
                    await SaveRepositoryAsync(item, repoInfo, cancellationToken);
                }

                // Update job execution times
                await UpdateJobExecutionTimesAsync(item, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log the error
                throw new InvalidOperationException($"Failed to process job {item.JobId}", ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
        }

        private async Task SaveRepositoryAsync(Job job, RepositoryInfo repoInfo, CancellationToken cancellationToken)
        {
            try
            {
                // Check if repository already exists
                var existingRepo = await _repoAsync.SingleOrDefaultAsync(r => r.JobId == job.JobId && r.RepoUrl == repoInfo.Url);

                if (existingRepo != null)
                {
                    return; // Skip if already exists
                }

                // Create new Repo entity
                var repo = new Repo
                {
                    JobId = job.JobId,
                    PublishedId = job.PublishedId,
                    Source = job.Source,
                    RepoUrl = repoInfo.Url,
                    Status = Convert.ToInt16(StatusEnum.Pending),
                    IsActive = true,
                    PostedOn = repoInfo.PublishedDate ?? DateTime.UtcNow,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = job.CreatedBy,
                    MetaData = repoInfo != null ? System.Text.Json.JsonSerializer.Serialize(repoInfo) : string.Empty
                };

                await _repoAsync.InsertAsync(repo, cancellationToken);
                _uow.Commit();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save repository information", ex);
            }
        }

        private async Task UpdateJobExecutionTimesAsync(Job job, CancellationToken cancellationToken)
        {
            try
            {
                var currentDateTime = DateTime.UtcNow;
                var jobSettings = _jobsConfiguration.Value.GetJobSettings("RepoJob");
                var nextExecutionInterval = TimeSpan.FromMilliseconds(jobSettings.NextExecutionIntervalMs);

                // Fetch the job from the database to ensure it's tracked by the current context
                var trackedJob = await _jobAsync.SingleOrDefaultAsync(j => j.JobId == job.JobId);
                
                if (trackedJob != null)
                {
                    trackedJob.ExecutionStartTime = currentDateTime;
                    trackedJob.NextExecutionTime = currentDateTime.Add(nextExecutionInterval);
                    
                    _uow.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update job execution times for job {job.JobId}", ex);
            }
        }
    }
}
