using Home.Core.Entities;
using MediatR;
using PostIQ.Core.Response;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Home.Application.Queries
{
    public record GetPostCountQuery(long[] PostId) : IRequest<ListResponse<PostsCount>>;
}
