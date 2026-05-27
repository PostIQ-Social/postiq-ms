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
    public class PostSyncJobProducer : IJobItemsProducer<LastBatchJobResponse>
    {
        private readonly ILogger<PostSyncJobProducer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBaseHttpClientService _clientService;

        public PostSyncJobProducer(ILogger<PostSyncJobProducer> logger, IServiceProvider serviceProvider, IBaseHttpClientService clientService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _clientService = clientService;
        }
        public async Task<IReadOnlyList<LastBatchJobResponse>> GetItemsToProcessAsync(int maxItems, CancellationToken cancellationToken = default)
        {
            var result = new List<LastBatchJobResponse>();
            const int batchSize = 5000;
            var batchId = Guid.NewGuid();

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var job = await mediator.Send(new GetLastJobQuery() { Status = StatusEnum.Succeeded.ToString() }, cancellationToken);
            if (job.Data != null)
            {
                var lastId = job.Data?.LastId ?? 0L;
                var startId = job.Data?.StartId ?? 0L;

                // Request repo details (client call returns total available count)
                var response = await _clientService.GetAsync("PublishedClient", $"RepoDetails/Batch?LastId={lastId}&BatchSize={1}");
                var batchData = await response.ToObjectAsync<ListResponse<BatchPostResponse>>();

                // Total items available to process
                var totalItems = batchData?.Count ?? 0;

                // Compute number of batches using Math.Ceiling
                var batchCount = (int)Math.Ceiling(totalItems / (double)batchSize);

                if (batchCount <= 0)
                {
                    _logger.LogDebug("No items to process (totalItems={TotalItems})", totalItems);
                    return Array.Empty<LastBatchJobResponse>();
                }

                result.Add(new LastBatchJobResponse
                {
                    BatchId = batchId,
                    BatchSize = batchSize,
                    LastId = lastId,
                    StatusId = job.Data?.StatusId ?? 0L,
                    Status = job.Data?.Status,
                });

                //result = Enumerable.Range(0, batchCount)
                //    .Select(index => new LastBatchJobResponse
                //    {
                //        BatchId = batchId,
                //        BatchSize = batchSize,
                //    }).ToList();

                _logger.LogInformation("Produced {BatchCount} batch jobs (batchSize={BatchSize}, totalItems={TotalItems}, BatchId = {BatchId})", batchCount, batchSize, totalItems, batchId);

            }


            return result;
        }
    }
}
