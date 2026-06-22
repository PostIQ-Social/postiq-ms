using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Commands;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Application.Handlers
{
    public class UpsertJobCommandHandler : IRequestHandler<UpsertJobCommand, SingleResponse<long>>
    {
        private readonly IRepositoryAsync<Job> _jobRepo;
        private readonly IUnitOfWork<PublishDbContext> _uow;

        public UpsertJobCommandHandler(IUnitOfWork<PublishDbContext> uow)
        {
            _uow = uow;
            _jobRepo = uow.GetRepositoryAsync<Job>();
        }

        public async Task<SingleResponse<long>> Handle(UpsertJobCommand request, CancellationToken cancellationToken)
        {
            // Try to find an existing Job by PublishedId
            var existingJob = await _jobRepo.SingleOrDefaultAsync(
                j => j.PublishedId == request.PublishedId && j.IsActive);

            if (existingJob != null)
            {
                // Update existing job
                existingJob.Source = request.Source;
                existingJob.BaseUrl = request.BaseUrl;
                existingJob.UpdatedOn = DateTime.UtcNow;
                existingJob.UpdatedBy = request.UserId;

                _uow.Commit();

                return new SingleResponse<long>(existingJob.JobId);
            }
            else
            {
                // Create new job
                var newJob = new Job
                {
                    PublishedId = request.PublishedId,
                    UserId = request.UserId,
                    Source = request.Source,
                    BaseUrl = request.BaseUrl,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.UserId,
                    NextExecutionTime = DateTime.UtcNow // Trigger immediate processing
                };

                await _jobRepo.InsertAsync(newJob, cancellationToken);
                _uow.Commit();

                return new SingleResponse<long>(newJob.JobId);
            }
        }
    }
}
