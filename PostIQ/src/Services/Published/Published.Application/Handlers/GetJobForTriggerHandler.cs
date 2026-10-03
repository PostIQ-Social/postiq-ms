using AutoMapper;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Core.Entities;
using Published.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Published.Application.Handlers
{
    public class GetJobForTriggerHandler : IRequestHandler<GetJobForTriggerQuery, ListResponse<Job>>
    {
        private readonly IRepositoryAsync<Job> _job;
        private readonly IUnitOfWork<PublishDbContext> _uow;

        public GetJobForTriggerHandler(IUnitOfWork<PublishDbContext> uow)
        {
            _uow = uow;
            _job = _uow.GetRepositoryAsync<Job>();
        }

        public async Task<ListResponse<Job>> Handle(GetJobForTriggerQuery request, CancellationToken cancellationToken)
        {
            var jobs = await _job.GetListAsync(x => x.UserId == request.UserId && x.IsActive, cancellationToken: cancellationToken);

            return new ListResponse<Job> { Data = jobs.Data.ToList() };
        }
    }
}
