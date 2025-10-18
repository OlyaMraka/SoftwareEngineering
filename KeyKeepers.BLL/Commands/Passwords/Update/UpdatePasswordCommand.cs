using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.Passwords;

namespace KeyKeepers.BLL.Commands.Passwords.Update;

public record UpdatePasswordCommand(UpdatePasswordRequest Request)
    : IRequest<Result<PasswordResponse>>;
