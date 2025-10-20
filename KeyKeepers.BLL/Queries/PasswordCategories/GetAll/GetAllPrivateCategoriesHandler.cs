using AutoMapper;
using MediatR;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Queries.PasswordCategories.GetAll;

public class GetAllPrivateCategoriesHandler
    : IRequestHandler<GetAllPasswordCategoriesQuery, Result<IEnumerable<PrivateCategoryResponseDto>>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;

    public GetAllPrivateCategoriesHandler(IMapper mapperObj, IRepositoryWrapper repositoryWrapperObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<IEnumerable<PrivateCategoryResponseDto>>> Handle(
        GetAllPasswordCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var filter = new QueryOptions<PrivateCategory>
            {
                Filter = filter => filter.UserId == request.UserId,
            };

            var entities = await repositoryWrapper.PrivatePasswordCategoryRepository.GetAllAsync(filter);
            return Result.Ok(mapper.Map<IEnumerable<PrivateCategoryResponseDto>>(entities));
        }
        catch
        {
            return Result.Fail<IEnumerable<PrivateCategoryResponseDto>>(PasswordCategoriesConstants.ErrorMessage);
        }
    }
}
