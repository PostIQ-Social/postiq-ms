using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.AI.Analyzer;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.Database;
using PostIQ.Core.Shared.Enums;
using Published.Core.Entities;
using Published.Core.Persistence;
using Published.Infrastructure.Analyzer;
using Published.Infrastructure.Providers;
using System.Text.Json;

namespace Published.Infrastructure.Jobs.RepoDetailsJobs
{
    public class RepoDetailsJobProcessor : IJobItemProcessor<Repo>
    {
        private readonly IUnitOfWork<PublishDbContext> _uow;
        private readonly IRepositoryAsync<RepoDetail> _repoDetailAsync;
        private readonly IRepositoryAsync<Repo> _repoAsync;
        private readonly ContentAnalyzer<RepositoryInfo, ContentAnalysisResult> _contentAnalysisService;

        public RepoDetailsJobProcessor(IServiceScopeFactory scopeFactory)
        {
            var scope = scopeFactory.CreateScope();
            _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<PublishDbContext>>();
            _repoDetailAsync = _uow.GetRepositoryAsync<RepoDetail>();
            _repoAsync = _uow.GetRepositoryAsync<Repo>();
            _contentAnalysisService = scope.ServiceProvider.GetRequiredService<ContentAnalyzer<RepositoryInfo, ContentAnalysisResult>>();
        }

        public async Task ProcessItemAsync(Repo item, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(item.Source) || string.IsNullOrEmpty(item.RepoUrl))
                {
                    return;
                }

                // Check if already processed
                var existingDetails = await _repoDetailAsync.GetListAsync(
                    predicate: rd => rd.RepoId == item.RepoId,
                    cancellationToken: cancellationToken);

                if (existingDetails.Data.Any())
                {
                    return; // Already processed
                }

                var repositoryInfo = JsonSerializer.Deserialize<RepositoryInfo>(item.MetaData);

                if (repositoryInfo == null)
                {
                    return;
                }

                // Analyze content using AI service
                var analysisResult = await _contentAnalysisService.AnalyzeAsync(
                    repositoryInfo,
                    cancellationToken);

                // Save analyzed content to RepoDetail table
                await SaveRepoDetailsAsync(item, analysisResult.ToDictionary(), cancellationToken);

                await UpdateRepoStatusAsync(item, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process repository {item.RepoId}", ex);
            }

            await Task.Delay(1000, cancellationToken); // Simulate some processing delay
        }

        private async Task SaveRepoDetailsAsync(
        Repo repo,
        Dictionary<string, string> analyzedContent,
        CancellationToken cancellationToken)
        {
            var order = 1;

            foreach (var (key, value) in analyzedContent)
            {
                var detail = new RepoDetail
                {
                    RepoId = repo.RepoId,
                    Key = key,
                    Value = value,
                    Ordered = order++,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = repo.CreatedBy
                };

                await _repoDetailAsync.InsertAsync(detail, cancellationToken);
            }

            await _uow.CommitAsync();
        }

        private async Task UpdateRepoStatusAsync(Repo repo, CancellationToken cancellationToken)
        {
            try
            {

                // Fetch the job from the database to ensure it's tracked by the current context
                var trackedRepo = await _repoAsync.SingleOrDefaultAsync(j => j.RepoId == repo.RepoId);

                if (trackedRepo != null)
                {
                    trackedRepo.Status = Convert.ToInt16(StatusEnum.Succeeded);

                    await _uow.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update job execution times for job {repo.JobId}", ex);
            }
        }

    }
}
