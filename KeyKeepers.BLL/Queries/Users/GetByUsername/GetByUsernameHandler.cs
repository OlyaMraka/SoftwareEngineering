using MediatR;
using FuzzySharp;
using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Repositories.Interfaces.Base;

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
        var entities = await repositoryWrapper.UserRepository.GetAllAsync();

        var filtered = entities
            .ToList()
            .Where(c => Fuzz.Ratio(c.UserName.ToLower(), request.Username.ToLower()) > 70);

        IEnumerable<UserResponseDto> response = mapper.Map<IEnumerable<UserResponseDto>>(filtered);

        return Result.Ok(response);
    }
}
