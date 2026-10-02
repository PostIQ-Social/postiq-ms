using MediatR;
using PostIQ.Core.Response;
using Published.Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Published.Application.Queries
{
    public record GetUserPostsQuery : IRequest<ListResponse<BatchRepoRes>>
    {
        public int PageNo { get; init; }
        public int PageSize { get; init; } = 50;
        public long UserId { get; init; }
    }
}
