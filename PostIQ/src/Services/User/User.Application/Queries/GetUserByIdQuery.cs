using MediatR;
using PostIQ.Core.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Application.Response;

namespace User.Application.Queries
{
    public record GetUIserByIdQuery(long UserId) : IRequest<SingleResponse<UserResponse>>;
}
