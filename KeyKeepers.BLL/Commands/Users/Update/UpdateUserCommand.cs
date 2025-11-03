using KeyKeepers.BLL.DTOs.Users;
using MediatR;
using FluentResults;

namespace KeyKeepers.BLL.Commands.Users.Update;

public record UpdateUserCommand(UpdateUserDto RequestDto)
    : IRequest<Result<UserResponseDto>>;
