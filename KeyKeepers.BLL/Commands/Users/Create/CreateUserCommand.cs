using FluentResults;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;

namespace KeyKeepers.BLL.Commands.Users.Create;

public record CreateUserCommand(UserRegisterDto RegisterDto)
    : IRequest<Result<AuthResponseDto>>;
