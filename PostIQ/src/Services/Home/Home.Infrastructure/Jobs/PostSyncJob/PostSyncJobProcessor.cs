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
            var job = await mediator.Send(new GetLastJobQuery(), cancellationToken);

            // Prefer LastId from the incoming item (first batch), otherwise fallback to job value
            var lastId = item.LastId > 0 ? item.LastId : job.Data?.LastId ?? 0L;
            int batchSize = item.BatchSize;

            await ProcessBatchAsync(mediator, mapper, item, lastId, batchSize, executionStartedAt, cancellationToken);

            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
        }

        private async Task ProcessBatchAsync(IMediator mediator, IMapper mapper, LastBatchJobResponse item, long lastId, int batchSize, DateTime executionStartedAt, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _clientService.GetAsync("PublishedClient", $"RepoDetails/Batch?LastId={lastId}&BatchSize={batchSize}");
                var batchData = await response.ToResponseAsync<ListResponse<BatchPostResponse>>();

                var count = batchData?.Value?.Count ?? 0;
                var data = batchData?.Value?.Data;

                if (count <= 0 || data == null || data.Count == 0)
                {
                    _logger.LogDebug("No posts returned for LastId={LastId}", lastId);
                    return;
                }

                _logger.LogInformation("Processing batch of {Count} posts starting from ID {LastId}", count, lastId);

                var models = mapper.Map<List<MergePostModel>>(data);
                var upsertCommand = new MergePostCommand { Models = models };
                var upsertResult = await mediator.Send(upsertCommand, cancellationToken);

                var startId = data.FirstOrDefault()?.RepoDetailsId ?? 0;
                var endId = data.LastOrDefault()?.RepoDetailsId ?? 0;

                var status = (upsertResult != null && upsertResult.Data) ? StatusEnum.Succeeded.ToString() : StatusEnum.Failed.ToString();

                var jobCommand = new UpsertBatchJobStatusCommand
                {
                    BatchId = item.BatchId,
                    BatchSize = batchSize,
                    StartId = startId,
                    LastId = endId,
                    RecordCount = count,
                    ExecutionStartedAt = executionStartedAt,
                    ExecutionEndedAt = DateTime.UtcNow,
                    Status = status
                };

                await mediator.Send(jobCommand, cancellationToken);

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
                        StartId = 0,
                        LastId = 0,
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
