using AutoMapper;
using MediatR;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.EntityFrameworkCore;

namespace KeyKeepers.BLL.Commands.JoinRequests.Create;

public class CreateJoinRequestHandler : IRequestHandler<CreateJoinRequestCommand, Result<JoinRequestResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;

    public CreateJoinRequestHandler(IMapper mapperObj, IRepositoryWrapper repositoryWrapperObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<JoinRequestResponseDto>> Handle(
        CreateJoinRequestCommand request,
        CancellationToken cancellationToken)
    {
        QueryOptions<JoinRequest> options = new QueryOptions<JoinRequest>
        {
            Filter = r =>
                r.CommunityId == request.RequestDto.CommunityId &&
                r.RecipientId == request.RequestDto.RecipientId,
        };

        JoinRequest? entity = await repositoryWrapper.JoinRequestRepository.GetFirstOrDefaultAsync(options);

        if (entity != null)
        {
            return Result.Fail<JoinRequestResponseDto>(JoinRequestConstants.RequestAlreadyExistsError);
        }

        JoinRequest newEntity = mapper.Map<JoinRequest>(request);

        await repositoryWrapper.JoinRequestRepository.CreateAsync(newEntity);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<JoinRequestResponseDto>(JoinRequestConstants.DbSaveError);
        }

        JoinRequestResponseDto response = mapper.Map<JoinRequestResponseDto>(newEntity);

        QueryOptions<CommunityUser> communityUserOptions = new QueryOptions<CommunityUser>
        {
            Filter = x => x.Id == request.RequestDto.CommunityId,
            Include = x => x.Include(x => x.User),
        };

        CommunityUser? sensedInfo = await repositoryWrapper
            .CommunityUserRepository.GetFirstOrDefaultAsync(communityUserOptions);

        response.SenderUsername = sensedInfo!.User.UserName;

        return Result.Ok(response);
    }
}
