using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SsoServer.Dtos.User;

namespace SsoServer.Models.Users;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationUser, UserBaseDto>();
        CreateMap<UserSubmitDto, ApplicationUser>()
            .ForMember(x => x.Email, dst => dst.MapFrom(x => x.UserName));
    }
}