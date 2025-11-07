using MediatR;
using FuzzySharp;
using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Queries.Users.GetByUsername;

public class GetByUsernameHandler : IRequestHandler<GetByUsernameQuery, Result<IEnumerable<UserResponseDto>>>
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IMapper mapper;

    public GetByUsernameHandler(IRepositoryWrapper repositoryWrapperObj, IMapper mapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
        mapper = mapperObj;
    }

    public async Task<Result<IEnumerable<UserResponseDto>>> Handle(
        GetByUsernameQuery request,
        CancellationToken cancellationToken)
    {
        QueryOptions<User> options = new QueryOptions<User>
        {
            Filter = c => Fuzz.Ratio(c.UserName.ToLower(), request.Username.ToLower()) > 70,
        };

        var entities = await repositoryWrapper.UserRepository.GetAllAsync(options);

        if (!entities.Any())
        {
            return Result.Fail<IEnumerable<UserResponseDto>>(UserConstants.DataNotFound);
        }

        IEnumerable<UserResponseDto> response = mapper.Map<IEnumerable<UserResponseDto>>(entities);

        return Result.Ok(response);
    }
}
