using MediatR;
using FluentResults;

namespace KeyKeepers.BLL.Commands.Communities.Delete;

public record DeleteCommunityCommand(long CommunityId)
    : IRequest<Result<long>>;
