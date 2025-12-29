using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using User.Application.Queries;
using User.Application.Response;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handllers
{
    public class GetUserByIdHandller(UnitOfWork<UserDBContext> _uow, IMapper _mapper) : IRequestHandler<GetUIserByIdQuery, SingleResponse<UserResponse>>
    {
        private readonly IRepositoryAsync<Users> _userAsync = _uow.GetRepositoryAsync<Users>();
        public async Task<SingleResponse<UserResponse>> Handle(GetUIserByIdQuery request, CancellationToken cancellationToken)
        {
            SingleResponse<UserResponse> response = new(null);
            var result = await _userAsync.SingleOrDefaultAsync(x => x.UserId == request.UserId && x.IsActive == true, null,
                                                                i => i.Include(x => x.UserDetail));
            response.Data = _mapper.Map<UserResponse>(result);
            return response;
        }
    }
}
