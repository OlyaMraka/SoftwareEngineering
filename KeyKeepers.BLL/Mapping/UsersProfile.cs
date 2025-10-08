using AutoMapper;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Mapping;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<UserRegisterDto, User>();
        CreateMap<User, UserResponseDto>();
    }
}
