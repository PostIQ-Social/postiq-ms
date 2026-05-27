using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostIQ.Core.BackgroundProcess.Configuration;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.BackgroundProcess.Services;
using Published.Core.Entities;

namespace Published.Infrastructure.Jobs.RepoJobs
{
    public class RepoJobHostedService : BaseBackgroundJobHostedService<Job>
    {
        public new const string JobName = "RepoJob";
        public RepoJobHostedService(
            IOptions<BackgroundJobsConfiguration> configuration, 
            ILogger<RepoJobHostedService> logger, 
            IJobItemsProducer<Job> producer, 
            IJobItemProcessor<Job> processor, 
            IBackgroundJobRegistry registry) : 
            base(JobName, configuration, logger, producer, processor, registry)
        {
        }
    }
}
