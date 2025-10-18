using FluentResults;
using KeyKeepers.BLL.DTOs.Passwords;
using MediatR;

namespace KeyKeepers.BLL.Commands.Passwords.Create;

public record CreatePasswordCommand(CreatePasswordRequest Request)
    : IRequest<Result<PasswordResponse>>;
