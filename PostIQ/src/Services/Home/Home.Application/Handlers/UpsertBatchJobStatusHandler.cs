using AutoMapper;
using Home.Application.Commands;
using Home.Core.Entities;
using Home.Core.Persistence;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Application.Handlers
{
    public class UpsertBatchJobStatusHandler : IRequestHandler<UpsertBatchJobStatusCommand, SingleResponse<bool>>
    {
        private readonly IRepositoryAsync<BatchJobStatus> _batchJobAsync;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<HomeDbContext> _uow;

        public UpsertBatchJobStatusHandler(IUnitOfWork<HomeDbContext> uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _batchJobAsync = _uow.GetRepositoryAsync<BatchJobStatus>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<SingleResponse<bool>> Handle(UpsertBatchJobStatusCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var now = DateTime.UtcNow;

            if (request.StatusId > 0)
            {
                var existing = await _batchJobAsync.SingleOrDefaultAsync(x => x.StatusId == request.StatusId);
                if (existing != null)
                {
                    existing.Status = request.Status;
                    existing.ExecutionEndedAt = request.ExecutionEndedAt;
                    existing.ExecutionStartedAt = request.ExecutionStartedAt;
                    existing.BatchSize = request.BatchSize;
                    existing.LastId = request.LastId;
                    existing.StartId = request.StartId;
                    existing.CreatedOn = now;

                    _uow.GetRepository<BatchJobStatus>().Update(existing);
                    var saved = await _uow.CommitAsync().ConfigureAwait(false);
                    return new SingleResponse<bool>(saved > 0);
                }
            }

            var entity = _mapper.Map<BatchJobStatus>(request);
            entity.CreatedOn = now;
            await _batchJobAsync.InsertAsync(entity).ConfigureAwait(false);
            var inserted = await _uow.CommitAsync().ConfigureAwait(false);
            return new SingleResponse<bool>(inserted > 0);
        }
    }
}
