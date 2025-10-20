using MediatR;
using FluentResults;

namespace KeyKeepers.BLL.Commands.Passwords.Delete;

public record DeletePasswordCommand(long Id)
    : IRequest<Result<long>>;
