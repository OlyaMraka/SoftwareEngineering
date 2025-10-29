using MediatR;
using FluentResults;
using KeyKeepers.BLL.DTOs.Passwords;

namespace KeyKeepers.BLL.Queries.Passwords.GetById;

public record GetByIdQuery(long Id)
    : IRequest<Result<PasswordResponse>>;
