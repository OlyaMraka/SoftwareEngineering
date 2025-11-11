using FluentResults;
using KeyKeepers.BLL.DTOs.JoinRequests;
using MediatR;

namespace KeyKeepers.BLL.Commands.JoinRequests.Accept;

public record AcceptOrDeclineCommand(AcceptOrDeclineDto RequestDto)
    : IRequest<Result<JoinRequestResponseDto>>;
