using MediatR;
using FluentResults;
using AutoMapper;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Queries.JoinRequests.GetByRecipientId;

public class GetByRecipientInHandler : IRequestHandler<GetByRecipientIdQuery, Result<IEnumerable<JoinRequestResponseDto>>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;

    public GetByRecipientInHandler(IMapper mapperObj, IRepositoryWrapper repositoryWrapperObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<IEnumerable<JoinRequestResponseDto>>> Handle(
        GetByRecipientIdQuery request,
        CancellationToken cancellationToken)
    {
        QueryOptions<JoinRequest> options = new QueryOptions<JoinRequest>
        {
            Filter = filter => filter.RecipientId == request.RecipientId,
        };

        var entities = await repositoryWrapper.JoinRequestRepository.GetAllAsync(options);

        if (!entities.Any())
        {
            return Result.Fail<IEnumerable<JoinRequestResponseDto>>(JoinRequestConstants.DataNotFoundError);
        }

        IEnumerable<JoinRequestResponseDto> response = mapper.Map<IEnumerable<JoinRequestResponseDto>>(entities);

        return Result.Ok(response);
    }
}
