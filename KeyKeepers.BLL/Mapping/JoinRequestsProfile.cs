using AutoMapper;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Mapping;

public class JoinRequestsProfile : Profile
{
    public JoinRequestsProfile()
    {
        CreateMap<CreateRequestDto, JoinRequest>();
        CreateMap<JoinRequest, JoinRequestResponseDto>();
    }
}
