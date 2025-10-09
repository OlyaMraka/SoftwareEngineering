using FluentResults;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;

namespace KeyKeepers.BLL.Commands.Users.LogIn;

public record UserLogInCommand(UserLogInDto UserLogInDto)
    : IRequest<Result<UserLogInResponseDto>>;
