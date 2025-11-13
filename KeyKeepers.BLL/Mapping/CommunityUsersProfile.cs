using AutoMapper;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.BLL.DTOs.CommunityUsers;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Mapping;

public class CommunityUsersProfile : Profile
{
    public CommunityUsersProfile()
    {
        CreateMap<CommunityUser, CommunityUserResponseDto>();
        CreateMap<CommunityUser, GetByUserIdResponseDto>()
            .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.Role));
    }
}
