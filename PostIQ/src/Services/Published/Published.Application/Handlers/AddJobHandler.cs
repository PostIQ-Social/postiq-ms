using AutoMapper;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Commands;
using Published.Core.Entities;
using Published.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

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
            var entity = _mapper.Map<Job>(request);
            entity.CreatedBy = request.UserId;
            entity.CreatedOn = DateTime.UtcNow;
            entity.IsActive = true;

            await _job.InsertAsync(entity);
            await _uow.CommitAsync();

            return new CreatedResponse<long>(entity.JobId);
        }
    }
}
