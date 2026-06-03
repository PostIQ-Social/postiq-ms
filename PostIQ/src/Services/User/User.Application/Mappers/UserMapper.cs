using AutoMapper;
using User.Application.Response;
using User.Core.Entities;

namespace User.Application.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<UserDetail, UserResponse>().ConstructUsing(src => new UserResponse(src.UserId, 
                                                                        src.FirstName, 
                                                                        src.LastName));
        }

    }
}
