using Home.Application.Response;
using Home.Core.Entities;
using MediatR;
using PostIQ.Core.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Application.Queries
{
    public record GetPostsQuery(int pageNo, long pageSize) : IRequest<ListResponse<PostResponse>>;
}
