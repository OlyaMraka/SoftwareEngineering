using AutoMapper;
using FluentResults;
using FluentValidation;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;

namespace KeyKeepers.BLL.Commands.Communities.Update;

public class UpdateCommunityHandler : IRequestHandler<UpdateCommunityCommand, Result<CommunityResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IValidator<UpdateCommunityCommand> validator;

    public UpdateCommunityHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IValidator<UpdateCommunityCommand> validatorObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
        validator = validatorObj;
    }

    public async Task<Result<CommunityResponseDto>> Handle(
        UpdateCommunityCommand request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            return Result.Fail<CommunityResponseDto>(result.Errors.First().ErrorMessage);
        }

        QueryOptions<Community> options = new QueryOptions<Community>
        {
            Filter = community => community.Id == request.RequestDto.CommunityId,
            AsNoTracking = false,
        };

        Community? entity = await repositoryWrapper.CommunityRepository.GetFirstOrDefaultAsync(options);

        if (entity == null)
        {
            return Result.Fail<CommunityResponseDto>(CommunityConstants.CommunityNotFoundError);
        }

        mapper.Map(request.RequestDto, entity);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<CommunityResponseDto>(CommunityConstants.DbSaveError);
        }

        CommunityResponseDto response = mapper.Map<CommunityResponseDto>(entity);

        return Result.Ok(response);
    }
}
