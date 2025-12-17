using AutoMapper;
using User.Application.Response;
using User.Core.Entities;

namespace User.Application.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<Users, UserResponse>()
                .ForMember(dest => dest.FirstName, src => src.MapFrom(s => s.UserDetail.FirstName))
                .ForMember(dest => dest.LastName, src => src.MapFrom(s => s.UserDetail.LastName));
        }

    }
}
