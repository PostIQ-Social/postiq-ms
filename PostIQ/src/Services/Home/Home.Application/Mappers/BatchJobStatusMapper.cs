using AutoMapper;
using Home.Application.Commands;
using Home.Application.Response;
using Home.Core.Entities;
using PostIQ.Core.Database.Entities;
using PostIQ.Core.Response;
using PostIQ.Core.Shared.Mappers;

namespace Home.Application.Mappers
{
    public class BatchJobStatusMapper : Profile
    {
        public BatchJobStatusMapper() 
        {
            CreateMap<UpsertBatchJobStatusCommand, BatchJobStatus>();
            CreateMap<BatchPostResponse, MergePostModel>();
            CreateMap<BatchJobStatus, LastBatchJobResponse>();       
            
        }
    }
}
