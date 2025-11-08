using MediatR;
using FluentResults;

using KeyKeepers.BLL.DTOs.JoinRequests;

namespace KeyKeepers.BLL.Queries.JoinRequests.GetByRecipientId;

public record GetByRecipientIdQuery(long RecipientId)
    : IRequest<Result<IEnumerable<JoinRequestResponseDto>>>;
