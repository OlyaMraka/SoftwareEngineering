using MediatR;
using FluentResults;
using AutoMapper;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Enums;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Commands.JoinRequests.Accept;

public class AcceptOrDeclineHandler : IRequestHandler<AcceptOrDeclineCommand, Result<JoinRequestResponseDto>>
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IMapper mapper;

    public AcceptOrDeclineHandler(IRepositoryWrapper repositoryWrapperObj, IMapper mapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
        mapper = mapperObj;
    }

    public async Task<Result<JoinRequestResponseDto>> Handle(
        AcceptOrDeclineCommand request,
        CancellationToken cancellationToken)
    {
        QueryOptions<JoinRequest> options = new QueryOptions<JoinRequest>
        {
            Filter = r => r.Id == request.RequestDto.Id,
            AsNoTracking = false,
        };

        JoinRequest? joinRequest = await repositoryWrapper.JoinRequestRepository.GetFirstOrDefaultAsync(options);

        if (joinRequest == null)
        {
            return Result.Fail<JoinRequestResponseDto>(JoinRequestConstants.RequestNotFound);
        }

        JoinRequestResponseDto response;

        if (request.RequestDto.Status == RequestStatus.Declined)
        {
            joinRequest.Status = RequestStatus.Declined;
            repositoryWrapper.JoinRequestRepository.Update(joinRequest);
            if (await repositoryWrapper.SaveChangesAsync() < 1)
            {
                return Result.Fail<JoinRequestResponseDto>(JoinRequestConstants.DbSaveError);
            }

            response = mapper.Map<JoinRequestResponseDto>(joinRequest);

            return Result.Ok(response);
        }

        joinRequest.Status = RequestStatus.Accepted;
        repositoryWrapper.JoinRequestRepository.Update(joinRequest);
        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<JoinRequestResponseDto>(JoinRequestConstants.DbSaveError);
        }

        CommunityUser newCommunityUser = new CommunityUser
        {
            Role = CommunityRole.Member,
            UserId = joinRequest.RecipientId,
            CommunityId = joinRequest.CommunityId,
        };

        await repositoryWrapper.CommunityUserRepository.CreateAsync(newCommunityUser);
        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<JoinRequestResponseDto>(JoinRequestConstants.DbSaveError);
        }

        response = mapper.Map<JoinRequestResponseDto>(joinRequest);

        return Result.Ok(response);
    }
}
