using Home.Application.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostIQ.Core.BackgroundProcess.Configuration;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.BackgroundProcess.Services;

namespace Home.Infrastructure.Jobs.PostSyncJob
{
    public class PostSyncJobHostedService : BaseBackgroundJobHostedService<BatchPostResponse>
    {
        public new const string JobName = "PostSyncJob";
        public PostSyncJobHostedService(
            IOptions<BackgroundJobsConfiguration> configuration, 
            ILogger<PostSyncJobHostedService> logger, 
            IJobItemsProducer<BatchPostResponse> producer, 
            IJobItemProcessor<BatchPostResponse> processor, 
            IBackgroundJobRegistry registry) : 
            base(JobName, configuration, logger, producer, processor, registry)
        {
        }
    }
}
