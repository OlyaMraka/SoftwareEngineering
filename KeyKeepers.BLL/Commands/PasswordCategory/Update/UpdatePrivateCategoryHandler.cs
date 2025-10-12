using AutoMapper;
using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Options;
using KeyKeepers.DAL.Repositories.Interfaces.Base;

namespace KeyKeepers.BLL.Commands.PasswordCategory.Update;

public class UpdatePrivateCategoryHandler : IRequestHandler<UpdatePrivateCategoryCommand, Result<PrivateCategoryResponseDto>>
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IMapper mapper;

    public UpdatePrivateCategoryHandler(IRepositoryWrapper repositoryWrapperObj, IMapper mapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
        mapper = mapperObj;
    }

    public async Task<Result<PrivateCategoryResponseDto>> Handle(
        UpdatePrivateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            PrivateCategory? existingCategory = await repositoryWrapper
                .PrivatePasswordCategoryRepository.GetFirstOrDefaultAsync(new QueryOptions<PrivateCategory>
                {
                    Filter = category => category.Id == request.RequestDto.Id,
                });

            if (existingCategory == null)
            {
                return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants
                    .CategoryNotFound);
            }

            mapper.Map(request, existingCategory);

            repositoryWrapper.PrivatePasswordCategoryRepository.Update(existingCategory);

            if (await repositoryWrapper.SaveChangesAsync() <= 0)
            {
                return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants.DbSaveErrorMessage);
            }

            PrivateCategoryResponseDto response = mapper.Map<PrivateCategoryResponseDto>(existingCategory);

            return Result.Ok(response);
        }
        catch
        {
            return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants.ErrorMessage);
        }
    }
}
