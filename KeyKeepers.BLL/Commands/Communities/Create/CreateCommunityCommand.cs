using KeyKeepers.BLL.DTOs.Communities;
using MediatR;
using FluentResults;

namespace KeyKeepers.BLL.Commands.Communities.Create;

public record CreateCommunityCommand(CreateCommunityDto RequestDto)
    : IRequest<Result<CreateCommunityResponseDto>>;
