using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using User.Application.Queries;
using User.Application.Response;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers
{
    public class GetUserByIdHandler : IRequestHandler<GetUIserByIdQuery, SingleResponse<UserResponse>>
    {
        private readonly IRepositoryAsync<Users> _userAsync;
        private readonly IMapper _mapper;

        public GetUserByIdHandler(IUnitOfWork<UserDBContext> uow, IMapper mapper)
        {
            _userAsync = uow.GetRepositoryAsync<Users>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

#nullable disable

        public async Task<SingleResponse<UserResponse>> Handle(GetUIserByIdQuery request, CancellationToken cancellationToken)
        {
            var response = new SingleResponse<UserResponse>(null);
            var result = await _userAsync.SingleOrDefaultAsync(
                x => x.UserId == request.UserId && x.IsActive == true,
                null,
                i => i.Include(x => x.UserDetail));

            if (result == null)
            {
                response.Data = null;
                return response;
            }

            // Construct safely from possibly-null UserDetail
            response.Data = new UserResponse(
                result.UserId,
                result.UserDetail?.FirstName ?? string.Empty,
                result.UserDetail?.LastName ?? string.Empty);

            return response;
        }

#nullable restore
    }
}
