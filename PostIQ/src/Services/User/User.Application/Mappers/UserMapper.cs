using AutoMapper;
using User.Application.Response;
using User.Core.Entities;

namespace User.Application.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<Users, UserResponse>().ConstructUsing(src => new UserResponse(src.UserId, 
                                                                        src.UserDetail.FirstName, 
                                                                        src.UserDetail.LastName));
        }

    }
}
