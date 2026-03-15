using AutoMapper;
using Home.Application.Commands;
using Home.Application.Response;
using Home.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Application.Mappers
{
    public class BatchJobStatusMapper : Profile
    {
        public BatchJobStatusMapper() 
        {
            CreateMap<UpsertBatchJobStatusCommand, BatchJobStatus>();
            CreateMap<MergePostCommand, Post>();
            CreateMap<BatchPostResponse, MergePostModel>();
        }
    }
}
