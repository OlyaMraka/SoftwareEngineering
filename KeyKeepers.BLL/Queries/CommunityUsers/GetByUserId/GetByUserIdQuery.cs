using FluentResults;
using MediatR;
using KeyKeepers.BLL.DTOs.CommunityUsers;

namespace KeyKeepers.BLL.Queries.CommunityUsers.GetByUserId;

public record GetByUserIdQuery(long UserId)
    : IRequest<Result<IEnumerable<GetByUserIdResponseDto>>>;
