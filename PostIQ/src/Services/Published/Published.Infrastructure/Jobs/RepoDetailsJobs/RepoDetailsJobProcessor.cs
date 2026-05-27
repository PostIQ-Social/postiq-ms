using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.AI.Analyzer;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.Database;
using PostIQ.Core.Shared.Enums;
using Published.Application.Services;
using Published.Core.Entities;
using Published.Core.Persistence;
using Published.Infrastructure.Analyzer;
using Published.Infrastructure.Providers;
using System.Text;
using System.Text.Json;

namespace Published.Infrastructure.Jobs.RepoDetailsJobs
{
    public class RepoDetailsJobProcessor : IJobItemProcessor<Repo>
    {
        private readonly IUnitOfWork<PublishDbContext> _uow;
        private readonly IRepositoryAsync<RepoDetail> _repoDetailAsync;
        private readonly IRepositoryAsync<Repo> _repoAsync;
        private readonly IRepositoryAsync<ProcessedPost> _processedPostAsync;
        private readonly ContentAnalyzer<RepositoryInfo, ContentAnalysisResult> _contentAnalysisService;
        private readonly IPostGenerationService _postGenerationService;

        public RepoDetailsJobProcessor(IServiceScopeFactory scopeFactory)
        {
            var scope = scopeFactory.CreateScope();
            _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<PublishDbContext>>();
            _repoDetailAsync = _uow.GetRepositoryAsync<RepoDetail>();
            _repoAsync = _uow.GetRepositoryAsync<Repo>();
            _processedPostAsync = _uow.GetRepositoryAsync<ProcessedPost>();
            _contentAnalysisService = scope.ServiceProvider.GetRequiredService<ContentAnalyzer<RepositoryInfo, ContentAnalysisResult>>();
            _postGenerationService = scope.ServiceProvider.GetRequiredService<IPostGenerationService>();
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

                // Extract and process post data
                var detailsDict = analysisResult.ToDictionary();
                var processedPost = await BuildAndSaveProcessedPostAsync(item, detailsDict, cancellationToken);

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

        private async Task<ProcessedPost> BuildAndSaveProcessedPostAsync(
            Repo repo,
            Dictionary<string, string> detailsDict,
            CancellationToken cancellationToken)
        {
            // Extract individual fields
            var headline = GetValueFromDict(detailsDict, "headline");
            var summary = GetValueFromDict(detailsDict, "summary");
            var takeaways = GetValueFromDict(detailsDict, "takeaways");
            var cta = GetValueFromDict(detailsDict, "cta");
            var hashtags = GetValueFromDict(detailsDict, "hashtags");
            var originalTitle = GetValueFromDict(detailsDict, "original_title");
            var originalAuthor = GetValueFromDict(detailsDict, "original_author");
            var originalBaseUrl = GetValueFromDict(detailsDict, "original_baseurl");
            var postedOnDetail = GetValueFromDict(detailsDict, "original_published_date");

            // Build the post content
            var postContent = BuildPost(headline, originalTitle, originalAuthor, summary, takeaways, cta, hashtags, originalBaseUrl);

            // Generate AI-powered catchy post
            var aiGeneratedPost = await _postGenerationService.GenerateCatchyPostAsync(
                headline, originalTitle, summary, takeaways, hashtags, cta, originalBaseUrl, cancellationToken);

            // Determine posted date
            DateTime postedOn = repo.PostedOn;
            if (!string.IsNullOrEmpty(postedOnDetail) && DateTime.TryParse(postedOnDetail, out var parsedDate))
            {
                postedOn = parsedDate;
            }

            // Create and save processed post
            var processedPost = new ProcessedPost
            {
                RepoId = repo.RepoId,
                Headline = headline,
                OriginalTitle = originalTitle,
                OriginalAuthor = originalAuthor,
                Summary = summary,
                Takeaways = takeaways,
                CTA = cta,
                Hashtags = hashtags,
                OriginalBaseUrl = originalBaseUrl,
                AutoGeneratedPost = postContent,
                AutoGeneratedPostByAI = aiGeneratedPost,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = repo.CreatedBy
            };

            await _processedPostAsync.InsertAsync(processedPost, cancellationToken);
            await _uow.CommitAsync();

            return processedPost;
        }

        private string? GetValueFromDict(Dictionary<string, string> dict, string key)
        {
            return dict.TryGetValue(key.ToLower(), out var value) ? value : null;
        }

        private string BuildPost(string? headline, string? originalTitle, string? originalAuthor, string? summary, string? takeaways, string? cta, string? hashtags, string? originalBaseUrl)
        {
            var postBuilder = new StringBuilder();

            // Add headline
            if (!string.IsNullOrWhiteSpace(headline))
            {
                postBuilder.AppendLine(headline);
                postBuilder.AppendLine();
            }

            // Add original title if different from headline
            if (!string.IsNullOrWhiteSpace(originalTitle) && originalTitle != headline)
            {
                postBuilder.AppendLine($"📰 {originalTitle}");
                if (!string.IsNullOrWhiteSpace(originalAuthor))
                {
                    postBuilder.AppendLine($"By {originalAuthor}");
                }
                postBuilder.AppendLine();
            }

            // Add summary
            if (!string.IsNullOrWhiteSpace(summary))
            {
                postBuilder.AppendLine(summary);
                postBuilder.AppendLine();
            }

            // Add takeaways
            if (!string.IsNullOrWhiteSpace(takeaways))
            {
                postBuilder.AppendLine("💡 Key Takeaways:");
                postBuilder.AppendLine(takeaways);
                postBuilder.AppendLine();
            }

            // Add call to action
            if (!string.IsNullOrWhiteSpace(cta))
            {
                postBuilder.AppendLine($"👉 {cta}");
                postBuilder.AppendLine();
            }

            // Add source URL
            if (!string.IsNullOrWhiteSpace(originalBaseUrl))
            {
                postBuilder.AppendLine($"Read more: {originalBaseUrl}");
                postBuilder.AppendLine();
            }

            // Add hashtags at the end
            if (!string.IsNullOrWhiteSpace(hashtags))
            {
                postBuilder.AppendLine(hashtags);
            }

            return postBuilder.ToString().Trim();
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
