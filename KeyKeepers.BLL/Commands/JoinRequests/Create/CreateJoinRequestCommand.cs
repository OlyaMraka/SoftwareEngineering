using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.JoinRequests;

namespace KeyKeepers.BLL.Commands.JoinRequests.Create;

public record CreateJoinRequestCommand(CreateRequestDto RequestDto)
    : IRequest<Result<JoinRequestResponseDto>>;
