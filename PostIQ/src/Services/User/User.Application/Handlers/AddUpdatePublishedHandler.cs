using MediatR;
using Microsoft.Extensions.Logging;
using PostIQ.Core.Database;
using PostIQ.Core.HttpClientService.Services;
using PostIQ.Core.Response;
using User.Application.Commands;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers
{
    public class AddUpdatePublishedHandler : IRequestHandler<AddUpdatePublishedCommand, SingleResponse<long>>
    {
        private readonly IRepositoryAsync<Published> _publishedRepo;
        private readonly IUnitOfWork<UserDBContext> _uow;
        private readonly IBaseHttpClientService _httpClient;
        private readonly ILogger<AddUpdatePublishedHandler> _logger;

        private const string PublishedServiceClient = "PublishedService";
        private const string UpsertJobEndpoint = "api/RepoDetails/Job";

        public AddUpdatePublishedHandler(
            IUnitOfWork<UserDBContext> uow,
            IBaseHttpClientService httpClient,
            ILogger<AddUpdatePublishedHandler> logger)
        {
            _uow = uow;
            _publishedRepo = uow.GetRepositoryAsync<Published>();
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SingleResponse<long>> Handle(AddUpdatePublishedCommand request, CancellationToken cancellationToken)
        {
            // 1. Upsert the [User].[Published] record
            var existing = await _publishedRepo.SingleOrDefaultAsync(
                p => p.UserId == request.UserId && p.Source == request.Source);

            long publishedId;

            if (existing != null)
            {
                existing.BaseUrl = request.BaseUrl;
                existing.UpdatedOn = DateTime.UtcNow;
                existing.UpdatedBy = request.UserId;

                _uow.Commit();
                publishedId = existing.PublishedId;
            }
            else
            {
                var newPublished = new Published
                {
                    UserId = request.UserId,
                    Source = request.Source,
                    BaseUrl = request.BaseUrl,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.UserId
                };

                await _publishedRepo.InsertAsync(newPublished, cancellationToken);
                _uow.Commit();
                publishedId = newPublished.PublishedId;
            }

            // 2. Sync to Published service via HTTP
            await SyncToPublishedServiceAsync(publishedId, request, cancellationToken);

            return new SingleResponse<long>(publishedId);
        }

        private async Task SyncToPublishedServiceAsync(
            long publishedId,
            AddUpdatePublishedCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var body = new
                {
                    publishedId,
                    userId = request.UserId,
                    source = request.Source,
                    baseUrl = request.BaseUrl
                };

                var response = await _httpClient.PostAsync(
                    PublishedServiceClient,
                    UpsertJobEndpoint,
                    body,
                    cancellationToken: cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to sync Published {PublishedId} to Published service. Status: {StatusCode}",
                        publishedId, response.StatusCode);
                }
                else
                {
                    _logger.LogInformation(
                        "Successfully synced Published {PublishedId} to Published service",
                        publishedId);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail — the local record was saved successfully
                _logger.LogError(ex,
                    "Error syncing Published {PublishedId} to Published service", publishedId);
            }
        }
    }
}
