using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;

namespace KeyKeepers.BLL.Queries.Users.GetById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoruWrapper;

    public GetUserByIdQueryHandler(IMapper mapperObj, IRepositoryWrapper repositoruWrapperObj)
    {
        mapper = mapperObj;
        repositoruWrapper = repositoruWrapperObj;
    }

    public async Task<Result<UserResponseDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        QueryOptions<User> options = new QueryOptions<User>
        {
            Filter = user => user.Id == request.Id,
        };

        User? user = await repositoruWrapper.UserRepository.GetFirstOrDefaultAsync(options);

        if (user == null)
        {
            return Result.Fail<UserResponseDto>(UserConstants.UserNotFound);
        }

        UserResponseDto response = mapper.Map<UserResponseDto>(user);

        return Result.Ok(response);
    }
}
