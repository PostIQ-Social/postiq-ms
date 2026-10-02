using AutoMapper;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Commands;
using Published.Core.Entities;
using Published.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Published.Application.Handlers
{
    public class AddJobHandler : IRequestHandler<AddJobCommand, CreatedResponse<long>>
    {
        private readonly IRepositoryAsync<Job> _job;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<PublishDbContext> _uow;

        public AddJobHandler(
            IUnitOfWork<PublishDbContext> uow,
            IMapper mapper)
        {
            _uow = uow;
            _job = _uow.GetRepositoryAsync<Job>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<CreatedResponse<long>> Handle(AddJobCommand request, CancellationToken cancellationToken)
        {
            var source = request.Source.Trim();
            var normalizedSource = source.ToLowerInvariant();
            var sourceAlreadyAdded = await _uow.Context.Jobs.AnyAsync(
                job => job.UserId == request.UserId
                    && job.Source.Trim().ToLower() == normalizedSource,
                cancellationToken);

            if (sourceAlreadyAdded)
            {
                return new CreatedResponse<long>(
                    0,
                    new List<KeyValuePair<string, string[]>>
                    {
                        new("Source", ["This source has already been added to your profile."])
                    });
            }

            var entity = _mapper.Map<Job>(request);
            entity.Source = source;
            entity.CreatedBy = request.UserId;
            entity.CreatedOn = DateTime.UtcNow;
            entity.IsActive = true;

            await _job.InsertAsync(entity, cancellationToken);
            await _uow.CommitAsync();

            return new CreatedResponse<long>(entity.JobId);
        }
    }
}
