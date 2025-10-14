using AutoMapper;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Mapping;

public class PasswordCategoryProfile : Profile
{
    public PasswordCategoryProfile()
    {
        CreateMap<CreatePrivateCategoryDto, PrivateCategory>();
        CreateMap<UpdatePrivateCategoryDto, PrivateCategory>();
        CreateMap<PrivateCategory, PrivateCategoryResponseDto>();
    }
}
