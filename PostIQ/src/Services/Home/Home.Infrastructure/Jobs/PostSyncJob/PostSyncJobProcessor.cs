using AutoMapper;
using Home.Application.Commands;
using Home.Application.Queries;
using Home.Application.Response;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.HttpClientService.Extensions;
using PostIQ.Core.HttpClientService.Services;
using PostIQ.Core.Response;
using PostIQ.Core.Shared.Enums;


namespace Home.Infrastructure.Jobs.PostSyncJob
{
    public class PostSyncJobProcessor : IJobItemProcessor<LastBatchJobResponse>
    {
        private readonly ILogger<PostSyncJobProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBaseHttpClientService _clientService;

        public PostSyncJobProcessor(ILogger<PostSyncJobProcessor> logger, IServiceProvider serviceProvider, IBaseHttpClientService clientService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _clientService = clientService;
        }
        public async Task ProcessItemAsync(LastBatchJobResponse item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                return;

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            var executionStartedAt = DateTime.UtcNow;

            // Prefer LastId from the incoming item (first batch), otherwise fallback to job value
            var lastId = item.LastId;
            int batchSize = item.BatchSize;

            await ProcessBatchAsync(mediator, mapper, item, lastId, batchSize, executionStartedAt, cancellationToken);

            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
        }

        private async Task ProcessBatchAsync(IMediator mediator, IMapper mapper, LastBatchJobResponse item, long lastId, int batchSize, DateTime executionStartedAt, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _clientService.GetAsync("PublishedClient", $"RepoDetails/Batch?LastId={lastId}&BatchSize={batchSize}");
                var batchData = await response.ToObjectAsync<ListResponse<BatchPostResponse>>();

                var count = batchData?.Count ?? 0;
                var data = batchData?.Data ?? null;

                if (count <= 0 || data == null || data.Count == 0)
                {
                    _logger.LogDebug("No posts returned for LastId={LastId}", lastId);
                    return;
                }

                _logger.LogInformation("Processing batch of {Count} posts starting from ID {LastId}", count, lastId);

                var models = mapper.Map<List<MergePostModel>>(data);
                var upsertCommand = new MergePostCommand { Models = models };
                var upsertResult = await mediator.Send(upsertCommand, cancellationToken);

                var startId = data.FirstOrDefault()?.ProcessedPostId ?? 0;
                var endId = data.LastOrDefault()?.ProcessedPostId ?? 0;

                if(upsertResult == null || upsertResult.Data)
                {
                    var jobCommand = new UpsertBatchJobStatusCommand
                    {
                        BatchId = item.BatchId,
                        BatchSize = batchSize,
                        StartId = startId,
                        LastId = endId,
                        RecordCount = count,
                        ExecutionStartedAt = executionStartedAt,
                        ExecutionEndedAt = DateTime.UtcNow,
                        Status = StatusEnum.Succeeded.ToString()
                    };

                    await mediator.Send(jobCommand, cancellationToken);
                }

                var status = (upsertResult != null && upsertResult.Data) ? StatusEnum.Succeeded.ToString() : StatusEnum.Failed.ToString();

                if (status == StatusEnum.Succeeded.ToString())
                {
                    _logger.LogInformation("Successfully processed batch of {Count} posts (startId={StartId}, endId={EndId})", count, startId, endId);
                }
                else
                {
                    _logger.LogError("Failed to upsert posts for batch starting at {LastId}", lastId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch starting from LastId={LastId}", lastId);

                try
                {
                    var failCommand = new UpsertBatchJobStatusCommand
                    {
                        BatchId = item.BatchId,
                        BatchSize = batchSize,
                        StartId = item.StartId,
                        LastId = item.LastId,
                        RecordCount = 0,
                        ExecutionStartedAt = executionStartedAt,
                        ExecutionEndedAt = DateTime.UtcNow,
                        Status = StatusEnum.Failed.ToString()
                    };

                    await mediator.Send(failCommand, cancellationToken);
                }
                catch (Exception sendEx)
                {
                    _logger.LogError(sendEx, "Failed to record failed batch status for BatchId={BatchId}", item.BatchId);
                }
            }


        }
    }
}
