using AutoMapper;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Mapping;

public class CommunityProfile : Profile
{
    public CommunityProfile()
    {
        CreateMap<Community, CommunityResponseDto>();
        CreateMap<CreateCommunityDto, Community>();
    }
}
