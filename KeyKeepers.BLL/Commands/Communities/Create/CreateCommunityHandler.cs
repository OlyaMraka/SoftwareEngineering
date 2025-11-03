using AutoMapper;
using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.CommunityUsers;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Enums;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Commands.Communities.Create;

public class CreateCommunityHandler : IRequestHandler<CreateCommunityCommand, Result<CreateCommunityResponseDto>>
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IMapper mapper;

    public CreateCommunityHandler(IRepositoryWrapper repositoryWrapperObj, IMapper mapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
        mapper = mapperObj;
    }

    public async Task<Result<CreateCommunityResponseDto>> Handle(
        CreateCommunityCommand request,
        CancellationToken cancellationToken)
    {
        QueryOptions<Community> options = new QueryOptions<Community>
        {
            Filter = c =>
                c.Name == request.RequestDto.Name
                && c.CommunityUsers.Any(x => x.UserId == request.RequestDto.OwnerId),
        };

        Community? community = await repositoryWrapper.CommunityRepository.GetFirstOrDefaultAsync(options);

        if (community != null)
        {
            return Result.Fail<CreateCommunityResponseDto>(CommunityConstants.AlreadyExistsError);
        }

        Community newCommunity = mapper.Map<Community>(request.RequestDto);

        await repositoryWrapper.CommunityRepository.CreateAsync(newCommunity);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<CreateCommunityResponseDto>(CommunityConstants.DbSaveError);
        }

        CommunityUser communityUser = new CommunityUser
        {
            UserId = request.RequestDto.OwnerId,
            Role = CommunityRole.Owner,
            CommunityId = newCommunity.Id,
        };

        await repositoryWrapper.CommunityUserRepository.CreateAsync(communityUser);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<CreateCommunityResponseDto>(CommunityConstants.DbSaveError);
        }

        CreateCommunityResponseDto response = new CreateCommunityResponseDto
        {
            NewCommunity = mapper.Map<CommunityResponseDto>(newCommunity),
            Owner = mapper.Map<CommunityUserResponseDto>(communityUser),
        };

        return Result.Ok(response);
    }
}
