using AutoMapper;
using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.CommunityUsers;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyKeepers.BLL.Queries.CommunityUsers.GetByUserId;

public class GetByUserIdHandler : IRequestHandler<GetByUserIdQuery, Result<IEnumerable<GetByUserIdResponseDto>>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;

    public GetByUserIdHandler(IMapper mapperObj, IRepositoryWrapper repositoryWrapperObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<IEnumerable<GetByUserIdResponseDto>>> Handle(
        GetByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        QueryOptions<CommunityUser> options = new QueryOptions<CommunityUser>
        {
            Filter = x => x.Id == request.UserId,
            Include = x => x.Include(x => x.Community),
        };

        var entities = await repositoryWrapper.CommunityUserRepository.GetAllAsync(options);

        IEnumerable<GetByUserIdResponseDto> result = mapper.Map<IEnumerable<GetByUserIdResponseDto>>(entities);

        return Result.Ok(result);
    }
}
