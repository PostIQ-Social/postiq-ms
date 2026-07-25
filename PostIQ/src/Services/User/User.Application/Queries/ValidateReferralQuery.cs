using MediatR;
using PostIQ.Core.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace User.Application.Queries
{
    public record ValidateReferralQuery(string code) : IRequest<SingleResponse<bool>>;
}
