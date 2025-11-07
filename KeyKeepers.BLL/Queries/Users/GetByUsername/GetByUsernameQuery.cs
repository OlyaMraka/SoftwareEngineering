using FluentResults;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;

namespace KeyKeepers.BLL.Queries.Users.GetByUsername;

public record GetByUsernameQuery(string Username)
    : IRequest<Result<IEnumerable<UserResponseDto>>>;
