using AutoMapper;
using FluentResults;
using FluentValidation;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;

namespace KeyKeepers.BLL.Commands.PasswordCategory.Create;

public class CreatePrivateCategoryHandler : IRequestHandler<CreatePrivateCategoryCommand, Result<PrivateCategoryResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IValidator<CreatePrivateCategoryCommand> validator;

    public CreatePrivateCategoryHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IValidator<CreatePrivateCategoryCommand> validatorObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
        validator = validatorObj;
    }

    public async Task<Result<PrivateCategoryResponseDto>> Handle(
        CreatePrivateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            PrivateCategory? existingCategory = await repositoryWrapper
                .PrivatePasswordCategoryRepository.GetFirstOrDefaultAsync(new QueryOptions<PrivateCategory>
                {
                    Filter = category => category.Name == request.RequestDto.Name,
                });

            if (existingCategory != null)
            {
                return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants
                    .CategoryAlreadyExistsErrorMessage);
            }

            PrivateCategory entity = mapper.Map<PrivateCategory>(request.RequestDto);
            entity.Community = null;
            entity.CommunityId = null;

            await repositoryWrapper.PrivatePasswordCategoryRepository.CreateAsync(entity);

            if (await repositoryWrapper.SaveChangesAsync() <= 0)
            {
                return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants.DbSaveErrorMessage);
            }

            PrivateCategoryResponseDto response = mapper.Map<PrivateCategoryResponseDto>(entity);

            return Result.Ok(response);
        }
        catch
        {
            return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants.ErrorMessage);
        }
    }
}
