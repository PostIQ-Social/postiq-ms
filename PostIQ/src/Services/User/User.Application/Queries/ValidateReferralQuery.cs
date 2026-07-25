using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace User.Application.Queries
{
    public record ValidateReferralQuery : IRequest<bool>
    {
        public string code { get; init; }
    }
}
