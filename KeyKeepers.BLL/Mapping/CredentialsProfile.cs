using AutoMapper;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Mapping;

public class CredentialsProfile : Profile
{
    public CredentialsProfile()
    {
        CreateMap<CreatePasswordRequest, Credentials>();
        CreateMap<Credentials, PasswordResponse>();
    }
}
