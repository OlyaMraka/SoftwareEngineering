using AutoMapper;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Mapping;

public class CredentialsProfile : Profile
{
    public CredentialsProfile()
    {
        CreateMap<CreatePasswordRequest, Credentials>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));
        CreateMap<Credentials, PasswordResponse>()
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash));
    }
}
