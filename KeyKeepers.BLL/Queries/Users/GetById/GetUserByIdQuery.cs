using FluentResults;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;

namespace KeyKeepers.BLL.Queries.Users.GetById;

public record GetUserByIdQuery(long Id)
    : IRequest<Result<UserResponseDto>>;
