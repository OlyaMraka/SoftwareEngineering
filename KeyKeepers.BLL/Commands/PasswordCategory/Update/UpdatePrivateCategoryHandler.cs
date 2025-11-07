using AutoMapper;
using FluentValidation;
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
    private readonly IValidator<UpdatePrivateCategoryCommand> validator;
    private readonly IMapper mapper;

    public UpdatePrivateCategoryHandler(
        IRepositoryWrapper repositoryWrapperObj,
        IMapper mapperObj,
        IValidator<UpdatePrivateCategoryCommand> validatorObj)
    {
        repositoryWrapper = repositoryWrapperObj;
        mapper = mapperObj;
        validator = validatorObj;
    }

    public async Task<Result<PrivateCategoryResponseDto>> Handle(
        UpdatePrivateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Fail<PrivateCategoryResponseDto>(validationResult.Errors.First().ErrorMessage);
        }

        PrivateCategory? existingCategory = await repositoryWrapper
            .PrivatePasswordCategoryRepository.GetFirstOrDefaultAsync(new QueryOptions<PrivateCategory>
            {
                Filter = category => category.Id == request.RequestDto.Id,
                AsNoTracking = false,
            });

        if (existingCategory == null)
        {
            return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants
                .CategoryNotFound);
        }

        mapper.Map(request.RequestDto, existingCategory);

        repositoryWrapper.PrivatePasswordCategoryRepository.Update(existingCategory);

        if (await repositoryWrapper.SaveChangesAsync() <= 0)
        {
            return Result.Fail<PrivateCategoryResponseDto>(PasswordCategoriesConstants.DbSaveErrorMessage);
        }

        PrivateCategoryResponseDto response = mapper.Map<PrivateCategoryResponseDto>(existingCategory);

        return Result.Ok(response);
    }
}
