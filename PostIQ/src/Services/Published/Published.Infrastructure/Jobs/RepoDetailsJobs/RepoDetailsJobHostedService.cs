using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostIQ.Core.BackgroundProcess.Configuration;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.BackgroundProcess.Services;
using Published.Core.Entities;

namespace Published.Infrastructure.Jobs.RepoDetailsJobs
{
    public class RepoDetailsJobHostedService : BaseBackgroundJobHostedService<Repo>
    {
        public new const string JobName = "RepoDetailsJob";
        public RepoDetailsJobHostedService(
            IOptions<BackgroundJobsConfiguration> configuration, 
            ILogger<RepoDetailsJobHostedService> logger, 
            IJobItemsProducer<Repo> producer, 
            IJobItemProcessor<Repo> processor, 
            IBackgroundJobRegistry registry) : 
            base(JobName, configuration, logger, producer, processor, registry)
        {
        }
    }
}
