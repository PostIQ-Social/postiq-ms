using AutoMapper;
using Home.Application.Queries;
using Home.Application.Response;
using Home.Core.Entities;
using Home.Core.Persistence;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using PostIQ.Core.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Application.Handlers
{
    public class GetLastJobHandler : IRequestHandler<GetLastJobQuery, SingleResponse<LastBatchJobResponse>>
    {
        private readonly IRepositoryAsync<BatchJobStatus> _batchJob;
        private readonly IMapper _mapper;

        public GetLastJobHandler(IUnitOfWork<HomeDbContext> uow, IMapper mapper)
        {
            _batchJob = uow.GetRepositoryAsync<BatchJobStatus>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<SingleResponse<LastBatchJobResponse>> Handle(GetLastJobQuery request, CancellationToken cancellationToken)
        {
            try
            {
                //we are handling two state Succeed or Failed
                var response = new SingleResponse<LastBatchJobResponse>(null);

                //// failed job
                //var failedJob = await _batchJob.SingleOrDefaultAsync(x => x.Status == StatusEnum.Failed.ToString());
                //if (failedJob is not null)
                //{
                //    response.Data = _mapper.Map<LastBatchJobResponse>(failedJob);
                //    return response;
                //}

                // pending job
                var requestedJob = await _batchJob.SingleOrDefaultAsync(predicate: x => x.Status == request.Status, orderBy: o => o.OrderByDescending(x => x.StatusId));
                if (requestedJob is not null)
                {
                    response.Data = _mapper.Map<LastBatchJobResponse>(requestedJob);
                }

                // if there is no record in the table
                if (requestedJob is null)
                {
                    var anyJob = await _batchJob.SingleOrDefaultAsync();
                    if (anyJob is null)
                    {
                        response.Data = new LastBatchJobResponse
                        {
                            LastId = 0,
                            StatusId = 0,
                            Status = string.Empty,
                        };
                    }
                }

                return response;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
            
    }
}
