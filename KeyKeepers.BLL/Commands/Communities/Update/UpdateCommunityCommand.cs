using FluentResults;
using KeyKeepers.BLL.DTOs.Communities;
using MediatR;

namespace KeyKeepers.BLL.Commands.Communities.Update;

public record UpdateCommunityCommand(UpdateCommunityRequestDto RequestDto)
    : IRequest<Result<CommunityResponseDto>>;
